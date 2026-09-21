using System.Net;
using System.Text;
using System.Text.Json;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task GeneratedTransportPreservesKnownAndFutureUnionValues()
    {
        string? sent = null;
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            sent = await request.Content!.ReadAsStringAsync(ct);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"model":"test","answers":{"known":{"type":"noul","noul":0.9,"future":42},"next":{"type":"future","value":[1,2]}},"usage":{"input_tokens":4,"output_tokens":0},"extra":"kept"}
                    """, Encoding.UTF8, "application/json"),
            };
            response.Headers.Add("x-typesafe-request-id", "test-request");
            return response;
        }));
        using var client = new TypeSafeClient("test", http);
        var response = await client.SystemOneAsync("test", new Dictionary<string, Question>
        {
            ["known"] = new NoulQuestion("Is this a test?"),
            ["next"] = new RawQuestion(JsonContent.Parse("""{"type":"future","custom":true}""").Value),
        });
        response.Get<NoulAnswer>("known").Noul.Should().Be(0.9);
        response.Get<NoulAnswer>("known").RawJson.GetProperty("future").GetInt32().Should().Be(42);
        response.Get<RawAnswer>("next").Value.GetProperty("value").GetArrayLength().Should().Be(2);
        response.RawJson.GetProperty("extra").GetString().Should().Be("kept");
        response.Metadata.RequestId.Should().Be("test-request");
        using var body = JsonDocument.Parse(sent!);
        body.RootElement.GetProperty("questions").GetProperty("next").GetProperty("custom").GetBoolean().Should().BeTrue();
    }

    [TestMethod]
    public void EnumLabelsRespectWireNamesDescriptionsAndSignedOrdering()
    {
        EnumLabels<TestIntent>.AtIndex(0).Should().Be(TestIntent.Cancel);
        EnumLabels<TestIntent>.GetLabel(TestIntent.Play).Should().Be("play_music");
        EnumLabels<TestIntent>.GetDescription(TestIntent.Play).Should().Be("Play music");
        new ChoiceAnswer("play_music", 1, new Dictionary<string, double>()).AsEnum<TestIntent>().Should().Be(TestIntent.Play);
        ScoreQuestion.FromEnum<TestIntent>().Criteria[1].ToString().Should().Be("\"Play music\"");
    }

    [TestMethod]
    public void IntegrationsAreAvailableFromMainAssembly()
    {
        typeof(Extensions.AI.TypeSafeAIFunctions).Assembly.Should().Be(typeof(TypeSafeClient).Assembly);
        typeof(Extensions.AI.Evaluation.TypeSafeEvaluator).Assembly.Should().Be(typeof(TypeSafeClient).Assembly);
    }

    private enum TestIntent
    {
        Cancel = -3,
        [Label("play_music", Description = "Play music")]
        Play = 10,
    }

    [TestMethod]
    public async Task RetryPolicyHonorsConfiguredStatusAndReportsAttempts()
    {
        var attempts = 0;
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(
            Interlocked.Increment(ref attempts) == 1
                ? new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent("{}") }
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"model":"test","answers":{"q":{"type":"noul","noul":1}},"usage":{"input_tokens":1,"output_tokens":0}}""") })));
        var retry = new RetryPolicy { InitialDelay = TimeSpan.Zero, MaximumDelay = TimeSpan.Zero };
        retry.HttpStatuses.Add(HttpStatusCode.Conflict);
        using var client = new TypeSafeClient("test", http, new TypeSafeClientOptions { Retry = retry });
        var response = await client.SystemOneAsync("test", new Dictionary<string, Question> { ["q"] = new NoulQuestion() });
        attempts.Should().Be(2);
        response.Metadata.RetryCount.Should().Be(1);
    }

    [TestMethod]
    public async Task ModelsCancellationUsesTheSameExceptionContract()
    {
        using var http = new HttpClient(new StubHandler((_, ct) => Task.FromCanceled<HttpResponseMessage>(ct)));
        using var client = new TypeSafeClient("test", http);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var action = () => client.Models.ListAsync(cancellationToken: cancellation.Token);
        await action.Should().ThrowAsync<TypeSafeUserAbortException>();
    }

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request, cancellationToken);
    }
}
