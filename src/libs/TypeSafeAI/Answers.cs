using System.Collections.ObjectModel;
using System.Text.Json;

namespace TypeSafeAI;

/// <summary>A System One answer.</summary>
public abstract record Answer(string Type)
{
    /// <summary>The exact server payload, retained for replay and forward compatibility.</summary>
    public JsonElement RawJson { get; init; }
}

/// <summary>A yes/true probability.</summary>
public sealed record NoulAnswer(double Noul) : Answer("noul")
{
    public bool IsTrue(double threshold = 0.5) => Noul >= threshold;
}

/// <summary>A selected choice and its full probability distribution.</summary>
public sealed record ChoiceAnswer(
    string Choice,
    double Confidence,
    IReadOnlyDictionary<string, double> Probabilities) : Answer("choice")
{
    public TEnum AsEnum<TEnum>(bool ignoreCase = true) where TEnum : struct, Enum =>
        Enum.Parse<TEnum>(Choice, ignoreCase);
}

/// <summary>An expected score, legend, and score-level probability distribution.</summary>
public sealed record ScoreAnswer(
    double Score,
    double Confidence,
    IReadOnlyDictionary<string, JsonElement> Legend,
    IReadOnlyDictionary<string, double> Probabilities) : Answer("score")
{
    public int MostLikely => Probabilities.Count == 0
        ? checked((int)Math.Round(Score, MidpointRounding.AwayFromZero))
        : int.Parse(Probabilities.MaxBy(static pair => pair.Value).Key, System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>An answer type added by the service after this SDK was published.</summary>
public sealed record RawAnswer(string Discriminator, JsonElement Value) : Answer(Discriminator);

/// <summary>Input and output token accounting.</summary>
public sealed record Usage(int InputTokens, int OutputTokens);

/// <summary>Answers keyed by the caller's question names.</summary>
public sealed class AnswerCollection : ReadOnlyDictionary<string, Answer>
{
    internal AnswerCollection(IDictionary<string, Answer> dictionary) : base(dictionary) { }

    public IReadOnlyDictionary<string, NoulAnswer> Nouls => OfType<NoulAnswer>();
    public IReadOnlyDictionary<string, ChoiceAnswer> Choices => OfType<ChoiceAnswer>();
    public IReadOnlyDictionary<string, ScoreAnswer> Scores => OfType<ScoreAnswer>();

    public TAnswer Get<TAnswer>(string name) where TAnswer : Answer =>
        this.TryGetValue(name, out var answer) && answer is TAnswer typed
            ? typed
            : throw new KeyNotFoundException($"Answer '{name}' is missing or is not a {typeof(TAnswer).Name}.");

    private IReadOnlyDictionary<string, TAnswer> OfType<TAnswer>() where TAnswer : Answer =>
        this.Where(static pair => pair.Value is TAnswer)
            .ToDictionary(static pair => pair.Key, static pair => (TAnswer)pair.Value, StringComparer.Ordinal);
}
