/*
order: 35
title: Custom response projection
slug: custom-response

Project named answers into an application-owned response type with source-generated JSON metadata.
*/
using System.Text.Json.Serialization;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task Example_CustomResponseAsync()
    {
        using var client = GetAuthenticatedClient();
        var questions = new Dictionary<string, Question>
        {
            ["intent"] = ChoiceQuestion.FromValues(["billing", "support", "sales"]),
        };

        var response = await client.SystemOneAsync(
            (JsonContent)"I need a refund for a duplicate invoice.",
            questions,
            ExampleJsonContext.Default.TriageProjection);

        response.Intent.Choice.Should().BeOneOf("billing", "support", "sales");
    }
}

internal sealed record TriageProjection(ChoiceProjection Intent);
internal sealed record ChoiceProjection(
    string Type,
    string Choice,
    double Confidence,
    IReadOnlyDictionary<string, double> Probabilities);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(TriageProjection))]
internal sealed partial class ExampleJsonContext : JsonSerializerContext;
