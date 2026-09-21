using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;

namespace TypeSafeAI.Extensions.AI;

/// <summary>Exposes TypeSafe judgments as tools an AI agent can call.</summary>
public static class TypeSafeAIFunctions
{
    public static AIFunction Create(
        ITypeSafeClient client,
        IReadOnlyDictionary<string, Question> questions,
        string name,
        string description,
        string stateDescription = "The text to judge.",
        RequestOptions? requestOptions = null,
        string? model = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(questions);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        return new JudgeFunction(client, questions, name, description, stateDescription, requestOptions, model);
    }

    private sealed class JudgeFunction(
        ITypeSafeClient client,
        IReadOnlyDictionary<string, Question> questions,
        string name,
        string description,
        string stateDescription,
        RequestOptions? requestOptions,
        string? model) : AIFunction
    {
        public override string Name { get; } = name;
        public override string Description { get; } = description;
        public override JsonElement JsonSchema { get; } = BuildSchema(stateDescription);

        protected override async ValueTask<object?> InvokeCoreAsync(
            AIFunctionArguments arguments,
            CancellationToken cancellationToken)
        {
            if (!arguments.TryGetValue("state", out var raw) || raw is null)
            {
                throw new ArgumentException("The 'state' argument is required.", nameof(arguments));
            }
            var state = raw switch
            {
                string text => (JsonContent)text,
                JsonElement element => (JsonContent)element,
                JsonNode node => JsonContent.FromNode(node),
                JsonContent content => content,
                _ => (JsonContent)(raw.ToString() ?? string.Empty),
            };
            var response = await client.SystemOneAsync(
                state, questions, model, requestOptions, cancellationToken).ConfigureAwait(false);
            return JsonNode.Parse(response.RawJson.GetRawText());
        }

        private static JsonElement BuildSchema(string description)
        {
            var schema = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["state"] = new JsonObject { ["type"] = "string", ["description"] = description },
                },
                ["required"] = new JsonArray((JsonNode)"state"),
            };
            return JsonSerializer.SerializeToElement(
                schema,
                AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonObject)));
        }
    }
}
