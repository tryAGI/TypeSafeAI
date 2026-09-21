
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// Request validation failures returned with HTTP status 422.
    /// </summary>
    public sealed partial class HTTPValidationError
    {
        /// <summary>
        /// Validation errors describing which request values are missing or invalid.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("detail")]
        public global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.ValidationError>? Detail { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HTTPValidationError" /> class.
        /// </summary>
        /// <param name="detail">
        /// Validation errors describing which request values are missing or invalid.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public HTTPValidationError(
            global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.ValidationError>? detail)
        {
            this.Detail = detail;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HTTPValidationError" /> class.
        /// </summary>
        public HTTPValidationError()
        {
        }

    }
}