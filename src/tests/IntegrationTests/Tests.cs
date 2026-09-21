namespace TypeSafeAI.IntegrationTests;

[TestClass]
public partial class Tests
{
    private static TypeSafeClient GetAuthenticatedClient(TypeSafeClientOptions? options = null)
    {
        var apiKey =
            Environment.GetEnvironmentVariable("TYPESAFE_API_KEY") is { Length: > 0 } apiKeyValue
                ? apiKeyValue
                : throw new AssertInconclusiveException("TYPESAFE_API_KEY environment variable is not found.");

        var client = new TypeSafeClient(apiKey, options: options);
        
        return client;
    }
}
