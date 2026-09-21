#nullable enable
#pragma warning disable CS0618 // Type or member is obsolete

namespace TypeSafeAI.Generated.JsonConverters
{
    /// <inheritdoc />
    public class AnswerJsonConverter : global::System.Text.Json.Serialization.JsonConverter<global::TypeSafeAI.Generated.Answer>
    {
        /// <inheritdoc />
        public override global::TypeSafeAI.Generated.Answer Read(
            ref global::System.Text.Json.Utf8JsonReader reader,
            global::System.Type typeToConvert,
            global::System.Text.Json.JsonSerializerOptions options)
        {
            options = options ?? throw new global::System.ArgumentNullException(nameof(options));
            var typeInfoResolver = options.TypeInfoResolver ?? throw new global::System.InvalidOperationException("TypeInfoResolver is not set.");


            var readerCopy = reader;
            var discriminatorTypeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.AnswerDiscriminator), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.AnswerDiscriminator> ??
                            throw new global::System.InvalidOperationException($"Cannot get type info for {nameof(global::TypeSafeAI.Generated.AnswerDiscriminator)}");
            var discriminator = global::System.Text.Json.JsonSerializer.Deserialize(ref readerCopy, discriminatorTypeInfo);

            global::TypeSafeAI.Generated.NoulAnswer? noul = default;
            if (discriminator?.Type == global::TypeSafeAI.Generated.AnswerDiscriminatorType.Noul)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.NoulAnswer), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.NoulAnswer> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {nameof(global::TypeSafeAI.Generated.NoulAnswer)}");
                noul = global::System.Text.Json.JsonSerializer.Deserialize(ref reader, typeInfo);
            }
            global::TypeSafeAI.Generated.ScoreAnswer? score = default;
            if (discriminator?.Type == global::TypeSafeAI.Generated.AnswerDiscriminatorType.Score)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.ScoreAnswer), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.ScoreAnswer> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {nameof(global::TypeSafeAI.Generated.ScoreAnswer)}");
                score = global::System.Text.Json.JsonSerializer.Deserialize(ref reader, typeInfo);
            }
            global::TypeSafeAI.Generated.ChoiceAnswer? choice = default;
            if (discriminator?.Type == global::TypeSafeAI.Generated.AnswerDiscriminatorType.Choice)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.ChoiceAnswer), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.ChoiceAnswer> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {nameof(global::TypeSafeAI.Generated.ChoiceAnswer)}");
                choice = global::System.Text.Json.JsonSerializer.Deserialize(ref reader, typeInfo);
            }

            var __value = new global::TypeSafeAI.Generated.Answer(
                discriminator?.Type,
                noul,

                score,

                choice
                );

            return __value;
        }

        /// <inheritdoc />
        public override void Write(
            global::System.Text.Json.Utf8JsonWriter writer,
            global::TypeSafeAI.Generated.Answer value,
            global::System.Text.Json.JsonSerializerOptions options)
        {
            options = options ?? throw new global::System.ArgumentNullException(nameof(options));
            var typeInfoResolver = options.TypeInfoResolver ?? throw new global::System.InvalidOperationException("TypeInfoResolver is not set.");

            if (value.IsNoul)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.NoulAnswer), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.NoulAnswer?> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {typeof(global::TypeSafeAI.Generated.NoulAnswer).Name}");
                global::System.Text.Json.JsonSerializer.Serialize(writer, value.Noul!, typeInfo);
            }
            else if (value.IsScore)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.ScoreAnswer), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.ScoreAnswer?> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {typeof(global::TypeSafeAI.Generated.ScoreAnswer).Name}");
                global::System.Text.Json.JsonSerializer.Serialize(writer, value.Score!, typeInfo);
            }
            else if (value.IsChoice)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.ChoiceAnswer), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.ChoiceAnswer?> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {typeof(global::TypeSafeAI.Generated.ChoiceAnswer).Name}");
                global::System.Text.Json.JsonSerializer.Serialize(writer, value.Choice!, typeInfo);
            }
        }
    }
}