#nullable enable
#pragma warning disable CS0618 // Type or member is obsolete

namespace TypeSafeAI.Generated.JsonConverters
{
    /// <inheritdoc />
    public class QuestionJsonConverter : global::System.Text.Json.Serialization.JsonConverter<global::TypeSafeAI.Generated.Question>
    {
        /// <inheritdoc />
        public override global::TypeSafeAI.Generated.Question Read(
            ref global::System.Text.Json.Utf8JsonReader reader,
            global::System.Type typeToConvert,
            global::System.Text.Json.JsonSerializerOptions options)
        {
            options = options ?? throw new global::System.ArgumentNullException(nameof(options));
            var typeInfoResolver = options.TypeInfoResolver ?? throw new global::System.InvalidOperationException("TypeInfoResolver is not set.");


            var readerCopy = reader;
            var discriminatorTypeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.QuestionDiscriminator), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.QuestionDiscriminator> ??
                            throw new global::System.InvalidOperationException($"Cannot get type info for {nameof(global::TypeSafeAI.Generated.QuestionDiscriminator)}");
            var discriminator = global::System.Text.Json.JsonSerializer.Deserialize(ref readerCopy, discriminatorTypeInfo);

            global::TypeSafeAI.Generated.NoulQuestion? noul = default;
            if (discriminator?.Type == global::TypeSafeAI.Generated.QuestionDiscriminatorType.Noul)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.NoulQuestion), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.NoulQuestion> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {nameof(global::TypeSafeAI.Generated.NoulQuestion)}");
                noul = global::System.Text.Json.JsonSerializer.Deserialize(ref reader, typeInfo);
            }
            global::TypeSafeAI.Generated.ChoiceQuestion? choice = default;
            if (discriminator?.Type == global::TypeSafeAI.Generated.QuestionDiscriminatorType.Choice)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.ChoiceQuestion), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.ChoiceQuestion> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {nameof(global::TypeSafeAI.Generated.ChoiceQuestion)}");
                choice = global::System.Text.Json.JsonSerializer.Deserialize(ref reader, typeInfo);
            }
            global::TypeSafeAI.Generated.ScoreQuestion? score = default;
            if (discriminator?.Type == global::TypeSafeAI.Generated.QuestionDiscriminatorType.Score)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.ScoreQuestion), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.ScoreQuestion> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {nameof(global::TypeSafeAI.Generated.ScoreQuestion)}");
                score = global::System.Text.Json.JsonSerializer.Deserialize(ref reader, typeInfo);
            }

            var __value = new global::TypeSafeAI.Generated.Question(
                discriminator?.Type,
                noul,

                choice,

                score
                );

            return __value;
        }

        /// <inheritdoc />
        public override void Write(
            global::System.Text.Json.Utf8JsonWriter writer,
            global::TypeSafeAI.Generated.Question value,
            global::System.Text.Json.JsonSerializerOptions options)
        {
            options = options ?? throw new global::System.ArgumentNullException(nameof(options));
            var typeInfoResolver = options.TypeInfoResolver ?? throw new global::System.InvalidOperationException("TypeInfoResolver is not set.");

            if (value.IsNoul)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.NoulQuestion), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.NoulQuestion?> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {typeof(global::TypeSafeAI.Generated.NoulQuestion).Name}");
                global::System.Text.Json.JsonSerializer.Serialize(writer, value.Noul!, typeInfo);
            }
            else if (value.IsChoice)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.ChoiceQuestion), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.ChoiceQuestion?> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {typeof(global::TypeSafeAI.Generated.ChoiceQuestion).Name}");
                global::System.Text.Json.JsonSerializer.Serialize(writer, value.Choice!, typeInfo);
            }
            else if (value.IsScore)
            {
                var typeInfo = typeInfoResolver.GetTypeInfo(typeof(global::TypeSafeAI.Generated.ScoreQuestion), options) as global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<global::TypeSafeAI.Generated.ScoreQuestion?> ??
                               throw new global::System.InvalidOperationException($"Cannot get type info for {typeof(global::TypeSafeAI.Generated.ScoreQuestion).Name}");
                global::System.Text.Json.JsonSerializer.Serialize(writer, value.Score!, typeInfo);
            }
        }
    }
}