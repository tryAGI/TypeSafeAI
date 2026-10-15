using System.Net;
using System.Text.Json;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    [DataRow(SystemOneProvider.Perplexity, "https://api.perplexity.ai/v1/decisions", "pplx-decider-v1.1-27b")]
    [DataRow(SystemOneProvider.Inception, "https://api.inceptionlabs.ai/v1/decisions", "mercury-decide")]
    [DataRow(SystemOneProvider.Liquid, "https://api.liquid.ai/decisions/v1/systemone", "d1")]
    [DataRow(SystemOneProvider.Wity, "https://api.wity.alphanimble.com/v1/systemone", "wity-1")]
    [DataRow(SystemOneProvider.SubQ, "https://api.subq.ai/v1/systemone", "subq-snap-preview")]
    [DataRow(SystemOneProvider.Sage, "https://sage.levanto.ai/v1/systemone", "sage-latest")]
    [DataRow(SystemOneProvider.Vansa, "https://api.vansa.org/v1/systemone", "vansa-3.4")]
    [DataRow(SystemOneProvider.Milliseconds, "https://api.milliseconds.ai/v1/systemone", "decision-machine-1")]
    [DataRow(SystemOneProvider.Instinct, "https://api.zoowork.ai/v1/systemone", "instinct")]
    [DataRow(SystemOneProvider.Classifier, "https://classifier.dev/v1/systemone", "jev")]
    [DataRow(SystemOneProvider.Bespoke, "https://api.bespokelabs.ai/v1/systemone", "nimble-latest")]
    [DataRow(SystemOneProvider.Surd, "https://api.simplexai.net/v1/systemone", "spx-cd-flash")]
    [DataRow(SystemOneProvider.Fastino, "https://api.fastino.ai/v1/systemone", "fastino/GLiDE")]
    [DataRow(SystemOneProvider.Amos, "https://api.amos.scira.ai/v1/systemone", "amos-1-large")]
    [DataRow(SystemOneProvider.MicrosoftDecision, "https://foundry.example/providers/microsoft/v1/systemone", "deployment")]
    [DataRow(SystemOneProvider.Compatible, "https://custom.example/prefix/v1/systemone", "custom")]
    public async Task HostedFactoriesPreserveMixedContractAndTransport(SystemOneProvider provider, string endpoint, string model)
    {
        var attempts = 0;
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            request.RequestUri!.AbsoluteUri.Should().Be(endpoint + "?trace=yes");
            request.Headers.Authorization!.Parameter.Should().Be("override");
            request.Headers.Contains("TypeSafe-Danger-Zone-Images").Should().BeFalse();
            request.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
            using var json = JsonDocument.Parse(await request.Content.ReadAsStringAsync(ct));
            json.RootElement.GetProperty("model").GetString().Should().Be(model);
            json.RootElement.GetProperty("state").GetProperty("text").GetString().Should().Be("fixture");
            json.RootElement.GetProperty("questions").GetProperty("matches").GetProperty("type").GetString().Should().Be("noul");
            json.RootElement.TryGetProperty("assets", out _).Should().BeFalse();
            return new HttpResponseMessage(++attempts == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)
            { Content = new StringContent(attempts == 1 ? "{}" : CelerisResponse) };
        }));
        using (var client = HostedClient(provider, http))
        {
            client.Provider.Should().Be(provider);
            var options = new RequestOptions { ApiKey = "override", Retry = new RetryPolicy { MaxAttempts = 2, InitialDelay = TimeSpan.Zero } };
            options.QueryParameters["trace"] = "yes";
            var response = await client.SystemOneAsync(JsonContent.Parse("""{"text":"fixture"}"""), CelerisQuestions(), requestOptions: options);
            response.Get<ChoiceAnswer>("team").Choice.Should().Be("billing");
            response.Get<NoulAnswer>("matches").Noul.Should().Be(0.95);
            response.Get<ScoreAnswer>("urgency").Score.Should().Be(0.6);
            attempts.Should().Be(2);
        }
        // The factory must not dispose a caller-owned HttpClient.
        http.DefaultRequestHeaders.Add("X-Still-Owned", "yes");
    }

    [TestMethod]
    [DataRow(SystemOneProvider.Perplexity)]
    [DataRow(SystemOneProvider.Inception)]
    [DataRow(SystemOneProvider.Liquid)]
    [DataRow(SystemOneProvider.Wity)]
    [DataRow(SystemOneProvider.SubQ)]
    [DataRow(SystemOneProvider.Classifier)]
    public async Task HostedImagesUseProviderWireShapes(SystemOneProvider provider)
    {
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            var root = json.RootElement;
            const string url = "data:image/png;base64,iVBORw==";
            if (provider is SystemOneProvider.Perplexity or SystemOneProvider.Inception)
            {
                root.GetProperty("state")[0].GetProperty("text").GetString().Should().Be("fixture");
                root.GetProperty("state")[1].GetProperty("text").GetString().Should().Be("Image named receipt:");
                root.GetProperty("state")[2].GetProperty("image_url").GetProperty("url").GetString().Should().Be(url);
            }
            else if (provider == SystemOneProvider.SubQ)
            {
                root.GetProperty("images")[0].GetProperty("mediaType").GetString().Should().Be("image/png");
                root.GetProperty("images")[0].GetProperty("base64").GetString().Should().Be("iVBORw==");
            }
            else if (provider == SystemOneProvider.Wity) root.GetProperty("image").GetString().Should().Be(url);
            else root.GetProperty("images")[0].GetString().Should().Be(url);
            root.TryGetProperty("assets", out _).Should().BeFalse();
            return CelerisOk();
        }));
        using var client = HostedClient(provider, http);
        var options = new RequestOptions();
        options.Images.Add("receipt", ImageAsset.FromBytes([137, 80, 78, 71], "png"));
        var response = await client.SystemOneAsync(JsonContent.Parse("""{"text":"fixture"}"""), CelerisQuestions(),
            model: provider == SystemOneProvider.Classifier ? "imajev-4b" : null, requestOptions: options);
        response.Get<NoulAnswer>("matches").Noul.Should().Be(0.95);
    }

    [TestMethod]
    [DataRow(SystemOneProvider.Sage)]
    [DataRow(SystemOneProvider.Vansa)]
    [DataRow(SystemOneProvider.Milliseconds)]
    [DataRow(SystemOneProvider.Instinct)]
    [DataRow(SystemOneProvider.MicrosoftDecision)]
    [DataRow(SystemOneProvider.Compatible)]
    public async Task TextProfilesRejectUnverifiedImagesAndBoxBeforeHttp(SystemOneProvider provider)
    {
        using var http = new HttpClient(new StubHandler((_, _) => throw new AssertFailedException("Invalid request reached HTTP.")));
        using var client = HostedClient(provider, http);
        var options = new RequestOptions();
        options.Images.Add("receipt", ImageAsset.FromBytes([137, 80, 78, 71], "png"));
        Func<Task> call = async () => await client.SystemOneAsync("fixture", CelerisQuestions(), requestOptions: options);
        await call.Should().ThrowAsync<NotSupportedException>();
        call = async () => await client.SystemOneAsync("fixture", new Dictionary<string, Question> { ["box"] = new BoxQuestion("target") });
        await call.Should().ThrowAsync<NotSupportedException>();
        call = async () => await client.SystemOneAsync("fixture", CelerisQuestions(), requestOptions: new RequestOptions { Celeris = new CelerisDecisionOptions() });
        await call.Should().ThrowAsync<NotSupportedException>();
    }

    [TestMethod]
    [DataRow(SystemOneProvider.Perplexity, 129)]
    [DataRow(SystemOneProvider.SubQ, 17)]
    [DataRow(SystemOneProvider.Vansa, 33)]
    [DataRow(SystemOneProvider.Milliseconds, 33)]
    public async Task HostedProfilesEnforceQuestionBounds(SystemOneProvider provider, int count)
    {
        using var http = new HttpClient(new StubHandler((_, _) => throw new AssertFailedException("Invalid request reached HTTP.")));
        using var client = HostedClient(provider, http);
        var questions = Enumerable.Range(0, count).ToDictionary(i => "q" + i, _ => (Question)new NoulQuestion("fixture"));
        Func<Task> call = async () => await client.SystemOneAsync("fixture", questions);
        await call.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    public async Task DecisionsPathPreservesCustomPrefixAndQuery()
    {
        using var http = new HttpClient(new StubHandler((request, _) =>
        {
            request.RequestUri!.AbsoluteUri.Should().Be("https://proxy.example/prefix/v1/decisions?a=b");
            return Task.FromResult(CelerisOk());
        }));
        using var client = CustomProviders.Inception("fixture", http, options => options.BaseUri = new Uri("https://proxy.example/prefix/"));
        var options = new RequestOptions();
        options.QueryParameters["a"] = "b";
        await client.SystemOneAsync("fixture", CelerisQuestions(), requestOptions: options);
    }

    private static TypeSafeClient HostedClient(SystemOneProvider provider, HttpClient http) => provider switch
    {
        SystemOneProvider.Perplexity => CustomProviders.Perplexity("fixture", http),
        SystemOneProvider.Inception => CustomProviders.Inception("fixture", http),
        SystemOneProvider.Liquid => CustomProviders.Liquid("fixture", http),
        SystemOneProvider.Wity => CustomProviders.Wity("fixture", http),
        SystemOneProvider.SubQ => CustomProviders.SubQ("fixture", http),
        SystemOneProvider.Sage => CustomProviders.Sage("fixture", http),
        SystemOneProvider.Vansa => CustomProviders.Vansa("fixture", http),
        SystemOneProvider.Milliseconds => CustomProviders.Milliseconds("fixture", http),
        SystemOneProvider.Instinct => CustomProviders.Instinct("fixture", http),
        SystemOneProvider.Classifier => CustomProviders.Classifier("fixture", http),
        SystemOneProvider.Bespoke => CustomProviders.Bespoke("fixture", http),
        SystemOneProvider.Surd => CustomProviders.Surd("fixture", http),
        SystemOneProvider.Fastino => CustomProviders.Fastino("fixture", http),
        SystemOneProvider.Amos => CustomProviders.Amos("fixture", http),
        SystemOneProvider.MicrosoftDecision => CustomProviders.MicrosoftDecision("fixture", new Uri("https://foundry.example"), "deployment", http),
        SystemOneProvider.Compatible => CustomProviders.SystemOne("fixture", new Uri("https://custom.example/prefix/"), "custom", http),
        _ => throw new ArgumentOutOfRangeException(nameof(provider)),
    };
}

public partial class Tests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task FastinoExpectedScoreIsConsistentAcrossTypedProjectionAndReplay(bool singleLevel)
    {
        const string json = """{"model":"glide","answers":{"urgency":{"type":"score","score":0,"expected_level":0.25,"confidence":0.5,"probabilities":{"0":0.75,"1":0.25},"legend":{"0":"Low","1":"High"}}},"usage":{"input_tokens":90,"output_tokens":1}}""";
        var payload = singleLevel ? json.Replace("0.25", "0", StringComparison.Ordinal).Replace("0.75", "1", StringComparison.Ordinal) : json;
        var expected = singleLevel ? 0 : 0.25;
        var attempts = 0;
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(++attempts == 1 ? (HttpStatusCode)425 : HttpStatusCode.OK)
        { Content = new StringContent(attempts == 1 ? "{}" : payload) })));
        using var client = CustomProviders.Fastino("fixture", http, options => options.Retry.InitialDelay = TimeSpan.Zero);
        var questions = new Dictionary<string, Question> { ["urgency"] = new ScoreQuestion(singleLevel ? ["Low"] : ["Low", "High"], "Urgency?") };
        var response = await client.SystemOneAsync("fixture", questions);
        response.Get<ScoreAnswer>("urgency").Score.Should().Be(expected);
        response.Get<ScoreAnswer>("urgency").RawJson.GetProperty("score").GetInt32().Should().Be(0);
        TypeSafeClient.FromHttpResponse(payload).Get<ScoreAnswer>("urgency").Score.Should().Be(expected);
        (await client.SystemOneAsync("fixture", questions, HostedProjectionContext.Default.HostedScoreProjection)).Urgency!.Score.Should().Be(expected);
        attempts.Should().Be(3);
    }

    [TestMethod]
    [DataRow(SystemOneProvider.Perplexity, 256, false)]
    [DataRow(SystemOneProvider.Inception, 11, true)]
    [DataRow(SystemOneProvider.Sage, 121, false)]
    [DataRow(SystemOneProvider.Vansa, 33, true)]
    [DataRow(SystemOneProvider.Milliseconds, 65, false)]
    [DataRow(SystemOneProvider.Instinct, 27, false)]
    [DataRow(SystemOneProvider.Fastino, 256, false)]
    public async Task HostedCriteriaBoundsFailBeforeHttp(SystemOneProvider provider, int count, bool score)
    {
        using var http = new HttpClient(new StubHandler((_, _) => throw new AssertFailedException("Invalid request reached HTTP.")));
        using var client = HostedClient(provider, http);
        Question question = score ? new ScoreQuestion(Enumerable.Range(0, count).Select(i => (JsonContent)("level" + i)), "Rate") :
            new ChoiceQuestion(Enumerable.Range(0, count).ToDictionary(i => "c" + i, _ => (JsonContent?)"description"), "Choose");
        Func<Task> call = async () => await client.SystemOneAsync("fixture", new Dictionary<string, Question> { ["q"] = question });
        await call.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    public async Task HostedImagesEnforceCountsBytesCollisionsAndEncodedBodyLimit()
    {
        using var http = new HttpClient(new StubHandler((_, _) => throw new AssertFailedException("Invalid request reached HTTP.")));
        using var client = CustomProviders.SubQ("fixture", http);
        var options = new RequestOptions();
        options.Images.Add("too-large", ImageAsset.FromBytes(new byte[4 * 1024 * 1024 + 1], "png"));
        Func<Task> call = async () => await client.SystemOneAsync("fixture", CelerisQuestions(), requestOptions: options);
        await call.Should().ThrowAsync<ArgumentException>();
        options.Images.Clear();
        foreach (var i in Enumerable.Range(0, 5)) options.Images.Add("image" + i, ImageAsset.FromBytes([1], "png"));
        await call.Should().ThrowAsync<NotSupportedException>();
        options.Images.Clear();
        options.Images.Add("receipt", ImageAsset.FromBytes([1], "png"));
        options.ExtraBody["images"] = JsonContent.Parse("[]").Value;
        await call.Should().ThrowAsync<ArgumentException>();
        using var vansa = CustomProviders.Vansa("fixture", http);
        call = async () => await vansa.SystemOneAsync(new string('x', 512_000), CelerisQuestions());
        await call.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    public async Task HostedDiscoverySupportsCompatibleEnvelopesAndRejectsUndocumentedRoutes()
    {
        using var http = new HttpClient(new StubHandler((request, _) =>
        {
            request.RequestUri!.AbsoluteUri.Should().Be("https://api.inceptionlabs.ai/v1/models");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"data":[{"id":"mercury-decide"}]}""") });
        }));
        using var inception = CustomProviders.Inception("fixture", http);
        (await inception.Models.ListAsync())[0].Name.Should().Be("mercury-decide");
        using var perplexity = CustomProviders.Perplexity("fixture", http);
        Func<Task> call = async () => await perplexity.Models.ListAsync();
        await call.Should().ThrowAsync<NotSupportedException>();
    }
}

internal sealed class HostedScoreProjection
{
    public ScoreAnswer? Urgency { get; set; }
}
[System.Text.Json.Serialization.JsonSourceGenerationOptions(PropertyNamingPolicy = System.Text.Json.Serialization.JsonKnownNamingPolicy.CamelCase)]
[System.Text.Json.Serialization.JsonSerializable(typeof(HostedScoreProjection))]
internal sealed partial class HostedProjectionContext : System.Text.Json.Serialization.JsonSerializerContext;
