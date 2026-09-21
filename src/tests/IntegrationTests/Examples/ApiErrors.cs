/*
order: 90
title: Typed API errors
slug: api-errors

Catch status-specific exceptions with the response body and retry metadata.
*/
namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task Example_InvalidApiKeyAsync()
    {
        using var client = new TypeSafeClient("invalid-key", options: new TypeSafeClientOptions
        {
            Retry = new RetryPolicy { MaxAttempts = 1 },
        });

        var action = async () => await client.Models.ListAsync();

        await action.Should().ThrowAsync<AuthenticationException>();
    }
}
