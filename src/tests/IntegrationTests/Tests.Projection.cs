using System.Net;
using System.Text.Json.Serialization;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task TypedProjectionRequiresAnswersAndHonorsOptionalAttribute()
    {
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"model":"test","answers":{"q":{"type":"noul","noul":0.8}},"usage":{"input_tokens":1,"output_tokens":0}}"""),
        })));
        using var client = new TypeSafeClient("test", http);
        var questions = new Dictionary<string, Question> { ["q"] = new NoulQuestion() };
        var optional = await client.SystemOneAsync("test", questions, ProjectionContext.Default.OptionalProjection);
        optional.Q!.Noul.Should().Be(0.8);
        optional.Absent.Should().BeNull();
        var action = () => client.SystemOneAsync("test", questions, ProjectionContext.Default.RequiredProjection);
        await action.Should().ThrowAsync<TypeSafeResponseValidationException>();
    }
}

internal sealed class OptionalProjection
{
    public NoulAnswer? Q { get; set; }
    [OptionalAnswer] public NoulAnswer? Absent { get; set; }
}
internal sealed class RequiredProjection
{
    public NoulAnswer? Absent { get; set; }
}
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(OptionalProjection))]
[JsonSerializable(typeof(RequiredProjection))]
internal sealed partial class ProjectionContext : JsonSerializerContext;
