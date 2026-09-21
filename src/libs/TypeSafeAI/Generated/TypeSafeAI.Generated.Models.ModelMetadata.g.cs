
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// A model or model alias available to the authenticated account.
    /// </summary>
    public sealed partial class ModelMetadata
    {
        /// <summary>
        /// Model name or alias accepted by the request's model field.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("name")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required string Name { get; set; }

        /// <summary>
        /// Human-readable description of the model and its capabilities.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("description")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required string Description { get; set; }

        /// <summary>
        /// Model release date, formatted as YYYY-MM-DD.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("release_date")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required string ReleaseDate { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMetadata" /> class.
        /// </summary>
        /// <param name="name">
        /// Model name or alias accepted by the request's model field.
        /// </param>
        /// <param name="description">
        /// Human-readable description of the model and its capabilities.
        /// </param>
        /// <param name="releaseDate">
        /// Model release date, formatted as YYYY-MM-DD.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public ModelMetadata(
            string name,
            string description,
            string releaseDate)
        {
            this.Name = name ?? throw new global::System.ArgumentNullException(nameof(name));
            this.Description = description ?? throw new global::System.ArgumentNullException(nameof(description));
            this.ReleaseDate = releaseDate ?? throw new global::System.ArgumentNullException(nameof(releaseDate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMetadata" /> class.
        /// </summary>
        public ModelMetadata()
        {
        }

    }
}