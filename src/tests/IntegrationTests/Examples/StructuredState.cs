/*
order: 30
title: Structured state
slug: structured-state

Send structured JSON state without reflection, including in NativeAOT applications.
*/
using System.Text.Json.Nodes;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task Example_StructuredStateAsync()
    {
        using var client = GetAuthenticatedClient();
        var state = JsonContent.FromNode(new JsonObject
        {
            ["subject"] = "Duplicate payment",
            ["message"] = "The same invoice appears twice.",
            ["customer_tier"] = "enterprise",
        });
        var questions = new Dictionary<string, Question>
        {
            ["intent"] = ChoiceQuestion.FromValues(["billing", "support", "sales"]),
        };

        var response = await client.SystemOneAsync(state, questions);

        response.Get<ChoiceAnswer>("intent").Choice.Should().BeOneOf("billing", "support", "sales");
    }
}
