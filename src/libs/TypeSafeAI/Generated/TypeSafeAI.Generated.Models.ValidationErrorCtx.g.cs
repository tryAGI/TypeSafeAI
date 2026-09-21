
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// Additional context used to explain the validation failure.
    /// </summary>
    public sealed partial class ValidationErrorCtx
    {

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

    }
}