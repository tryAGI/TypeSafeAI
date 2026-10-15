using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    private const string CelerisModels = """
        {"object":"list","data":[{"id":"celeris-1-decision","object":"model","created":1791763200,"owned_by":"celeris","future":{"retained":true}},{"id":"future-model"}]}
        """;

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task CelerisDiscoveryPreservesMetadataAndTransport(bool readAsString)
    {
        var attempts = 0;
        using var http = new HttpClient(new StubHandler((request, _) =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.AbsoluteUri.Should().Be("https://inference.celeris.ai/celeris-1-decision/v1/models?trace=yes");
            request.Headers.Authorization!.Parameter.Should().Be("per-call");
            request.Headers.GetValues("X-Test").Single().Should().Be("retained");
            return Task.FromResult(new HttpResponseMessage(++attempts == 1 ? HttpStatusCode.InternalServerError : HttpStatusCode.OK)
            { Content = new StringContent(attempts == 1 ? "{}" : CelerisModels) });
        }));
        using var client = CustomProviders.Celeris("fixture", http, options => options.Retry = new RetryPolicy { MaxAttempts = 2, InitialDelay = TimeSpan.Zero });
        client.RawClient.ReadResponseAsString = readAsString;
        var requestOptions = new RequestOptions { ApiKey = "per-call" };
        requestOptions.QueryParameters["trace"] = "yes";
        requestOptions.Headers["X-Test"] = "retained";
        var models = await client.Models.ListAsync(requestOptions);
        attempts.Should().Be(2);
        models[0].Name.Should().Be(CustomProviders.CelerisModel);
        models[0].Description.Should().BeEmpty();
        models[0].ReleaseDate.Should().BeEmpty();
        models[0].ReleasedAt.Should().BeNull();
        models[0].CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1791763200));
        models[0].OwnedBy.Should().Be("celeris");
        models[0].RawJson.GetProperty("future").GetProperty("retained").GetBoolean().Should().BeTrue();
        models[1].CreatedAt.Should().BeNull();
        models[1].OwnedBy.Should().BeNull();
    }

    [TestMethod]
    [DataRow("{\"data\":[]}")]
    [DataRow("{\"models\":[]}")]
    public void CelerisDiscoverySupportsEmptyListsAndSynchronousCalls(string json)
    {
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) })));
        using var client = CustomProviders.Celeris("fixture", http);
        client.Models.List().Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("{}")]
    [DataRow("{\"data\":{}}")]
    [DataRow("{\"data\":[{}]}")]
    [DataRow("{\"data\":[{\"id\":17}]}")]
    [DataRow("{\"data\":[{\"id\":\"m\",\"created\":1.2}]}")]
    [DataRow("{\"data\":[{\"id\":\"m\",\"created\":9223372036854775807}]}")]
    [DataRow("{\"data\":[{\"id\":\"m\",\"owned_by\":false}]}")]
    public async Task CelerisDiscoveryRejectsMalformedMetadata(string json)
    {
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) })));
        using var client = CustomProviders.Celeris("fixture", http);
        Func<Task> call = async () => await client.Models.ListAsync();
        await call.Should().ThrowAsync<TypeSafeResponseValidationException>();
    }

    [TestMethod]
    public async Task CelerisDiscoveryMapsAuthenticationAndCancellation()
    {
        using var http = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("{}") })));
        using var client = CustomProviders.Celeris("fixture", http);
        Func<Task> call = async () => await client.Models.ListAsync();
        await call.Should().ThrowAsync<AuthenticationException>();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        call = async () => await client.Models.ListAsync(cancellationToken: cancellation.Token);
        await call.Should().ThrowAsync<TypeSafeUserAbortException>();
    }

    [TestMethod]
    public async Task AddCelerisClientSharesFactoryDefaultsAndSupportsConfiguration()
    {
        var services = new ServiceCollection();
        services.AddCelerisClient("fixture", options => options.Retry = RetryPolicy.None)
            .ConfigurePrimaryHttpMessageHandler(() => new StubHandler((request, _) =>
            {
                request.RequestUri!.AbsoluteUri.Should().Be(CustomProviders.CelerisBaseUrl + "v1/models");
                request.Headers.Authorization!.Parameter.Should().Be("fixture");
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(CelerisModels) });
            }));
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TypeSafeClientOptions>();
        options.Provider.Should().Be(SystemOneProvider.Celeris);
        options.DefaultModel.Should().Be(CustomProviders.CelerisModel);
        options.Retry.MaxAttempts.Should().Be(1);
        provider.GetRequiredService<ITypeSafeClient>().Should().BeOfType<TypeSafeClient>();
        var client = provider.GetRequiredService<TypeSafeClient>();
        (await client.Models.ListAsync())[0].OwnedBy.Should().Be("celeris");
        Action invalid = () => new ServiceCollection().AddCelerisClient("fixture", settings => settings.Provider = SystemOneProvider.TypeSafe);
        invalid.Should().Throw<ArgumentException>();
        Action empty = () => new ServiceCollection().AddCelerisClient(" ");
        empty.Should().Throw<ArgumentException>();
    }
}

[TestClass]
[DoNotParallelize]
public sealed class CelerisEnvironmentTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("fixture")]
    public void EnvironmentRegistrationValidatesCredentials(string? value)
    {
        var previous = Environment.GetEnvironmentVariable("CELERIS_API_KEY");
        try
        {
            Environment.SetEnvironmentVariable("CELERIS_API_KEY", value);
            var services = new ServiceCollection();
            if (string.IsNullOrWhiteSpace(value))
            {
                Action call = () => services.AddCelerisClientFromEnvironment();
                call.Should().Throw<InvalidOperationException>();
            }
            else
            {
                services.AddCelerisClientFromEnvironment(options => options.Timeout = TimeSpan.FromSeconds(7));
                using var provider = services.BuildServiceProvider();
                provider.GetRequiredService<TypeSafeClient>().Provider.Should().Be(SystemOneProvider.Celeris);
                provider.GetRequiredService<TypeSafeClientOptions>().Timeout.Should().Be(TimeSpan.FromSeconds(7));
            }
        }
        finally { Environment.SetEnvironmentVariable("CELERIS_API_KEY", previous); }
    }
}
