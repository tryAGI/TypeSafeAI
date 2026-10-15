using System.Net;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using TypeSafeAI;
using TypeSafeAI.Extensions.AI;

using var http = new HttpClient(new FixtureHandler());
using var client = new TypeSafeClient("fixture-not-a-secret", http);
var questions = new QuestionSet();
var intent = questions.AddChoice<Intent>("intent");
questions.AddNoul("ok", new NoulQuestion("Is this a test?"));
var box = questions.AddBox("box", new BoxQuestion("the target in the image"));
var image = ImageAsset.FromBytes([137, 80, 78, 71], "png");
var deep = JsonContent.Parse(new string('[', 64) + "\"leaf\"" + new string(']', 64));
questions.AddScore("structure", ScoreQuestion.FromCriteria(JsonContent.Parse("""[{"signals":[true,null,1]},["other"]]"""), deep));
var imageOptions = new RequestOptions { Compression = RequestCompression.Gzip, EnableLongContext = true };
imageOptions.Images.Add(image.Reference, image);
var response = await client.SystemOneAsync(JsonContent.Parse("""{"text":"synthetic"}"""), questions);
if (response.Get(intent).Choice != Intent.Play || EnumLabels<Intent>.GetDescription(Intent.Play) != "Play music")
    throw new InvalidOperationException("Trimmed enum metadata did not survive.");
if (response.Response.Get<RawAnswer>("future").Value.GetProperty("value").GetInt32() != 42)
    throw new InvalidOperationException("Unknown answer was lost.");
var imageResponse = await client.SystemOneAsync(image.Reference + new string('x', 2048), questions, "jev-preview", imageOptions);
if (imageResponse.Get(box).Box.Right != 0.8 || imageResponse.Response.Get<ScoreAnswer>("structure").Legend["0"].GetProperty("signals")[0].GetBoolean() != true)
    throw new InvalidOperationException("Typed box decoding did not survive.");
var projected = await client.SystemOneAsync("synthetic", questions, SmokeJsonContext.Default.Projection);
if (imageResponse.Response.Usage.OutputTokens is not null)
    throw new InvalidOperationException("Unknown token usage was not retained.");
if (projected.Ok?.Noul != 0.9 || projected.Absent is not null || projected.Box?.Box.Right != 0.8)
    throw new InvalidOperationException("Typed projection did not survive.");
var function = TypeSafeAIFunctions.Create(client, questions, "judge", "Judge synthetic state.");
if (await function.InvokeAsync(new AIFunctionArguments { ["state"] = "synthetic" }) is null)
    throw new InvalidOperationException("AI function returned no result.");
using var celerisHttp = new HttpClient(new CelerisFixtureHandler());
using var celeris = CustomProviders.Celeris("fixture-not-a-secret", celerisHttp);
var celerisOptions = new RequestOptions
{
    Celeris = new CelerisDecisionOptions { Explain = true, ImageResolution = CelerisImageResolution.Tokens560 },
};
celerisOptions.Images.Add("receipt", image);
var celerisResponse = await celeris.SystemOneAsync(JsonContent.Parse("""{"image":"receipt"}"""),
    new Dictionary<string, Question> { ["ok"] = new NoulQuestion("The receipt is visible.") }, requestOptions: celerisOptions);
if (celerisResponse.Celeris?.Explanations["ok"] != "The receipt is visible." ||
    TypeSafeClient.FromHttpResponse(celerisResponse.RawJson.GetRawText()).Celeris?.Explanations["ok"] != "The receipt is visible.")
    throw new InvalidOperationException("Celeris typed explanations or replay did not survive NativeAOT.");
Console.WriteLine("NativeAOT transport, enum labels, projection, structure, compression, images, boxes, usage, future answers, AI function, and Celeris images/resolution/explanations/replay passed.");

enum Intent
{
    [Label("play_music", Description = "Play music")]
    Play,
    Ignore,
}

sealed class Projection
{
    public NoulAnswer? Ok { get; set; }
    public BoxAnswer? Box { get; set; }
    [OptionalAnswer] public NoulAnswer? Absent { get; set; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Projection))]
internal partial class SmokeJsonContext : JsonSerializerContext;

sealed class FixtureHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content!.Headers.ContentEncoding.Contains("gzip"))
        {
            using var compressed = new MemoryStream(await request.Content.ReadAsByteArrayAsync(cancellationToken));
            using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
            using var decoded = new MemoryStream();
            await gzip.CopyToAsync(decoded, cancellationToken);
            var text = Encoding.UTF8.GetString(decoded.ToArray());
            if (!text.Contains("leaf", StringComparison.Ordinal) || !text.Contains("image-0", StringComparison.Ordinal))
                throw new InvalidOperationException("Compressed structured multipart content was lost.");
        }
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"model":"fixture","answers":{"intent":{"type":"choice","choice":"play_music","confidence":1,"probabilities":{"play_music":1,"Ignore":0}},"ok":{"type":"noul","noul":0.9},"box":{"type":"box","box":{"top":0.2,"bottom":0.9,"left":0.1,"right":0.8},"confidence":0.95},"structure":{"type":"score","score":0,"confidence":1,"legend":{"0":{"signals":[true,null,1]},"1":["other"]},"probabilities":{"0":1,"1":0}},"future":{"type":"future","value":42}},"usage":{"input_tokens":1,"output_tokens":null}}"""),
        };
    }
}

sealed class CelerisFixtureHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri != new Uri("https://inference.celeris.ai/celeris-1-decision/v1/systemone") ||
            request.Headers.Contains("TypeSafe-Danger-Zone-Images") || request.Content!.Headers.ContentType!.MediaType != "application/json")
            throw new InvalidOperationException("The Celeris transport profile was lost.");
        using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
        var extension = body.RootElement.GetProperty("x_celeris");
        if (!extension.GetProperty("explain").GetBoolean() || extension.GetProperty("max_soft_tokens").GetInt32() != 560 ||
            extension.GetProperty("images").GetProperty("receipt").GetString() != "data:image/png;base64,iVBORw==" ||
            body.RootElement.TryGetProperty("assets", out _))
            throw new InvalidOperationException("Celeris images or typed options did not survive NativeAOT.");
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"model":"celeris-1-decision","answers":{"ok":{"type":"noul","noul":0.95}},"usage":{"input_tokens":122,"output_tokens":15},"x_celeris":{"explanations":{"ok":"The receipt is visible."}}}"""),
        };
    }
}
