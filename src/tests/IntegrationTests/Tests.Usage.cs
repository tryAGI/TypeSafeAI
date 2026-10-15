using System.Net;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    [DataRow("{\"input_tokens\":null,\"output_tokens\":null}", null, null)]
    [DataRow("{\"input_tokens\":17,\"output_tokens\":null}", 17, null)]
    [DataRow("{\"input_tokens\":null,\"output_tokens\":4}", null, 4)]
    [DataRow("{}", null, null)]
    public async Task UnknownTokenCountsRemainUnknownThroughTransportAndReplay(string usage, int? inputTokens, int? outputTokens)
    {
        var payload = "{\"model\":\"fixture\",\"answers\":{\"ok\":{\"type\":\"noul\",\"noul\":1}},\"usage\":" + usage + "}";
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload),
        })));
        using var client = new TypeSafeClient("test", http);
        var response = await client.SystemOneAsync("test", new Dictionary<string, Question> { ["ok"] = new NoulQuestion("test?") });
        response.Usage.Should().Be(new Usage(inputTokens, outputTokens));
        TypeSafeClient.FromHttpResponse(payload).Usage.Should().Be(response.Usage);
        response.RawJson.GetProperty("usage").GetRawText().Should().Be(usage);
    }
}
