#nullable enable

namespace TypeSafeAI.Generated.JsonConverters
{
    /// <inheritdoc />
    public sealed class SystemoneV1SystemonePostTypeSafeDangerZoneLongContextNullableJsonConverter : global::System.Text.Json.Serialization.JsonConverter<global::TypeSafeAI.Generated.SystemoneV1SystemonePostTypeSafeDangerZoneLongContext?>
    {
        /// <inheritdoc />
        public override global::TypeSafeAI.Generated.SystemoneV1SystemonePostTypeSafeDangerZoneLongContext? Read(
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
                        return global::TypeSafeAI.Generated.SystemoneV1SystemonePostTypeSafeDangerZoneLongContextExtensions.ToEnum(stringValue);
                    }

                    break;
                }
                case global::System.Text.Json.JsonTokenType.Number:
                {
                    var numValue = reader.GetInt32();
                    return (global::TypeSafeAI.Generated.SystemoneV1SystemonePostTypeSafeDangerZoneLongContext)numValue;
                }
                case global::System.Text.Json.JsonTokenType.Null:
                {
                    return default(global::TypeSafeAI.Generated.SystemoneV1SystemonePostTypeSafeDangerZoneLongContext?);
                }
                default:
                    throw new global::System.ArgumentOutOfRangeException(nameof(reader));
            }

            return default;
        }

        /// <inheritdoc />
        public override void Write(
            global::System.Text.Json.Utf8JsonWriter writer,
            global::TypeSafeAI.Generated.SystemoneV1SystemonePostTypeSafeDangerZoneLongContext? value,
            global::System.Text.Json.JsonSerializerOptions options)
        {
            writer = writer ?? throw new global::System.ArgumentNullException(nameof(writer));

            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(global::TypeSafeAI.Generated.SystemoneV1SystemonePostTypeSafeDangerZoneLongContextExtensions.ToValueString(value.Value));
            }
        }
    }
}
