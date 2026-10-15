
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// Media assets keyed by an opaque placeholder (recommend a per-image UUID). Each placeholder, wherever it appears as or inside a string in state, question instructions, or criteria, is replaced with the asset. Image assets require a vision-language model.
    /// </summary>
    public sealed partial class SystemOneRequestAssets
    {

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

    }
}