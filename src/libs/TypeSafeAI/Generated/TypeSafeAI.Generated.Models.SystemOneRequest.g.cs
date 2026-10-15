
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// Content and named questions to evaluate together using a TypeSafe model.
    /// </summary>
    public sealed partial class SystemOneRequest
    {
        /// <summary>
        /// Name or alias of the model to use. Available names are returned by GET /v1/models.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("model")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required string Model { get; set; }

        /// <summary>
        /// The content all questions in this request refer to.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("state")]
        [global::System.Text.Json.Serialization.JsonConverter(typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>>))]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>> State { get; set; }

        /// <summary>
        /// Questions to ask about the content, each with a name you choose. The response uses those names to identify the answers.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("questions")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required global::System.Collections.Generic.Dictionary<string, global::TypeSafeAI.Generated.Question> Questions { get; set; }

        /// <summary>
        /// Media assets keyed by an opaque placeholder (recommend a per-image UUID). Each placeholder, wherever it appears as or inside a string in state, question instructions, or criteria, is replaced with the asset. Image assets require a vision-language model.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("assets")]
        public global::System.Collections.Generic.Dictionary<string, global::TypeSafeAI.Generated.Asset>? Assets { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemOneRequest" /> class.
        /// </summary>
        /// <param name="model">
        /// Name or alias of the model to use. Available names are returned by GET /v1/models.
        /// </param>
        /// <param name="state">
        /// The content all questions in this request refer to.
        /// </param>
        /// <param name="questions">
        /// Questions to ask about the content, each with a name you choose. The response uses those names to identify the answers.
        /// </param>
        /// <param name="assets">
        /// Media assets keyed by an opaque placeholder (recommend a per-image UUID). Each placeholder, wherever it appears as or inside a string in state, question instructions, or criteria, is replaced with the asset. Image assets require a vision-language model.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public SystemOneRequest(
            string model,
            global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>> state,
            global::System.Collections.Generic.Dictionary<string, global::TypeSafeAI.Generated.Question> questions,
            global::System.Collections.Generic.Dictionary<string, global::TypeSafeAI.Generated.Asset>? assets)
        {
            this.Model = model ?? throw new global::System.ArgumentNullException(nameof(model));
            this.State = state;
            this.Questions = questions ?? throw new global::System.ArgumentNullException(nameof(questions));
            this.Assets = assets;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemOneRequest" /> class.
        /// </summary>
        public SystemOneRequest()
        {
        }

    }
}