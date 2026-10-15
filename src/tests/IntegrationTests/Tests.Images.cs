using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    private const string BoxResponse = """
        {"model":"jev-1.14.0","answers":{"mug":{"type":"box","box":{"top":0.25,"bottom":0.82,"left":0.44,"right":0.84},"confidence":0.92,"future":"kept"},"visible":{"type":"noul","noul":0.95}},"usage":{"input_tokens":290,"output_tokens":21}}
        """;

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ImagesSupportMultipartAndBase64WithMixedTypedQuestions(bool base64)
    {
        var bytes = new byte[] { 137, 80, 78, 71 }; // Wire fixture; decoding belongs to the server.
        var source = bytes.ToArray();
        var image = ImageAsset.FromBytes(source, "PNG");
        source[0] = 0; // The asset must own a copy.
        var options = new RequestOptions { UseBase64Images = base64 };
        options.Images.Add(image.Reference, image);
        var questions = new QuestionSet();
        var mug = questions.AddBox("mug", new BoxQuestion(JsonContent.Parse($$"""{"target":"the mug","reference":"{{image.Reference}}"}""")));
        questions.AddNoul("visible", new NoulQuestion("Is the mug visible?", new NoulCriteria(True: image.Reference)));
        questions.AddChoice("gesture", new ChoiceQuestion(new Dictionary<string, JsonContent?> { ["mug"] = image.Reference }));
        questions.AddScore("size", new ScoreQuestion([JsonContent.Parse($$"""{"examples":["{{image.Reference}}"]}""")]));
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            request.Headers.GetValues("TypeSafe-Danger-Zone-Images").Should().Equal("true");
            request.Headers.Authorization!.Parameter.Should().Be("test");
            string json;
            if (base64)
            {
                request.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
                json = await request.Content.ReadAsStringAsync(ct);
            }
            else
            {
                var parts = ((MultipartFormDataContent)request.Content!).ToArray();
                parts.Length.Should().Be(2);
                parts[0].Headers.ContentDisposition!.Name!.Trim('"').Should().Be("request");
                parts[0].Headers.ContentType!.MediaType.Should().Be("application/json");
                parts[1].Headers.ContentDisposition!.Name!.Trim('"').Should().Be("image-0");
                parts[1].Headers.ContentType!.MediaType.Should().Be("image/png");
                (await parts[1].ReadAsByteArrayAsync(ct)).Should().Equal(bytes);
                json = await parts[0].ReadAsStringAsync(ct);
            }
            using var body = JsonDocument.Parse(json);
            var root = body.RootElement;
            root.GetProperty("state").GetProperty("frames")[0].GetString().Should().Be(image.Reference);
            var asset = root.GetProperty("assets").GetProperty(image.Reference);
            asset.GetProperty("type").GetString().Should().Be("image");
            if (base64)
            {
                asset.GetProperty("format").GetString().Should().Be("png");
                Convert.FromBase64String(asset.GetProperty("data_base64").GetString()!).Should().Equal(bytes);
                asset.TryGetProperty("part", out _).Should().BeFalse();
            }
            else
            {
                asset.GetProperty("part").GetString().Should().Be("image-0");
                asset.TryGetProperty("data_base64", out _).Should().BeFalse();
            }
            var wireQuestions = root.GetProperty("questions");
            wireQuestions.GetProperty("mug").GetProperty("type").GetString().Should().Be("box");
            wireQuestions.GetProperty("mug").TryGetProperty("criteria", out _).Should().BeFalse();
            wireQuestions.GetProperty("mug").GetProperty("instructions").GetProperty("reference").GetString().Should().Be(image.Reference);
            wireQuestions.GetProperty("visible").GetProperty("criteria").GetProperty("true").GetString().Should().Be(image.Reference);
            wireQuestions.GetProperty("gesture").GetProperty("criteria").GetProperty("mug").GetString().Should().Be(image.Reference);
            wireQuestions.GetProperty("size").GetProperty("criteria")[0].GetProperty("examples")[0].GetString().Should().Be(image.Reference);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(BoxResponse) };
        }));
        using var client = new TypeSafeClient("test", http);
        var result = await client.SystemOneAsync(JsonContent.Parse($$"""{"frames":["{{image.Reference}}"]}"""), questions, "jev-preview", options);
        result.Get(mug).Box.Should().Be(new BoxCoordinates(0.25, 0.82, 0.44, 0.84));
        result.Get(mug).Confidence.Should().Be(0.92);
        result.Response.Answers.Boxes.Should().ContainKey("mug");
        result.Get(mug).RawJson.GetProperty("future").GetString().Should().Be("kept");
        result.Response.Usage.Should().Be(new Usage(290, 21));
    }

    [TestMethod]
    public async Task MultipartRetriesRecreateAllParts()
    {
        var attempts = 0;
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            var parts = ((MultipartFormDataContent)request.Content!).ToArray();
            (await parts[1].ReadAsByteArrayAsync(ct)).Should().Equal(new byte[] { 1, 2, 3 });
            return ++attempts == 1
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("{}") }
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(BoxResponse) };
        }));
        using var client = new TypeSafeClient("test", http, new TypeSafeClientOptions
        {
            Retry = new RetryPolicy { InitialDelay = TimeSpan.Zero, MaximumDelay = TimeSpan.Zero },
        });
        var options = new RequestOptions();
        options.Images.Add("unique-image-reference", ImageAsset.FromBytes([1, 2, 3], "jpeg"));
        var response = await client.SystemOneAsync("unique-image-reference", new Dictionary<string, Question> { ["mug"] = new BoxQuestion("mug") }, requestOptions: options);
        attempts.Should().Be(2);
        response.Metadata.RetryCount.Should().Be(1);
    }

    [TestMethod]
    public async Task ImageOptionsDoNotLeakIntoOtherCallsAndValidateReferenceBytes()
    {
        var requests = 0;
        using var http = new HttpClient(new StubHandler((request, _) =>
        {
            request.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
            request.Headers.Contains("TypeSafe-Danger-Zone-Images").Should().BeFalse();
            requests++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(BoxResponse) });
        }));
        using var client = new TypeSafeClient("test", http);
        var questions = new Dictionary<string, Question> { ["visible"] = new NoulQuestion("visible?") };
        var invalid = new RequestOptions();
        invalid.Images.Add(new string('é', 513), ImageAsset.FromBytes([1], "png"));
        var rejected = () => client.SystemOneAsync("test", questions, requestOptions: invalid);
        await rejected.Should().ThrowAsync<ArgumentException>();
        await client.SystemOneAsync("test", questions);
        requests.Should().Be(1);
    }

    [TestMethod]
    public async Task BoxSupportsTypedProjectionAndRejectsWrongAnswerType()
    {
        var payload = BoxResponse;
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload) })));
        using var client = new TypeSafeClient("test", http);
        var questions = new Dictionary<string, Question> { ["mug"] = new BoxQuestion("mug") };
        var projection = await client.SystemOneAsync("test", questions, ImageProjectionContext.Default.BoxProjection);
        projection.Mug!.Confidence.Should().Be(0.92);
        projection.Mug.Box.Left.Should().Be(0.44);
        payload = BoxResponse.Replace("\"type\":\"box\"", "\"type\":\"future\"");
        var action = () => client.SystemOneAsync("test", questions, ImageProjectionContext.Default.BoxProjection);
        await action.Should().ThrowAsync<TypeSafeResponseValidationException>();
    }

    [TestMethod]
    [DataRow("\"left\":0.44", "\"left\":0.99")]
    [DataRow("\"top\":0.25", "\"top\":-0.1")]
    [DataRow("\"confidence\":0.92", "\"confidence\":1.1")]
    public void BoxReplayRejectsInvalidCoordinatesAndConfidence(string original, string invalid)
    {
        var action = () => TypeSafeClient.FromHttpResponse(BoxResponse.Replace(original, invalid));
        action.Should().Throw<TypeSafeResponseValidationException>();
    }

    [TestMethod]
    public void ImageAssetRejectsUnsupportedInput()
    {
        var empty = () => ImageAsset.FromBytes([], "png");
        empty.Should().Throw<ArgumentException>();
        var unsupported = () => ImageAsset.FromBytes([1], "webp");
        unsupported.Should().Throw<ArgumentException>();
        var oversized = () => ImageAsset.FromBytes(new byte[16 * 1024 * 1024 + 1], "jpg");
        oversized.Should().Throw<ArgumentException>();
        ImageAsset.FromBytes([1], "jpeg").Format.Should().Be("jpg");
    }
}

internal sealed class BoxProjection
{
    public BoxAnswer? Mug { get; set; }
}
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(BoxProjection))]
internal sealed partial class ImageProjectionContext : JsonSerializerContext;
