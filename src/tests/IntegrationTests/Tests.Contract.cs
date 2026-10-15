using System.Net;
using System.Text.Json;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    [DataRow(400, typeof(BadRequestException))]
    [DataRow(401, typeof(AuthenticationException))]
    [DataRow(403, typeof(PermissionDeniedException))]
    [DataRow(404, typeof(NotFoundException))]
    [DataRow(413, typeof(TypeSafeApiException))]
    [DataRow(415, typeof(TypeSafeApiException))]
    [DataRow(422, typeof(UnprocessableEntityException))]
    [DataRow(429, typeof(RateLimitException))]
    [DataRow(529, typeof(InternalServerException))]
    public async Task DocumentedHttpErrorsPreserveStatusAndBody(int status, Type expectedType)
    {
        const string payload = """{"detail":"fixture failure"}""";
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status)
        {
            Content = new StringContent(payload),
        })));
        using var client = new TypeSafeClient("test", http, new TypeSafeClientOptions { Retry = RetryPolicy.None });
        var action = () => client.SystemOneAsync("test", new Dictionary<string, Question> { ["q"] = new NoulQuestion("test?") });
        var failure = await action.Should().ThrowAsync<TypeSafeApiException>();
        failure.Which.GetType().Should().Be(expectedType);
        failure.Which.StatusCode.Should().Be((HttpStatusCode)status);
        failure.Which.ResponseBody.Should().Be(payload);
    }

    [TestMethod]
    public async Task KnownRawQuestionExtrasAndExplicitNullBodyFieldsAreForwarded()
    {
        using var http = new HttpClient(new StubHandler(async (request, ct) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            body.RootElement.GetProperty("future_option").ValueKind.Should().Be(JsonValueKind.Null);
            body.RootElement.GetProperty("questions").GetProperty("q").GetProperty("weight").GetInt32().Should().Be(2);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(BoxResponse) };
        }));
        using var client = new TypeSafeClient("test", http);
        var options = new RequestOptions();
        options.ExtraBody.Add("future_option", JsonContent.Null.Value);
        await client.SystemOneAsync("test", new Dictionary<string, Question>
        {
            ["q"] = new RawQuestion(JsonContent.Parse("""{"type":"noul","instructions":"test?","weight":2}""").Value),
        }, requestOptions: options);
    }
}
