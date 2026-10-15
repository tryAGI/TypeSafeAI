using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TypeSafeAI.Extensions.AI;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    private const string CelerisResponse = """
        {"model":"celeris-1-decision","answers":{
          "matches":{"type":"noul","noul":0.95},
          "team":{"type":"choice","choice":"billing","confidence":0.94,"probabilities":{"billing":0.97,"technical":0.03}},
          "urgency":{"type":"score","score":0.6,"confidence":0.2,"probabilities":{"0":0.4,"1":0.6},"legend":{"0":"Calm","1":"Urgent"}}},
          "usage":{"input_tokens":122,"output_tokens":15},
          "x_celeris":{"explanations":{"matches":"The totals match.","team":"The ticket concerns a payment.","urgency":"A prompt reply would help."},"future":{"kept":true}}}
        """;

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task CelerisImagesAndMixedAnswersUseSystemOneContract(bool useBase64Images)
    {
        var options = new RequestOptions
        {
            UseBase64Images = useBase64Images,
            Celeris = new CelerisDecisionOptions { Explain = true, ImageResolution = CelerisImageResolution.Tokens560 },
        };
        options.Images.Add("receipt", ImageAsset.FromBytes([137, 80, 78, 71], "png"));
        options.Images.Add("photo", ImageAsset.FromBytes([255, 216, 255], "jpeg"));
        options.ExtraBody.Add("x_celeris", JsonContent.Parse("""{"future_option":"kept"}""").Value);
        var questions = new QuestionSet();
        var matches = questions.AddNoul("matches", new NoulQuestion("The receipt total matches the claim."));
        var team = questions.AddChoice("team", ChoiceQuestion.FromValues(["billing", "technical"]));
        var urgency = questions.AddScore("urgency", ScoreQuestion.FromValues(["Calm", "Urgent"]));
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            request.RequestUri.Should().Be(new Uri("https://inference.celeris.ai/celeris-1-decision/v1/systemone"));
            request.Method.Should().Be(HttpMethod.Post);
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be("celeris-test-key");
            request.Headers.Contains("TypeSafe-Danger-Zone-Images").Should().BeFalse();
            request.Headers.Contains("TypeSafe-Danger-Zone-Long-Context").Should().BeFalse();
            request.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
            using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(ct));
            var root = body.RootElement;
            root.GetProperty("model").GetString().Should().Be(CustomProviders.CelerisModel);
            root.GetProperty("state").GetProperty("claim").GetDouble().Should().Be(13.5);
            root.TryGetProperty("assets", out _).Should().BeFalse();
            root.TryGetProperty("stream", out _).Should().BeFalse();
            root.GetProperty("questions").GetProperty("matches").GetProperty("type").GetString().Should().Be("noul");
            var extension = root.GetProperty("x_celeris");
            extension.GetProperty("explain").GetBoolean().Should().BeTrue();
            extension.GetProperty("max_soft_tokens").GetInt32().Should().Be(560);
            extension.GetProperty("future_option").GetString().Should().Be("kept");
            extension.GetProperty("images").GetProperty("receipt").GetString().Should().Be("data:image/png;base64,iVBORw==");
            extension.GetProperty("images").GetProperty("photo").GetString().Should().Be("data:image/jpeg;base64,/9j/");
            return CelerisOk();
        }));
        using (var client = CustomProviders.Celeris("celeris-test-key", http))
        {
            client.Provider.Should().Be(SystemOneProvider.Celeris);
            var response = await client.SystemOneAsync(JsonContent.Parse("""{"claim":13.5}"""), questions, requestOptions: options);
            response.Get(matches).Noul.Should().Be(0.95);
            response.Get(team).Choice.Should().Be("billing");
            response.Get(urgency).Score.Should().Be(0.6);
            response.Response.Celeris!.Explanations["matches"].Should().Be("The totals match.");
            response.Response.RawJson.GetProperty("x_celeris").GetProperty("future").GetProperty("kept").GetBoolean().Should().BeTrue();
            response.Response.Usage.Should().Be(new Usage(122, 15));
            var replay = TypeSafeClient.FromHttpResponse(response.Response.RawJson.GetRawText());
            replay.Celeris!.Explanations.Should().BeEquivalentTo(response.Response.Celeris.Explanations);
        }
        // Disposing the SDK client must leave a caller-owned HttpClient usable.
        using var again = CustomProviders.Celeris("celeris-test-key", http);
        await again.SystemOneAsync(JsonContent.Parse("""{"claim":13.5}"""), questions, requestOptions: options);
        options.ExtraBody["x_celeris"].TryGetProperty("explain", out _).Should().BeFalse();
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(70)]
    [DataRow(140)]
    [DataRow(280)]
    [DataRow(560)]
    [DataRow(1120)]
    public async Task CelerisResolutionUsesDocumentedValues(int resolution)
    {
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            if (resolution == 0) body.RootElement.TryGetProperty("x_celeris", out _).Should().BeFalse();
            else body.RootElement.GetProperty("x_celeris").GetProperty("max_soft_tokens").GetInt32().Should().Be(resolution);
            return CelerisOk();
        }));
        using var client = CustomProviders.Celeris("test", http);
        await client.SystemOneAsync("test", CelerisQuestions(), requestOptions: new RequestOptions
        {
            Celeris = new CelerisDecisionOptions { ImageResolution = (CelerisImageResolution)resolution },
        });
    }

    [TestMethod]
    public async Task CelerisOptionsArePerRequestAndWorkWithAiFunctions()
    {
        var requests = 0;
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            body.RootElement.TryGetProperty("x_celeris", out _).Should().Be(++requests == 1);
            return CelerisOk();
        }));
        using var client = CustomProviders.Celeris("test", http);
        await client.SystemOneAsync("test", CelerisQuestions(), requestOptions: new RequestOptions
        { Celeris = new CelerisDecisionOptions { Explain = false } });
        var function = TypeSafeAIFunctions.Create(client, CelerisQuestions(), "judge", "Judge synthetic input.");
        await function.InvokeAsync(new Microsoft.Extensions.AI.AIFunctionArguments { ["state"] = "test" });
        requests.Should().Be(2);
    }

    [TestMethod]
    public async Task CelerisRejectsUnsupportedOptionsBeforeHttp()
    {
        using var http = new HttpClient(new StubHandler((_, _) => throw new InvalidOperationException("Must fail before HTTP.")));
        using var client = CustomProviders.Celeris("test", http);
        foreach (var options in new[]
        {
            new RequestOptions { EnableLongContext = true },
            new RequestOptions { EnableImages = true },
            new RequestOptions { Compression = RequestCompression.Gzip },
        })
        {
            var action = () => client.SystemOneAsync("test", CelerisQuestions(), requestOptions: options);
            await action.Should().ThrowAsync<NotSupportedException>();
        }
        var box = () => client.SystemOneAsync("test", new Dictionary<string, Question> { ["box"] = new BoxQuestion("Find a box.") });
        await box.Should().ThrowAsync<NotSupportedException>();
        var rawBox = () => client.SystemOneAsync("test", new Dictionary<string, Question> { ["box"] = new RawQuestion(JsonContent.Parse("""{"type":"box"}""").Value) });
        await rawBox.Should().ThrowAsync<NotSupportedException>();
        var model = () => client.SystemOneAsync("test", CelerisQuestions(), model: "jev-latest");
        await model.Should().ThrowAsync<ArgumentException>();
        using var typeSafe = new TypeSafeClient("test", http);
        var wrongProvider = () => typeSafe.SystemOneAsync("test", CelerisQuestions(), requestOptions: new RequestOptions
        { Celeris = new CelerisDecisionOptions { Explain = true } });
        await wrongProvider.Should().ThrowAsync<NotSupportedException>();
    }

    [TestMethod]
    [DataRow("{\"explain\":true}", true)]
    [DataRow("{\"max_soft_tokens\":560}", false)]
    public async Task CelerisRejectsTypedAndRawOptionCollisions(string extra, bool explain)
    {
        using var http = new HttpClient(new StubHandler((_, _) => throw new InvalidOperationException("Must fail before HTTP.")));
        using var client = CustomProviders.Celeris("test", http);
        var options = new RequestOptions { Celeris = explain
            ? new CelerisDecisionOptions { Explain = true }
            : new CelerisDecisionOptions { ImageResolution = CelerisImageResolution.Tokens560 } };
        options.ExtraBody.Add("x_celeris", JsonContent.Parse(extra).Value);
        var action = () => client.SystemOneAsync("test", CelerisQuestions(), requestOptions: options);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    [DataRow("null")]
    [DataRow("{\"explain\":null}")]
    [DataRow("{\"explain\":\"true\"}")]
    [DataRow("{\"max_soft_tokens\":0}")]
    [DataRow("{\"max_soft_tokens\":256}")]
    [DataRow("{\"images\":{\"photo\":\"https://example.test/photo.png\"}}")]
    public async Task CelerisRejectsMalformedRawExtensions(string extra)
    {
        using var http = new HttpClient(new StubHandler((_, _) => throw new InvalidOperationException("Must fail before HTTP.")));
        using var client = CustomProviders.Celeris("test", http);
        var options = new RequestOptions();
        options.ExtraBody.Add("x_celeris", JsonContent.Parse(extra).Value);
        var action = () => client.SystemOneAsync("test", CelerisQuestions(), requestOptions: options);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    [DataRow(false, 512, true)]
    [DataRow(false, 513, false)]
    [DataRow(true, 64, true)]
    [DataRow(true, 65, false)]
    public async Task CelerisQuestionLimitsRespectRawExplanationOptions(bool explain, int count, bool allowed)
    {
        var calls = 0;
        using var http = new HttpClient(new StubHandler((_, _) => { calls++; return Task.FromResult(CelerisOk()); }));
        using var client = CustomProviders.Celeris("test", http);
        var questions = Enumerable.Range(0, count).ToDictionary(i => "q" + i, _ => (Question)new NoulQuestion("test?"));
        var options = new RequestOptions();
        options.ExtraBody.Add("x_celeris", JsonContent.Parse(explain ? "{\"explain\":true}" : "{\"explain\":false}").Value);
        var action = () => client.SystemOneAsync("test", questions, requestOptions: options);
        if (allowed) await action();
        else await action.Should().ThrowAsync<ArgumentException>();
        calls.Should().Be(allowed ? 1 : 0);
    }

    [TestMethod]
    [DataRow(2, true)]
    [DataRow(512, true)]
    [DataRow(513, false)]
    [DataRow(1, false)]
    public async Task CelerisChoiceCountsHaveProviderSpecificBounds(int count, bool allowed)
    {
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(CelerisOk())));
        using var client = CustomProviders.Celeris("test", http);
        var question = ChoiceQuestion.FromValues(Enumerable.Range(0, count).Select(i => "c" + i));
        var action = () => client.SystemOneAsync("test", new Dictionary<string, Question> { ["team"] = question });
        if (allowed) await action();
        else await action.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    [DataRow(1, false)]
    [DataRow(2, true)]
    [DataRow(64, true)]
    [DataRow(65, false)]
    public async Task CelerisScoreCountsHaveProviderSpecificBounds(int count, bool allowed)
    {
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(CelerisOk())));
        using var client = CustomProviders.Celeris("test", http);
        var question = ScoreQuestion.FromValues(Enumerable.Range(0, count).Select(i => "level" + i));
        var action = () => client.SystemOneAsync("test", new Dictionary<string, Question> { ["urgency"] = question });
        if (allowed) await action();
        else await action.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    public async Task CelerisRejectsInvalidResolutionTooManyImagesAndOversizedBodies()
    {
        using var http = new HttpClient(new StubHandler((_, _) => throw new InvalidOperationException("Must fail before HTTP.")));
        using var client = CustomProviders.Celeris("test", http);
        var invalid = () => client.SystemOneAsync("test", CelerisQuestions(), requestOptions: new RequestOptions
        { Celeris = new CelerisDecisionOptions { ImageResolution = (CelerisImageResolution)256 } });
        await invalid.Should().ThrowAsync<ArgumentOutOfRangeException>();
        var options = new RequestOptions();
        for (var i = 0; i < 9; i++) options.Images.Add("image" + i, ImageAsset.FromBytes([1], "png"));
        var images = () => client.SystemOneAsync("test", CelerisQuestions(), requestOptions: options);
        await images.Should().ThrowAsync<ArgumentException>();
        var huge = () => client.SystemOneAsync(new string('é', 4 * 1024 * 1024), CelerisQuestions());
        await huge.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    public async Task CelerisRetriesPreserveImagesAndExplanationsAndErrors()
    {
        var attempts = 0;
        string? first = null;
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            var body = await request.Content!.ReadAsStringAsync(ct);
            if (first is null) first = body;
            else body.Should().Be(first);
            return ++attempts == 1 ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("{}") } : CelerisOk();
        }));
        using var client = CustomProviders.Celeris("test", http, options => options.Retry = new RetryPolicy
        { InitialDelay = TimeSpan.Zero, MaximumDelay = TimeSpan.Zero });
        var options = new RequestOptions { Celeris = new CelerisDecisionOptions { Explain = true } };
        options.Images.Add("photo", ImageAsset.FromBytes([1], "png"));
        var result = await client.SystemOneAsync("photo", CelerisQuestions(), requestOptions: options);
        result.Metadata.RetryCount.Should().Be(1);
        result.Celeris!.Explanations["matches"].Should().Be("The totals match.");
        using var failedHttp = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        { Content = new StringContent("""{"detail":[{"msg":"invalid question"}]}""") })));
        using var failed = CustomProviders.Celeris("test", failedHttp);
        var action = () => failed.SystemOneAsync("test", CelerisQuestions());
        var error = await action.Should().ThrowAsync<UnprocessableEntityException>();
        error.Which.ResponseBody.Should().Contain("invalid question");
    }

    [TestMethod]
    [DataRow("[]")]
    [DataRow("{\"matches\":42}")]
    public void CelerisReplayRejectsInvalidExplanationShapes(string explanations)
    {
        var response = CelerisResponse.Replace("{\"matches\":\"The totals match.\",\"team\":\"The ticket concerns a payment.\",\"urgency\":\"A prompt reply would help.\"}", explanations, StringComparison.Ordinal);
        var action = () => TypeSafeClient.FromHttpResponse(response);
        action.Should().Throw<TypeSafeResponseValidationException>();
    }

    private static IReadOnlyDictionary<string, Question> CelerisQuestions() =>
        new Dictionary<string, Question> { ["matches"] = new NoulQuestion("The totals match.") };
    private static HttpResponseMessage CelerisOk() => new(HttpStatusCode.OK) { Content = new StringContent(CelerisResponse) };

    [TestMethod]
    public async Task CelerisAcceptsEightImagesAndRejectsConflictingRawImages()
    {
        var calls = 0;
        using var http = new HttpClient(new StubHandler((_, _) => { calls++; return Task.FromResult(CelerisOk()); }));
        using var client = CustomProviders.Celeris("test", http);
        var options = new RequestOptions();
        for (var i = 0; i < 8; i++) options.Images.Add("image" + i, ImageAsset.FromBytes([1], "png"));
        await client.SystemOneAsync("images", CelerisQuestions(), requestOptions: options);
        options.ExtraBody.Add("x_celeris", JsonContent.Parse("""{"images":{"other":"data:image/png;base64,AQ=="}}""").Value);
        var collision = () => client.SystemOneAsync("images", CelerisQuestions(), requestOptions: options);
        await collision.Should().ThrowAsync<ArgumentException>();
        var assets = new RequestOptions();
        assets.ExtraBody.Add("assets", JsonContent.Parse("{}").Value);
        var invalidAssets = () => client.SystemOneAsync("images", CelerisQuestions(), requestOptions: assets);
        await invalidAssets.Should().ThrowAsync<ArgumentException>();
        calls.Should().Be(1);
    }

    [TestMethod]
    public async Task CelerisRawExtensionsAndRequestOverridesRemainAvailable()
    {
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            request.Headers.Authorization!.Parameter.Should().Be("override-key");
            request.Headers.GetValues("x-client-test").Should().Equal("fixture");
            request.RequestUri!.Query.Should().Be("?fixture=true");
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            body.RootElement.GetProperty("x_celeris").GetProperty("images").GetProperty("photo").GetString().Should().Be("data:image/png;base64,AQ==");
            return CelerisOk();
        }));
        using var client = CustomProviders.Celeris("test", http);
        var options = new RequestOptions { ApiKey = "override-key" };
        options.Headers.Add("x-client-test", "fixture");
        options.QueryParameters.Add("fixture", "true");
        options.ExtraBody.Add("x_celeris", JsonContent.Parse("""{"images":{"photo":"data:image/png;base64,AQ=="},"max_soft_tokens":140}""").Value);
        await client.SystemOneAsync("photo", CelerisQuestions(), requestOptions: options);
    }

    [TestMethod]
    public void CelerisFactorySupportsConfigurationAndValidatesProfile()
    {
        using var client = CustomProviders.Celeris("test", configure: options =>
        {
            options.Timeout = TimeSpan.FromSeconds(5);
            options.Retry = RetryPolicy.None;
        });
        client.Provider.Should().Be(SystemOneProvider.Celeris);
        var wrongProfile = () => CustomProviders.Celeris("test", configure: options => options.Provider = SystemOneProvider.TypeSafe);
        wrongProfile.Should().Throw<ArgumentException>();
        var invalidProfile = () => new TypeSafeClient("test", options: new TypeSafeClientOptions { Provider = (SystemOneProvider)9 });
        invalidProfile.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ProviderApiKeyOverridesAreIsolatedAcrossCalls(bool celeris)
    {
        var calls = 0;
        using var http = new HttpClient(new StubHandler((request, _) =>
        {
            request.Headers.Authorization!.Parameter.Should().Be(++calls == 1 ? "one-call-key" : "default-key");
            return Task.FromResult(CelerisOk());
        }));
        using var client = celeris ? CustomProviders.Celeris("default-key", http) : new TypeSafeClient("default-key", http);
        await client.SystemOneAsync("test", CelerisQuestions(), requestOptions: new RequestOptions { ApiKey = "one-call-key" });
        await client.SystemOneAsync("test", CelerisQuestions());
        calls.Should().Be(2);
    }

    [TestMethod]
    public async Task CelerisDependencyInjectionSelectsProviderImageEncoding()
    {
        var services = new ServiceCollection();
        services.AddTypeSafeClient("test", options =>
        {
            options.Provider = SystemOneProvider.Celeris;
            options.BaseUri = new Uri(CustomProviders.CelerisBaseUrl);
            options.DefaultModel = CustomProviders.CelerisModel;
        }).ConfigurePrimaryHttpMessageHandler(() => new StubHandler(async (request, ct) =>
        {
            request.RequestUri.Should().Be(new Uri("https://inference.celeris.ai/celeris-1-decision/v1/systemone"));
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            body.RootElement.GetProperty("x_celeris").GetProperty("images").GetProperty("photo").GetString().Should().Be("data:image/png;base64,AQ==");
            return CelerisOk();
        }));
        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<ITypeSafeClient>();
        var requestOptions = new RequestOptions();
        requestOptions.Images.Add("photo", ImageAsset.FromBytes([1], "png"));
        await client.SystemOneAsync((JsonContent)"photo", CelerisQuestions(), requestOptions: requestOptions);
    }
}
