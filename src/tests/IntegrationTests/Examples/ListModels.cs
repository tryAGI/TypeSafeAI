/*
order: 40
title: List models
slug: list-models

Discover the model names and aliases available to the authenticated account.
*/
namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task Example_ListModelsAsync()
    {
        using var client = GetAuthenticatedClient();

        var models = await client.Models.ListAsync();

        models.Should().Contain(model => model.Name == "jev-latest");
        models.Count(model => model.ReleasedAt.HasValue).Should().Be(models.Count);
    }
}
