
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// The JSON request as the first part, named request, then each image as its own part. Every image part needs a unique, non-empty name and must be used by exactly one asset's part.
    /// </summary>
    public sealed partial class SystemoneV1SystemonePostRequest
    {
        /// <summary>
        /// The JSON request, as the first part.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("request")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required object Request { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemoneV1SystemonePostRequest" /> class.
        /// </summary>
        /// <param name="request">
        /// The JSON request, as the first part.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public SystemoneV1SystemonePostRequest(
            object request)
        {
            this.Request = request ?? throw new global::System.ArgumentNullException(nameof(request));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemoneV1SystemonePostRequest" /> class.
        /// </summary>
        public SystemoneV1SystemonePostRequest()
        {
        }

    }
}