using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CS0282 // These managed JSON unions are never marshalled; field layout is not a contract.

namespace TypeSafeAI.Generated
{
    public readonly partial struct Question
    {
        /// <summary>Original JSON, including future question variants.</summary>
        [JsonIgnore]
        public JsonElement RawJson { get; init; }
    }

    public readonly partial struct Answer
    {
        /// <summary>Original JSON, including future answer variants.</summary>
        [JsonIgnore]
        public JsonElement RawJson { get; init; }
    }
}

namespace TypeSafeAI
{
    internal sealed class ForwardCompatibleQuestionConverter : Generated.JsonConverters.QuestionJsonConverter
    {
        public override Generated.Question Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var copy = reader;
            using var document = JsonDocument.ParseValue(ref copy);
            var raw = document.RootElement.Clone();
            if (raw.TryGetProperty("type", out var type) && type.GetString() is "noul" or "choice" or "score")
            {
                return base.Read(ref reader, typeToConvert, options) with { RawJson = raw };
            }
            reader = copy;
            return new Generated.Question { RawJson = raw };
        }

        public override void Write(Utf8JsonWriter writer, Generated.Question value, JsonSerializerOptions options)
        {
            if (value.RawJson.ValueKind != JsonValueKind.Undefined) value.RawJson.WriteTo(writer);
            else base.Write(writer, value, options);
        }
    }

    internal sealed class ForwardCompatibleAnswerConverter : Generated.JsonConverters.AnswerJsonConverter
    {
        public override Generated.Answer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var copy = reader;
            using var document = JsonDocument.ParseValue(ref copy);
            var raw = document.RootElement.Clone();
            if (raw.TryGetProperty("type", out var type) && type.GetString() is "noul" or "choice" or "score")
            {
                return base.Read(ref reader, typeToConvert, options) with { RawJson = raw };
            }
            reader = copy;
            return new Generated.Answer { RawJson = raw };
        }

        public override void Write(Utf8JsonWriter writer, Generated.Answer value, JsonSerializerOptions options)
        {
            if (value.RawJson.ValueKind != JsonValueKind.Undefined) value.RawJson.WriteTo(writer);
            else base.Write(writer, value, options);
        }
    }
}
