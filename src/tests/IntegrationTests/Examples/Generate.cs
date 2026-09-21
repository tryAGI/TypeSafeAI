/*
order: 10
title: Generate
slug: generate

Create a client from `TYPESAFE_API_KEY` and discover the models available to the account.
*/

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task Example_Generate()
    {
        using var client = GetAuthenticatedClient();

        var models = await client.Models.ListAsync();

        models.Should().NotBeEmpty();
    }
}
