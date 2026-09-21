using System.Globalization;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace TypeSafeAI.Extensions.AI.Evaluation;

public enum NoulMetricKind { Numeric, Boolean }

public sealed class TypeSafeMetricContext(
    string questionId,
    string metricName,
    Question question,
    Answer answer,
    EvaluationMetric metric)
{
    public string QuestionId { get; } = questionId;
    public string MetricName { get; } = metricName;
    public Question Question { get; } = question;
    public Answer Answer { get; } = answer;
    public EvaluationMetric Metric { get; } = metric;
}

public sealed class TypeSafeEvaluatorOptions
{
    public RequestOptions? RequestOptions { get; set; }
    public string? Model { get; set; }
    public IDictionary<string, string> MetricNames { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IDictionary<string, NoulMetricKind> NoulMetricKinds { get; } = new Dictionary<string, NoulMetricKind>(StringComparer.Ordinal);
    public double NoulThreshold { get; set; } = 0.5;
    public bool IncludeProbabilities { get; set; } = true;
    public Func<TypeSafeMetricContext, EvaluationMetricInterpretation?>? Interpret { get; set; }
    public Func<IReadOnlyList<ChatMessage>, ChatResponse, JsonContent>? StateBuilder { get; set; }
}

/// <summary>Evaluates chat responses with calibrated TypeSafe judgments instead of an LLM judge.</summary>
public sealed class TypeSafeEvaluator : IEvaluator
{
    private const string MetadataPrefix = "typesafe.";
    private readonly ITypeSafeClient _client;
    private readonly IReadOnlyDictionary<string, Question> _questions;
    private readonly TypeSafeEvaluatorOptions _options;
    private readonly IReadOnlyDictionary<string, string> _names;

    public TypeSafeEvaluator(
        ITypeSafeClient client,
        IReadOnlyDictionary<string, Question> questions,
        TypeSafeEvaluatorOptions? options = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _questions = questions ?? throw new ArgumentNullException(nameof(questions));
        if (questions.Count == 0) throw new ArgumentException("At least one question is required.", nameof(questions));
        _options = options ?? new TypeSafeEvaluatorOptions();
        _names = questions.Keys.ToDictionary(
            id => id,
            id => _options.MetricNames.TryGetValue(id, out var custom) ? custom : id,
            StringComparer.Ordinal);
        if (_names.Values.Distinct(StringComparer.Ordinal).Count() != _names.Count)
        {
            throw new ArgumentException("Metric names must be unique.", nameof(options));
        }
        EvaluationMetricNames = _names.Values.ToArray();
    }

    public IReadOnlyCollection<string> EvaluationMetricNames { get; }

    public async ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse modelResponse,
        ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null,
        CancellationToken cancellationToken = default)
    {
        var conversation = messages.ToArray();
        var state = _options.StateBuilder?.Invoke(conversation, modelResponse)
            ?? ChatState.FromMessages(conversation, modelResponse);
        SystemOneResponse response;
        try
        {
            response = await _client.SystemOneAsync(
                state, _questions, _options.Model, _options.RequestOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (TypeSafeException exception)
        {
            return new EvaluationResult(_questions.Select(pair => Placeholder(pair.Key, pair.Value, exception.Message)));
        }

        var metrics = new List<EvaluationMetric>(_questions.Count);
        foreach (var pair in _questions)
        {
            if (!response.Answers.TryGetValue(pair.Key, out var answer))
            {
                metrics.Add(Placeholder(pair.Key, pair.Value, $"No answer was returned for '{pair.Key}'."));
                continue;
            }
            var metric = ToMetric(pair.Key, pair.Value, answer);
            metric.AddOrUpdateMetadata(MetadataPrefix + "model", response.Model);
            metric.AddOrUpdateMetadata(MetadataPrefix + "request_id", response.Metadata.RequestId ?? string.Empty);
            metric.AddOrUpdateMetadata(MetadataPrefix + "usage.input_tokens", response.Usage.InputTokens.ToString(CultureInfo.InvariantCulture));
            metric.Interpretation = _options.Interpret?.Invoke(new TypeSafeMetricContext(pair.Key, metric.Name, pair.Value, answer, metric));
            metrics.Add(metric);
        }
        var result = new EvaluationResult(metrics);
        if (additionalContext is not null) result.AddOrUpdateContextInAllMetrics(additionalContext);
        return result;
    }

    private EvaluationMetric CreateMetric(string id, Question question)
    {
        var name = _names[id];
        return question switch
        {
            NoulQuestion when _options.NoulMetricKinds.TryGetValue(id, out var kind) && kind == NoulMetricKind.Boolean => new BooleanMetric(name),
            NoulQuestion or ScoreQuestion => new NumericMetric(name),
            _ => new StringMetric(name),
        };
    }

    private EvaluationMetric Placeholder(string id, Question question, string error)
    {
        var metric = CreateMetric(id, question);
        metric.AddDiagnostics(EvaluationDiagnostic.Error(error));
        return metric;
    }

    private EvaluationMetric ToMetric(string id, Question question, Answer answer)
    {
        var metric = CreateMetric(id, question);
        switch (answer)
        {
            case NoulAnswer noul when metric is BooleanMetric boolean:
                boolean.Value = noul.Noul >= _options.NoulThreshold;
                metric.AddOrUpdateMetadata(MetadataPrefix + "probability", Format(noul.Noul));
                break;
            case NoulAnswer noul when metric is NumericMetric numeric:
                numeric.Value = noul.Noul;
                metric.AddOrUpdateMetadata(MetadataPrefix + "probability", Format(noul.Noul));
                break;
            case ChoiceAnswer choice when metric is StringMetric text:
                text.Value = choice.Choice;
                metric.AddOrUpdateMetadata(MetadataPrefix + "confidence", Format(choice.Confidence));
                AddProbabilities(metric, choice.Probabilities);
                break;
            case ScoreAnswer score when metric is NumericMetric numeric:
                numeric.Value = score.Score;
                metric.AddOrUpdateMetadata(MetadataPrefix + "confidence", Format(score.Confidence));
                metric.AddOrUpdateMetadata(MetadataPrefix + "most_likely", score.MostLikely.ToString(CultureInfo.InvariantCulture));
                AddProbabilities(metric, score.Probabilities);
                break;
            default:
                metric.AddDiagnostics(EvaluationDiagnostic.Warning($"Unexpected answer type '{answer.Type}'."));
                break;
        }
        return metric;
    }

    private void AddProbabilities(EvaluationMetric metric, IReadOnlyDictionary<string, double> probabilities)
    {
        if (!_options.IncludeProbabilities) return;
        foreach (var pair in probabilities)
        {
            metric.AddOrUpdateMetadata(MetadataPrefix + "p." + pair.Key, Format(pair.Value));
        }
    }

    private static string Format(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);
}

public static class TypeSafeInterpretations
{
    public static Func<TypeSafeMetricContext, EvaluationMetricInterpretation?> ScoreAtLeast(int level) => context =>
        context.Answer is ScoreAnswer score
            ? Create(Math.Clamp(score.Score / Math.Max(1, score.Legend.Count - 1), 0, 1), score.Score < level,
                score.Score < level ? $"Score {score.Score:F2} is below level {level}." : null)
            : null;

    public static Func<TypeSafeMetricContext, EvaluationMetricInterpretation?> NoulAtMost(double threshold) => context =>
        context.Answer is NoulAnswer noul
            ? Create(1 - noul.Noul, noul.Noul > threshold,
                noul.Noul > threshold ? $"Probability {noul.Noul:F2} exceeds {threshold:F2}." : null)
            : null;

    public static Func<TypeSafeMetricContext, EvaluationMetricInterpretation?> NoulAtLeast(double threshold) => context =>
        context.Answer is NoulAnswer noul
            ? Create(noul.Noul, noul.Noul < threshold,
                noul.Noul < threshold ? $"Probability {noul.Noul:F2} is below {threshold:F2}." : null)
            : null;

    public static Func<TypeSafeMetricContext, EvaluationMetricInterpretation?> ChoiceIn(params string[] passingLabels)
    {
        var passing = new HashSet<string>(passingLabels, StringComparer.Ordinal);
        return context => context.Answer is ChoiceAnswer choice
            ? Create(choice.Confidence, !passing.Contains(choice.Choice),
                passing.Contains(choice.Choice) ? null : $"Choice '{choice.Choice}' is not allowed.")
            : null;
    }

    private static EvaluationMetricInterpretation Create(double fraction, bool failed, string? reason) =>
        new(fraction switch
        {
            >= 0.9 => EvaluationRating.Exceptional,
            >= 0.7 => EvaluationRating.Good,
            >= 0.5 => EvaluationRating.Average,
            >= 0.3 => EvaluationRating.Poor,
            _ => EvaluationRating.Unacceptable,
        }, failed, reason);
}
