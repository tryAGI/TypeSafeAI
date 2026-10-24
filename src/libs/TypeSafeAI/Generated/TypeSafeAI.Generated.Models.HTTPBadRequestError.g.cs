
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// Application validation failures returned with HTTP status 400.
    /// </summary>
    public sealed partial class HTTPBadRequestError
    {
        /// <summary>
        /// Explanation of the invalid request. String details and message fields are bounded, with shortened text ending with ….
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("detail")]
        [global::System.Text.Json.Serialization.JsonConverter(typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object>))]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required global::TypeSafeAI.Generated.AnyOf<string, object> Detail { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HTTPBadRequestError" /> class.
        /// </summary>
        /// <param name="detail">
        /// Explanation of the invalid request. String details and message fields are bounded, with shortened text ending with ….
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public HTTPBadRequestError(
            global::TypeSafeAI.Generated.AnyOf<string, object> detail)
        {
            this.Detail = detail;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HTTPBadRequestError" /> class.
        /// </summary>
        public HTTPBadRequestError()
        {
        }

    }
}