namespace TypeSafeAI;

/// <summary>Routes state to a typed enum intent using a choice question.</summary>
public sealed class TypeSafeIntentRouter<TIntent>
    where TIntent : struct, Enum
{
    private readonly ITypeSafeClient _client;
    public TypeSafeIntentRouter(TypeSafeClient client, JsonContent? instructions = null)
        : this((ITypeSafeClient)client, instructions, null, null) { }

    public TypeSafeIntentRouter(ITypeSafeClient client, JsonContent? instructions = null,
        IEnumerable<string>? complexityLevels = null, JsonContent? complexityInstructions = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        Questions.Add("intent", ChoiceQuestion.FromEnum<TIntent>(instructions));
        if (complexityLevels is not null)
            Questions.Add("complexity", ScoreQuestion.FromValues(complexityLevels, complexityInstructions ?? "How difficult is this request to resolve?"));
    }

    public QuestionSet Questions { get; } = new();
    public double ConfidenceFloor { get; set; } = 0.5;
    public double ComplexityCeiling { get; set; } = 1;
    public double ComplexityConfidenceFloor { get; set; } = 0.5;
    public ISet<TIntent> CodeIntents { get; } = new HashSet<TIntent>();

    public async Task<TIntent> RouteAsync(
        JsonContent state,
        string? model = null,
        CancellationToken cancellationToken = default)
    {
        return (await AssessAsync(state, model, cancellationToken).ConfigureAwait(false)).Intent;
    }

    public async Task<IntentRoute<TIntent>> AssessAsync(JsonContent state, string? model = null, CancellationToken cancellationToken = default)
    {
        var response = await _client.SystemOneAsync(state, (IReadOnlyDictionary<string, Question>)Questions, model, cancellationToken: cancellationToken).ConfigureAwait(false);
        var intent = response.Get<ChoiceAnswer>("intent");
        var value = intent.AsEnum<TIntent>();
        var complexity = Questions.ContainsKey("complexity") ? response.Get<ScoreAnswer>("complexity") : null;
        var target = intent.Confidence < ConfidenceFloor || complexity is not null &&
            (complexity.Score > ComplexityCeiling || complexity.Confidence < ComplexityConfidenceFloor)
            ? RouteTarget.Human : CodeIntents.Contains(value) ? RouteTarget.Code : RouteTarget.Model;
        return new(value, intent, complexity, target, response);
    }
}

public enum RouteTarget { Code, Model, Human }
public sealed record IntentRoute<TIntent>(TIntent Intent, ChoiceAnswer IntentAnswer, ScoreAnswer? Complexity,
    RouteTarget Target, SystemOneResponse Result) where TIntent : struct, Enum
{
    public double Confidence => IntentAnswer.Confidence;
}
