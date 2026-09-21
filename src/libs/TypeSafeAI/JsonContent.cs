using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace TypeSafeAI;

/// <summary>JSON-compatible state or instruction content with NativeAOT-safe factories.</summary>
public readonly struct JsonContent
{
    private readonly JsonElement _value;
    private static readonly JsonElement NullValue = Parse("null")._value;

    private JsonContent(JsonElement value) => _value = value.Clone();

    /// <summary>Gets the underlying immutable JSON value.</summary>
    public JsonElement Value => _value.ValueKind == JsonValueKind.Undefined
        ? NullValue
        : _value;

    /// <summary>Creates content from an arbitrary value using source-generated metadata.</summary>
    public static JsonContent Create<T>(T value, JsonTypeInfo<T> typeInfo) =>
        new(JsonSerializer.SerializeToElement(value, typeInfo));

    /// <summary>Parses a JSON value.</summary>
    public static JsonContent Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return new(document.RootElement);
    }

    /// <summary>Creates content from a DOM value.</summary>
    public static JsonContent FromNode(JsonNode? node) =>
        node is null ? Null : new(JsonSerializer.SerializeToElement(node, JsonDefaults.Node));

    /// <summary>Represents JSON null.</summary>
    public static JsonContent Null => default;

    public static implicit operator JsonContent(string value) =>
        new(JsonSerializer.SerializeToElement(value, JsonDefaults.String));

    public static implicit operator JsonContent(JsonElement value) => new(value);

    public static implicit operator JsonContent(JsonNode? value) => FromNode(value);

    public override string ToString() => Value.GetRawText();
}

internal static class JsonDefaults
{
    internal static JsonTypeInfo<string> String => TypeSafeJsonContext.Default.String;
    internal static JsonTypeInfo<JsonNode> Node => TypeSafeJsonContext.Default.JsonNode;
}
