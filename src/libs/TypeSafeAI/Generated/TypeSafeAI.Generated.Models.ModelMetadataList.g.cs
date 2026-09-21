
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// Models and aliases available to the authenticated account.
    /// </summary>
    public sealed partial class ModelMetadataList
    {
        /// <summary>
        /// Available models and aliases. Use a model's name in POST /v1/systemone requests.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("models")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.ModelMetadata> Models { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMetadataList" /> class.
        /// </summary>
        /// <param name="models">
        /// Available models and aliases. Use a model's name in POST /v1/systemone requests.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public ModelMetadataList(
            global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.ModelMetadata> models)
        {
            this.Models = models ?? throw new global::System.ArgumentNullException(nameof(models));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMetadataList" /> class.
        /// </summary>
        public ModelMetadataList()
        {
        }

    }
}