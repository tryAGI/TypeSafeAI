#pragma warning disable MEAI001

using Microsoft.Extensions.AI;

namespace TypeSafeAI.Extensions.AI;

public sealed class TypeSafeRoutingContext
{
    public TypeSafeRoutingContext(QuestionSetResult result, RoutingContext routing, IChatClient defaultClient)
    {
        Result = result;
        Routing = routing;
        DefaultClient = defaultClient;
    }

    public QuestionSetResult Result { get; }
    public IEnumerable<ChatMessage> Messages => Routing.Messages;
    public ChatOptions? Options => Routing.ChatOptions;
    public RoutingContext Routing { get; }
    public IChatClient DefaultClient { get; }
    public TAnswer Get<TAnswer>(QuestionHandle<TAnswer> handle) where TAnswer : Answer => Result.Get(handle);
}

public sealed class RoutingOptions
{
    public Func<IReadOnlyList<ChatMessage>, JsonContent>? StateBuilder { get; set; }
    public RequestOptions? RequestOptions { get; set; }
    public string? Model { get; set; }
    public bool AnnotateResponses { get; set; } = true;
}

/// <summary>Calibrated Microsoft.Extensions.AI routing backed by TypeSafe System One.</summary>
public sealed class TypeSafeRoutingChatClient : RoutingChatClient
{
    public const string ResultPropertyName = "typesafe.routing";
    private readonly ITypeSafeClient _typeSafe;
    private readonly QuestionSet _questions;
    private readonly Func<TypeSafeRoutingContext, IChatClient?> _select;
    private readonly IChatClient _defaultClient;
    private readonly RoutingOptions _options;

    public TypeSafeRoutingChatClient(
        ITypeSafeClient typeSafeClient,
        QuestionSet questions,
        Func<TypeSafeRoutingContext, IChatClient?> select,
        IChatClient defaultClient,
        RoutingOptions? options = null)
    {
        _typeSafe = typeSafeClient ?? throw new ArgumentNullException(nameof(typeSafeClient));
        _questions = questions ?? throw new ArgumentNullException(nameof(questions));
        _select = select ?? throw new ArgumentNullException(nameof(select));
        _defaultClient = defaultClient ?? throw new ArgumentNullException(nameof(defaultClient));
        _options = options ?? new RoutingOptions();
    }

    protected override async ValueTask<IChatClient> SelectClientAsync(
        RoutingContext context,
        CancellationToken cancellationToken)
    {
        var conversation = context.Messages.ToArray();
        var state = _options.StateBuilder?.Invoke(conversation) ?? ChatState.FromMessages(conversation);
        var result = await _typeSafe.SystemOneAsync(
            state, _questions, _options.Model, _options.RequestOptions, cancellationToken).ConfigureAwait(false);
        var target = _select(new TypeSafeRoutingContext(result, context, _defaultClient)) ?? _defaultClient;
        return _options.AnnotateResponses ? new AnnotatingChatClient(target, result) : target;
    }

    private sealed class AnnotatingChatClient(IChatClient inner, QuestionSetResult result) : DelegatingChatClient(inner)
    {
        public override async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var response = await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
            response.AdditionalProperties ??= [];
            response.AdditionalProperties[ResultPropertyName] = result;
            return response;
        }

        protected override void Dispose(bool disposing) { }
    }
}
