using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task CompressionAndLongContextSurviveRetries(bool multipart)
    {
        var attempts = 0;
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            request.Content!.Headers.ContentEncoding.Should().Equal("gzip");
            request.Headers.GetValues("TypeSafe-Danger-Zone-Long-Context").Should().Equal("true");
            var compressed = await request.Content.ReadAsByteArrayAsync(ct);
            using var input = new MemoryStream(compressed);
            await using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var decoded = new MemoryStream();
            await gzip.CopyToAsync(decoded, ct);
            var text = Encoding.UTF8.GetString(decoded.ToArray());
            if (multipart)
            {
                request.Content.Headers.ContentType!.MediaType.Should().Be("multipart/form-data");
                text.Should().Contain("name=request").And.Contain("image/jpeg").And.Contain("unique-image-ref");
                text.Should().Contain("ABC");
            }
            else
            {
                request.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
                using var json = JsonDocument.Parse(text);
                json.RootElement.GetProperty("state").GetString()!.Length.Should().Be(2048);
            }
            return ++attempts == 1
                ? new HttpResponseMessage((HttpStatusCode)529) { Content = new StringContent("{}") }
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(BoxResponse) };
        }));
        using var client = new TypeSafeClient("test", http, new TypeSafeClientOptions
        {
            Compression = RequestCompression.Gzip,
            Retry = new RetryPolicy { InitialDelay = TimeSpan.Zero, MaximumDelay = TimeSpan.Zero },
        });
        var options = new RequestOptions { EnableLongContext = true };
        if (multipart) options.Images.Add("unique-image-ref", ImageAsset.FromBytes([65, 66, 67], "jpeg"));
        var state = multipart ? "unique-image-ref" + new string('x', 2048) : new string('x', 2048);
        var response = await client.SystemOneAsync(state, new Dictionary<string, Question> { ["visible"] = new NoulQuestion("Is it visible?") }, requestOptions: options);
        attempts.Should().Be(2);
        response.Metadata.RetryCount.Should().Be(1);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task SmallBodiesAndPerCallCompressionDisableStayUncompressed(bool disable)
    {
        using var http = new HttpClient(new StubHandler((request, _) =>
        {
            request.Content!.Headers.ContentEncoding.Should().BeEmpty();
            request.Headers.Contains("TypeSafe-Danger-Zone-Long-Context").Should().BeFalse();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(BoxResponse) });
        }));
        using var client = new TypeSafeClient("test", http, new TypeSafeClientOptions { Compression = RequestCompression.Gzip });
        await client.SystemOneAsync(disable ? new string('x', 2048) : "small",
            new Dictionary<string, Question> { ["visible"] = new NoulQuestion("visible?") },
            requestOptions: disable ? new RequestOptions { Compression = RequestCompression.None } : null);
    }

    [TestMethod]
    public async Task CriteriaFactoriesSupportTaxonomyWalkAndOrderedRubrics()
    {
        var taxonomy = JsonContent.Parse("""{"outdoors":{"cycling":{"bottles":null}},"kitchen":{"drinkware":["bottles","mugs"]}}""");
        var choice = ChoiceQuestion.FromCriteria(taxonomy, JsonContent.Parse("""{"question":"Which branch fits?","focus":["mount","use"]}"""));
        var score = ScoreQuestion.FromCriteria(JsonContent.Parse("""[{"summary":"small","examples":[1,null]},["medium",{"size":2}],null]"""));
        choice.Criteria["outdoors"]!.Value.Value.GetProperty("cycling").GetProperty("bottles").ValueKind.Should().Be(JsonValueKind.Null);
        score.Criteria.Count.Should().Be(3);
        score.Criteria[2].Value.ValueKind.Should().Be(JsonValueKind.Null);
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"model":"fixture","answers":{"branch":{"type":"choice","choice":"outdoors","confidence":0.8,"probabilities":{"outdoors":0.9,"kitchen":0.1}},"severity":{"type":"score","score":1.5,"confidence":0.5,"legend":{"0":"low","1":"medium","2":"high"},"probabilities":{"0":0,"1":0.5,"2":0.5}},"quality":{"type":"score","score":1,"confidence":1,"legend":{"0":"low","1":"high"},"probabilities":{"0":0,"1":1}},"present":{"type":"noul","noul":0.8}},"usage":{"input_tokens":1,"output_tokens":0}}"""),
        })));
        using var client = new TypeSafeClient("test", http);
        var response = await client.SystemOneAsync("a cage-mounted bottle", new Dictionary<string, Question>
        {
            ["branch"] = choice, ["severity"] = score, ["quality"] = score, ["present"] = new NoulQuestion("present?"),
        });
        var branch = response.Get<ChoiceAnswer>("branch");
        var children = ChoiceQuestion.FromCriteria(taxonomy.Value.GetProperty(branch.Choice));
        children.Criteria.Should().ContainKey("cycling");
        branch.Probability("outdoors").Should().Be(0.9);
        (branch.Confidence >= 0.7 ? branch.Choice : "review").Should().Be("outdoors");
        // Different scale lengths must be normalized before composition.
        var combined = 0.6 * response.Get<ScoreAnswer>("severity").Score / 2 + 0.4 * response.Get<ScoreAnswer>("quality").Score;
        combined.Should().BeApproximately(0.85, 1e-12);
        response.Get<NoulAnswer>("present").IsTrue(0.75).Should().BeTrue();
        var invalidChoice = () => ChoiceQuestion.FromCriteria("not an object");
        invalidChoice.Should().Throw<ArgumentException>();
        var invalidScore = () => ScoreQuestion.FromCriteria("not an array");
        invalidScore.Should().Throw<ArgumentException>();
    }
}
