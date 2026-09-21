using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace TypeSafeAI.Extensions.AI;

public enum GuardrailDirection { Input, Output }
public enum GuardrailAction { Allow, Review, Block }

public sealed record GuardrailDecision(GuardrailAction Action, string? Reason = null)
{
    public static GuardrailDecision Allow { get; } = new(GuardrailAction.Allow);
    public static GuardrailDecision Review(string reason) => new(GuardrailAction.Review, reason);
    public static GuardrailDecision Block(string reason) => new(GuardrailAction.Block, reason);
}

public sealed record GuardrailOutcome(
    GuardrailDirection Direction,
    GuardrailAction Action,
    string? Reason,
    string? RequestId);

public sealed class GuardrailAssessment(
    GuardrailDirection direction,
    QuestionSetResult result,
    IReadOnlyList<ChatMessage> messages,
    ChatResponse? response)
{
    public GuardrailDirection Direction { get; } = direction;
    public QuestionSetResult Result { get; } = result;
    public IReadOnlyList<ChatMessage> Messages { get; } = messages;
    public ChatResponse? Response { get; } = response;
    public TAnswer Get<TAnswer>(QuestionHandle<TAnswer> handle) where TAnswer : Answer => Result.Get(handle);
}

public sealed class GuardrailOptions
{
    public QuestionSet? InputQuestions { get; set; }
    public QuestionSet? OutputQuestions { get; set; }
    public Func<GuardrailAssessment, GuardrailDecision>? DecideInput { get; set; }
    public Func<GuardrailAssessment, GuardrailDecision>? DecideOutput { get; set; }
    public Func<GuardrailAssessment, GuardrailDecision>? Decide { get; set; }
    public string BlockedMessage { get; set; } = "I can't help with that request.";
    public bool ThrowWhenBlocked { get; set; }
    public bool GuardStreamingOutput { get; set; }
    public Func<IReadOnlyList<ChatMessage>, JsonContent>? InputStateBuilder { get; set; }
    public Func<IReadOnlyList<ChatMessage>, ChatResponse, JsonContent>? OutputStateBuilder { get; set; }
    public RequestOptions? RequestOptions { get; set; }
    public string? Model { get; set; }

    internal Func<GuardrailAssessment, GuardrailDecision> PolicyFor(GuardrailDirection direction) =>
        (direction == GuardrailDirection.Input ? DecideInput : DecideOutput) ?? Decide
        ?? throw new InvalidOperationException($"No policy is configured for the {direction} guardrail.");

    internal void Validate()
    {
        if (InputQuestions is null && OutputQuestions is null)
        {
            throw new InvalidOperationException("Configure input questions, output questions, or both.");
        }
        if (InputQuestions is not null) _ = PolicyFor(GuardrailDirection.Input);
        if (OutputQuestions is not null) _ = PolicyFor(GuardrailDirection.Output);
    }
}

public static class GuardrailPolicies
{
    public static Func<GuardrailAssessment, GuardrailDecision> Thresholds(
        IReadOnlyDictionary<QuestionHandle<NoulAnswer>, GuardrailAction> hazards,
        QuestionHandle<ScoreAnswer>? severity = null,
        double actionThreshold = 0.70,
        double reviewThreshold = 0.35,
        double severityBlock = 2.0)
    {
        ArgumentNullException.ThrowIfNull(hazards);
        if (reviewThreshold > actionThreshold)
        {
            throw new ArgumentException("Review threshold cannot exceed the action threshold.", nameof(reviewThreshold));
        }
        return assessment =>
        {
            var strongest = GuardrailAction.Allow;
            string? reason = null;
            var severe = severity is { } handle && assessment.Get(handle).Score >= severityBlock;
            foreach (var hazard in hazards)
            {
                var probability = assessment.Get(hazard.Key).Noul;
                if (probability < reviewThreshold) continue;
                var action = probability >= actionThreshold ? hazard.Value : GuardrailAction.Review;
                if (action == GuardrailAction.Review && severe) action = GuardrailAction.Block;
                if (action > strongest)
                {
                    strongest = action;
                    reason = $"{hazard.Key.Name} = {probability:F2}" + (severe ? " (severe)" : string.Empty);
                }
            }
            return new GuardrailDecision(strongest, reason);
        };
    }
}

public sealed class TypeSafeGuardrailException(GuardrailOutcome outcome)
    : TypeSafeException($"The {outcome.Direction.ToString().ToLowerInvariant()} guardrail blocked the message: {outcome.Reason ?? "no reason given"}")
{
    public GuardrailOutcome Outcome { get; } = outcome;
}

public sealed class TypeSafeGuardrailChatClient : DelegatingChatClient
{
    public const string OutcomePropertyName = "typesafe.guardrail";
    private readonly ITypeSafeClient _typeSafe;
    private readonly GuardrailOptions _options;

    public TypeSafeGuardrailChatClient(IChatClient innerClient, ITypeSafeClient typeSafeClient, GuardrailOptions options)
        : base(innerClient)
    {
        _typeSafe = typeSafeClient ?? throw new ArgumentNullException(nameof(typeSafeClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
    }

    public override Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var conversation = messages.ToArray();
        return GuardedAsync(conversation, () => base.GetResponseAsync(conversation, options, cancellationToken), cancellationToken);
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var conversation = messages.ToArray();
        if (_options.OutputQuestions is not null && _options.GuardStreamingOutput)
        {
            var guarded = await GuardedAsync(
                conversation,
                () => base.GetStreamingResponseAsync(conversation, options, cancellationToken).ToChatResponseAsync(cancellationToken),
                cancellationToken).ConfigureAwait(false);
            foreach (var update in guarded.ToChatResponseUpdates()) yield return update;
            yield break;
        }

        var input = await GuardAsync(GuardrailDirection.Input, conversation, null, cancellationToken).ConfigureAwait(false);
        if (input?.Action == GuardrailAction.Block)
        {
            foreach (var update in Blocked(input, [input]).ToChatResponseUpdates()) yield return update;
            yield break;
        }
        await foreach (var update in base.GetStreamingResponseAsync(conversation, options, cancellationToken).ConfigureAwait(false))
        {
            yield return update;
        }
    }

    private async Task<ChatResponse> GuardedAsync(
        IReadOnlyList<ChatMessage> conversation,
        Func<Task<ChatResponse>> inner,
        CancellationToken cancellationToken)
    {
        var outcomes = new List<GuardrailOutcome>(2);
        var input = await GuardAsync(GuardrailDirection.Input, conversation, null, cancellationToken).ConfigureAwait(false);
        if (input is not null)
        {
            outcomes.Add(input);
            if (input.Action == GuardrailAction.Block) return Blocked(input, outcomes);
        }
        var response = await inner().ConfigureAwait(false);
        var output = await GuardAsync(GuardrailDirection.Output, conversation, response, cancellationToken).ConfigureAwait(false);
        if (output is not null)
        {
            outcomes.Add(output);
            if (output.Action == GuardrailAction.Block) return Blocked(output, outcomes);
        }
        Annotate(response, outcomes);
        return response;
    }

    private async Task<GuardrailOutcome?> GuardAsync(
        GuardrailDirection direction,
        IReadOnlyList<ChatMessage> conversation,
        ChatResponse? response,
        CancellationToken cancellationToken)
    {
        var questions = direction == GuardrailDirection.Input ? _options.InputQuestions : _options.OutputQuestions;
        if (questions is null) return null;
        var state = direction == GuardrailDirection.Input
            ? _options.InputStateBuilder?.Invoke(conversation) ?? ChatState.FromMessages(conversation)
            : _options.OutputStateBuilder?.Invoke(conversation, response!) ?? ChatState.FromMessages(conversation, response);
        var result = await _typeSafe.SystemOneAsync(
            state, questions, _options.Model, _options.RequestOptions, cancellationToken).ConfigureAwait(false);
        var decision = _options.PolicyFor(direction)(new GuardrailAssessment(direction, result, conversation, response));
        return new GuardrailOutcome(direction, decision.Action, decision.Reason, result.Response.Metadata.RequestId);
    }

    private ChatResponse Blocked(GuardrailOutcome outcome, IReadOnlyList<GuardrailOutcome> outcomes)
    {
        if (_options.ThrowWhenBlocked) throw new TypeSafeGuardrailException(outcome);
        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, _options.BlockedMessage))
        {
            FinishReason = ChatFinishReason.ContentFilter,
        };
        Annotate(response, outcomes);
        return response;
    }

    private static void Annotate(ChatResponse response, IReadOnlyList<GuardrailOutcome> outcomes)
    {
        if (outcomes.Count == 0) return;
        response.AdditionalProperties ??= [];
        response.AdditionalProperties[OutcomePropertyName] = outcomes;
    }
}
