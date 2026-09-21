using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;

namespace TypeSafeAI.Extensions.AI;

/// <summary>Builds structured TypeSafe state from Microsoft.Extensions.AI chat messages.</summary>
public static class ChatState
{
    public static JsonContent FromMessages(
        IEnumerable<ChatMessage> messages,
        ChatResponse? response = null,
        IReadOnlyDictionary<string, string>? context = null)
    {
        ArgumentNullException.ThrowIfNull(messages);
        var conversation = new JsonArray();
        string? latestUser = null;
        foreach (var message in messages)
        {
            var text = message.Text;
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }
            conversation.Add((JsonNode)new JsonObject { ["role"] = message.Role.Value, ["text"] = text });
            if (message.Role == ChatRole.User)
            {
                latestUser = text;
            }
        }

        var state = new JsonObject { ["conversation"] = conversation };
        if (latestUser is not null)
        {
            state["latest_user_message"] = latestUser;
        }
        if (response is not null)
        {
            state["response"] = response.Text;
        }
        if (context is { Count: > 0 })
        {
            var values = new JsonObject();
            foreach (var pair in context)
            {
                values[pair.Key] = pair.Value;
            }
            state["context"] = values;
        }
        return JsonContent.FromNode(state);
    }

    public static string? LatestUserText(IEnumerable<ChatMessage> messages) =>
        messages.LastOrDefault(static message => message.Role == ChatRole.User)?.Text;
}
