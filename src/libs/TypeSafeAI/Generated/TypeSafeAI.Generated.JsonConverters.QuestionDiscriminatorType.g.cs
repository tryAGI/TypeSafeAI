#nullable enable

namespace TypeSafeAI.Generated.JsonConverters
{
    /// <inheritdoc />
    public sealed class QuestionDiscriminatorTypeJsonConverter : global::System.Text.Json.Serialization.JsonConverter<global::TypeSafeAI.Generated.QuestionDiscriminatorType>
    {
        /// <inheritdoc />
        public override global::TypeSafeAI.Generated.QuestionDiscriminatorType Read(
            ref global::System.Text.Json.Utf8JsonReader reader,
            global::System.Type typeToConvert,
            global::System.Text.Json.JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case global::System.Text.Json.JsonTokenType.String:
                {
                    var stringValue = reader.GetString();
                    if (stringValue != null)
                    {
                        return global::TypeSafeAI.Generated.QuestionDiscriminatorTypeExtensions.ToEnum(stringValue) ?? default;
                    }

                    break;
                }
                case global::System.Text.Json.JsonTokenType.Number:
                {
                    var numValue = reader.GetInt32();
                    return (global::TypeSafeAI.Generated.QuestionDiscriminatorType)numValue;
                }
                case global::System.Text.Json.JsonTokenType.Null:
                {
                    return default(global::TypeSafeAI.Generated.QuestionDiscriminatorType);
                }
                default:
                    throw new global::System.ArgumentOutOfRangeException(nameof(reader));
            }

            return default;
        }

        /// <inheritdoc />
        public override void Write(
            global::System.Text.Json.Utf8JsonWriter writer,
            global::TypeSafeAI.Generated.QuestionDiscriminatorType value,
            global::System.Text.Json.JsonSerializerOptions options)
        {
            writer = writer ?? throw new global::System.ArgumentNullException(nameof(writer));

            writer.WriteStringValue(global::TypeSafeAI.Generated.QuestionDiscriminatorTypeExtensions.ToValueString(value));
        }
    }
}
