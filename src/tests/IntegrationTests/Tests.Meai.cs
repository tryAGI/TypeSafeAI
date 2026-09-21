using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using TypeSafeAI.Extensions.AI;
using TypeSafeAI.Extensions.AI.Evaluation;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task InputGuardrailBlocksBeforeCallingChatModel()
    {
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"model":"test","answers":{"hazard":{"type":"noul","noul":0.99}},"usage":{"input_tokens":1,"output_tokens":0}}"""),
        })));
        using var client = new TypeSafeClient("test", http);
        var questions = new QuestionSet();
        var hazard = questions.AddNoul("hazard", new NoulQuestion());
        using var inner = new CountingChatClient();
        using var guard = new TypeSafeGuardrailChatClient(inner, client, new GuardrailOptions
        {
            InputQuestions = questions,
            DecideInput = assessment => assessment.Get(hazard).IsTrue() ? GuardrailDecision.Block("hazard") : GuardrailDecision.Allow,
            BlockedMessage = "blocked",
        });
        var response = await guard.GetResponseAsync([new ChatMessage(ChatRole.User, "synthetic input")]);
        response.Text.Should().Be("blocked");
        inner.Calls.Should().Be(0);
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in guard.GetStreamingResponseAsync([new ChatMessage(ChatRole.User, "synthetic input")])) updates.Add(update);
        updates.Should().NotBeEmpty();
        inner.Calls.Should().Be(0);
    }

    [TestMethod]
    public async Task EvaluatorDoesNotSwallowCallerCancellation()
    {
        using var client = new TypeSafeClient("test");
        var evaluator = new TypeSafeEvaluator(client, new Dictionary<string, Question> { ["q"] = new NoulQuestion() });
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var action = async () => await evaluator.EvaluateAsync([], new ChatResponse(new ChatMessage(ChatRole.Assistant, "test")), cancellationToken: cancellation.Token);
        await action.Should().ThrowAsync<TypeSafeUserAbortException>();
    }

    private sealed class CountingChatClient : IChatClient
    {
        public int Calls { get; private set; }
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "allowed")));
        }
        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            Calls++;
            yield return new ChatResponseUpdate(ChatRole.Assistant, "allowed");
        }
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
