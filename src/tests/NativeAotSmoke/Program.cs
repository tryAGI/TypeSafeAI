using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using TypeSafeAI;
using TypeSafeAI.Extensions.AI;

using var http = new HttpClient(new FixtureHandler());
using var client = new TypeSafeClient("fixture-not-a-secret", http);
var questions = new QuestionSet();
var intent = questions.AddChoice<Intent>("intent");
questions.AddNoul("ok", new NoulQuestion("Is this a test?"));
var response = await client.SystemOneAsync(JsonContent.Parse("""{"text":"synthetic"}"""), questions);
if (response.Get(intent).Choice != Intent.Play || EnumLabels<Intent>.GetDescription(Intent.Play) != "Play music")
    throw new InvalidOperationException("Trimmed enum metadata did not survive.");
if (response.Response.Get<RawAnswer>("future").Value.GetProperty("value").GetInt32() != 42)
    throw new InvalidOperationException("Unknown answer was lost.");
var projected = await client.SystemOneAsync("synthetic", questions, SmokeJsonContext.Default.Projection);
if (projected.Ok?.Noul != 0.9 || projected.Absent is not null)
    throw new InvalidOperationException("Typed projection did not survive.");
var function = TypeSafeAIFunctions.Create(client, questions, "judge", "Judge synthetic state.");
if (await function.InvokeAsync(new AIFunctionArguments { ["state"] = "synthetic" }) is null)
    throw new InvalidOperationException("AI function returned no result.");
Console.WriteLine("NativeAOT transport, enum labels, typed projection, future answers, and AI function passed.");

enum Intent
{
    [Label("play_music", Description = "Play music")]
    Play,
    Ignore,
}

sealed class Projection
{
    public NoulAnswer? Ok { get; set; }
    [OptionalAnswer] public NoulAnswer? Absent { get; set; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Projection))]
internal partial class SmokeJsonContext : JsonSerializerContext;

sealed class FixtureHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"model":"fixture","answers":{"intent":{"type":"choice","choice":"play_music","confidence":1,"probabilities":{"play_music":1,"Ignore":0}},"ok":{"type":"noul","noul":0.9},"future":{"type":"future","value":42}},"usage":{"input_tokens":1,"output_tokens":0}}"""),
        });
}
