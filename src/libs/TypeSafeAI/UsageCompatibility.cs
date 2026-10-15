using System.Text.Json;
using System.Text.Json.Serialization;

namespace TypeSafeAI.Generated
{
    public sealed partial class Usage
    {
        [JsonIgnore]
        internal JsonElement OriginalJson { get; set; }
    }
}

namespace TypeSafeAI
{
    // The published JS/Python contracts permit null counts, while the OpenAPI still requires ints.
    // Preserve the wire payload outside Generated/ until the upstream schema catches up.
    internal sealed class NullableUsageConverter : JsonConverter<Generated.Usage>
    {
        public override Generated.Usage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;
            static int Count(JsonElement element, string name) =>
                element.TryGetProperty(name, out var count) && count.ValueKind != JsonValueKind.Null ? count.GetInt32() : 0;
            return new Generated.Usage(Count(root, "input_tokens"), Count(root, "output_tokens"))
            {
                OriginalJson = root.Clone(),
            };
        }

        public override void Write(Utf8JsonWriter writer, Generated.Usage value, JsonSerializerOptions options)
        {
            if (value.OriginalJson.ValueKind != JsonValueKind.Undefined)
            {
                value.OriginalJson.WriteTo(writer);
                return;
            }
            writer.WriteStartObject();
            writer.WriteNumber("input_tokens", value.InputTokens);
            writer.WriteNumber("output_tokens", value.OutputTokens);
            foreach (var pair in value.AdditionalProperties)
            {
                writer.WritePropertyName(pair.Key);
                pair.Value.WriteTo(writer);
            }
            writer.WriteEndObject();
        }
    }
}
