
#nullable enable

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant

namespace TypeSafeAI.Generated
{
    /// <summary>
    ///
    /// </summary>
    [global::System.Text.Json.Serialization.JsonSourceGenerationOptions(
        DefaultIgnoreCondition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = new global::System.Type[]
        {
            typeof(global::TypeSafeAI.Generated.JsonConverters.AnswerDiscriminatorTypeJsonConverter),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnswerDiscriminatorTypeNullableJsonConverter),

            typeof(global::TypeSafeAI.Generated.JsonConverters.QuestionDiscriminatorTypeJsonConverter),

            typeof(global::TypeSafeAI.Generated.JsonConverters.QuestionDiscriminatorTypeNullableJsonConverter),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnswerJsonConverter),

            typeof(global::TypeSafeAI.Generated.JsonConverters.QuestionJsonConverter),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>, object>),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>, object>),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>, object>),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>, object>),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>, object>),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>>),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>, object>),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>>),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>>),

            typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, int?>),

            typeof(global::TypeSafeAI.Generated.JsonConverters.UnixTimestampJsonConverter),
        })]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.JsonSerializerContextTypes))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.List<object>), TypeInfoPropertyName = "SystemCollectionsGeneric_ObjectList")]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.Answer), TypeInfoPropertyName = "Answer2")]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.NoulAnswer))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.ScoreAnswer))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.ChoiceAnswer))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.AnswerDiscriminator))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.AnswerDiscriminatorType), TypeInfoPropertyName = "AnswerDiscriminatorType2")]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(string))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(double))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.Dictionary<string, double>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.ChoiceQuestion))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>, object>), TypeInfoPropertyName = "AnyOfStringObjectIListObjectObject2")]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(object))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.IList<object>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.HTTPValidationError))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.ValidationError>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.ValidationError))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.ModelMetadata))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.ModelMetadataList))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.ModelMetadata>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.NoulCriteria))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.NoulQuestion))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.Question), TypeInfoPropertyName = "Question2")]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.ScoreQuestion))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.QuestionDiscriminator))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.QuestionDiscriminatorType), TypeInfoPropertyName = "QuestionDiscriminatorType2")]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>>), TypeInfoPropertyName = "AnyOfStringObjectIListObject2")]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>>>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.SystemOneRequest))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.Dictionary<string, global::TypeSafeAI.Generated.Question>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.SystemOneResponse))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.Dictionary<string, global::TypeSafeAI.Generated.Answer>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.Usage))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(int))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.AnyOf<string, int?>>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.AnyOf<string, int?>), TypeInfoPropertyName = "AnyOfStringInt322")]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.List<object>, object>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.List<global::TypeSafeAI.Generated.ValidationError>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.List<global::TypeSafeAI.Generated.ModelMetadata>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.List<object>>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.List<global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.List<object>>>))]
    [global::System.Text.Json.Serialization.JsonSerializable(typeof(global::System.Collections.Generic.List<global::TypeSafeAI.Generated.AnyOf<string, int?>>))]
    public sealed partial class SourceGenerationContext : global::System.Text.Json.Serialization.JsonSerializerContext
    {
    }
}