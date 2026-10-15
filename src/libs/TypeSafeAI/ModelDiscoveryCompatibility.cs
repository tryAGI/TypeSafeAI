using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace TypeSafeAI.Generated
{
    public sealed partial class ModelMetadata
    {
        [JsonIgnore]
        internal JsonElement OriginalJson { get; set; }
    }

    public sealed partial class ModelMetadataList
    {
        [JsonIgnore]
        internal JsonElement OriginalJson { get; set; }
    }
}

namespace TypeSafeAI
{
    // Celeris serves the OpenAI model-list envelope at the same /v1/models route.
    // Adapt its JSON contract without replacing generated HTTP/auth/retry behavior.
    internal sealed class CompatibleModelListConverter : JsonConverter<Generated.ModelMetadataList>
    {
        public override Generated.ModelMetadataList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !(root.TryGetProperty("data", out var items) || root.TryGetProperty("models", out items)) ||
                items.ValueKind != JsonValueKind.Array)
                throw new JsonException("Model discovery requires a data or models array.");
            var models = new List<Generated.ModelMetadata>();
            foreach (var item in items.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) throw new JsonException("A model must be an object.");
                var name = String(item, "id") ?? String(item, "name");
                if (string.IsNullOrWhiteSpace(name)) throw new JsonException("A model must have a nonempty id or name.");
                models.Add(new Generated.ModelMetadata(name, String(item, "description") ?? "", String(item, "release_date") ?? "")
                {
                    OriginalJson = item.Clone(),
                    AdditionalProperties = item.EnumerateObject().Where(static p => p.Name is not ("name" or "description" or "release_date"))
                        .ToDictionary(static p => p.Name, static p => p.Value.Clone(), StringComparer.Ordinal),
                });
            }
            return new Generated.ModelMetadataList(models) { OriginalJson = root.Clone() };
        }

        public override void Write(Utf8JsonWriter writer, Generated.ModelMetadataList value, JsonSerializerOptions options)
        {
            if (value.OriginalJson.ValueKind != JsonValueKind.Undefined)
            {
                value.OriginalJson.WriteTo(writer);
                return;
            }
            writer.WriteStartObject();
            writer.WritePropertyName("models");
            writer.WriteStartArray();
            foreach (var model in value.Models)
                JsonSerializer.Serialize(writer, model, (JsonTypeInfo<Generated.ModelMetadata>)Generated.SourceGenerationContext.Default.GetTypeInfo(typeof(Generated.ModelMetadata))!);
            writer.WriteEndArray();
            foreach (var pair in value.AdditionalProperties)
            {
                writer.WritePropertyName(pair.Key);
                pair.Value.WriteTo(writer);
            }
            writer.WriteEndObject();
        }

        internal static ModelMetadata Project(Generated.ModelMetadata model, JsonTypeInfo<Generated.ModelMetadata> typeInfo)
        {
            var raw = model.OriginalJson.ValueKind == JsonValueKind.Undefined
                ? JsonSerializer.SerializeToElement(model, typeInfo) : model.OriginalJson;
            DateTimeOffset? createdAt = null;
            if (raw.TryGetProperty("created", out var created) && created.ValueKind != JsonValueKind.Null)
            {
                if (created.ValueKind != JsonValueKind.Number || !created.TryGetInt64(out var seconds))
                    throw new JsonException("Model created must be Unix seconds.");
                try { createdAt = DateTimeOffset.FromUnixTimeSeconds(seconds); }
                catch (ArgumentOutOfRangeException exception) { throw new JsonException("Model created is outside the supported date range.", exception); }
            }
            return new ModelMetadata(model.Name, model.Description, model.ReleaseDate)
            {
                CreatedAt = createdAt,
                OwnedBy = String(raw, "owned_by"),
                RawJson = raw.Clone(),
            };
        }

        private static string? String(JsonElement item, string name)
        {
            if (!item.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null) return null;
            if (value.ValueKind != JsonValueKind.String) throw new JsonException($"Model {name} must be a string.");
            return value.GetString();
        }
    }
}
