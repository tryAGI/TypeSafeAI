using System.Net;
using System.Text.Json;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    [DataRow("\"plain text\"")]
    [DataRow("{\"field\":{\"values\":[1,true,null,\"非英語\",{\"amount\":1.25}]}}")]
    [DataRow("[\"check sender\",{\"compare\":[\"sender.name\",\"sender.email\"]}]")]
    [DataRow("null")]
    public async Task AdvancedStructurePreservesEveryEntryShapeAndStructuredScoreLegends(string json)
    {
        var entry = JsonContent.Parse(json);
        var questions = new Dictionary<string, Question>
        {
            ["noul"] = new NoulQuestion(entry, new NoulCriteria(entry, entry)),
            ["choice"] = new ChoiceQuestion(new Dictionary<string, JsonContent?> { ["structured"] = entry, ["bare"] = null }, entry),
            ["score"] = new ScoreQuestion([entry, "another level"], entry),
            ["box"] = new BoxQuestion(entry),
        };
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            var sent = body.RootElement.GetProperty("questions");
            foreach (var name in questions.Keys)
                JsonElement.DeepEquals(sent.GetProperty(name).GetProperty("instructions"), entry.Value).Should().BeTrue(name);
            var criteria = sent.GetProperty("noul").GetProperty("criteria");
            JsonElement.DeepEquals(criteria.GetProperty("false"), entry.Value).Should().BeTrue();
            JsonElement.DeepEquals(criteria.GetProperty("true"), entry.Value).Should().BeTrue();
            JsonElement.DeepEquals(sent.GetProperty("choice").GetProperty("criteria").GetProperty("structured"), entry.Value).Should().BeTrue();
            sent.GetProperty("choice").GetProperty("criteria").GetProperty("bare").ValueKind.Should().Be(JsonValueKind.Null);
            JsonElement.DeepEquals(sent.GetProperty("score").GetProperty("criteria")[0], entry.Value).Should().BeTrue();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($$$$"""
                    {"model":"fixture","answers":{"score":{"type":"score","score":0.25,"confidence":0.5,"legend":{"0":{{{{json}}}},"1":"another level"},"probabilities":{"0":0.75,"1":0.25}}},"usage":{"input_tokens":1,"output_tokens":0}}
                    """),
            };
        }));
        using var client = new TypeSafeClient("test", http);
        var response = await client.SystemOneAsync(JsonContent.Parse("""{"events":[{"amount":1.25,"enabled":true,"missing":null}]}"""), questions);
        var score = response.Get<ScoreAnswer>("score");
        JsonElement.DeepEquals(score.Legend["0"], entry.Value).Should().BeTrue();
        var replay = TypeSafeClient.FromHttpResponse(response.RawJson.GetRawText());
        JsonElement.DeepEquals(replay.Get<ScoreAnswer>("score").Legend["0"], entry.Value).Should().BeTrue();
    }

    [TestMethod]
    public async Task SixtyFourLevelsRemainValidInsideRequestAndResponseEnvelopes()
    {
        var json = new string('[', 64) + "\"leaf\"" + new string(']', 64);
        var nested = JsonContent.Parse(json);
        var levelJson = new string('[', 63) + "\"leaf\"" + new string(']', 63);
        var level = JsonContent.Parse(levelJson);
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct), new JsonDocumentOptions { MaxDepth = 80 });
            JsonElement.DeepEquals(body.RootElement.GetProperty("state"), nested.Value).Should().BeTrue();
            var question = body.RootElement.GetProperty("questions").GetProperty("q");
            JsonElement.DeepEquals(question.GetProperty("instructions"), nested.Value).Should().BeTrue();
            JsonElement.DeepEquals(question.GetProperty("criteria")[0], level.Value).Should().BeTrue();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($$$$"""
                    {"model":"fixture","answers":{"q":{"type":"score","score":0,"confidence":1,"legend":{"0":{{{{levelJson}}}},"1":"other"},"probabilities":{"0":1,"1":0}}},"usage":{"input_tokens":1,"output_tokens":0}}
                    """),
            };
        }));
        using var client = new TypeSafeClient("test", http);
        var response = await client.SystemOneAsync(nested, new Dictionary<string, Question>
        {
            ["q"] = new ScoreQuestion([level, "other"], nested),
        });
        JsonElement.DeepEquals(response.Get<ScoreAnswer>("q").Legend["0"], level.Value).Should().BeTrue();
        var replay = TypeSafeClient.FromHttpResponse(response.RawJson.GetRawText());
        JsonElement.DeepEquals(replay.Get<ScoreAnswer>("q").Legend["0"], level.Value).Should().BeTrue();
    }
}
