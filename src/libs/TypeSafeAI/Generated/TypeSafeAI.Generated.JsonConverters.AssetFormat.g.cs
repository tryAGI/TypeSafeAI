#nullable enable

namespace TypeSafeAI.Generated.JsonConverters
{
    /// <inheritdoc />
    public sealed class AssetFormatJsonConverter : global::System.Text.Json.Serialization.JsonConverter<global::TypeSafeAI.Generated.AssetFormat>
    {
        /// <inheritdoc />
        public override global::TypeSafeAI.Generated.AssetFormat Read(
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
                        return global::TypeSafeAI.Generated.AssetFormatExtensions.ToEnum(stringValue) ?? default;
                    }

                    break;
                }
                case global::System.Text.Json.JsonTokenType.Number:
                {
                    var numValue = reader.GetInt32();
                    return (global::TypeSafeAI.Generated.AssetFormat)numValue;
                }
                case global::System.Text.Json.JsonTokenType.Null:
                {
                    return default(global::TypeSafeAI.Generated.AssetFormat);
                }
                default:
                    throw new global::System.ArgumentOutOfRangeException(nameof(reader));
            }

            return default;
        }

        /// <inheritdoc />
        public override void Write(
            global::System.Text.Json.Utf8JsonWriter writer,
            global::TypeSafeAI.Generated.AssetFormat value,
            global::System.Text.Json.JsonSerializerOptions options)
        {
            writer = writer ?? throw new global::System.ArgumentNullException(nameof(writer));

            writer.WriteStringValue(global::TypeSafeAI.Generated.AssetFormatExtensions.ToValueString(value));
        }
    }
}
