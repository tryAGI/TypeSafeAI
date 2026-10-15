
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// A question that locates what its instructions describe in the request's image. Requests with box questions must send the header TypeSafe-Danger-Zone-Images: true.
    /// </summary>
    public sealed partial class BoxQuestion
    {
        /// <summary>
        /// Identifies a box question.
        /// </summary>
        /// <default>"box"</default>
        [global::System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; } = "box";

        /// <summary>
        /// What to locate in the image.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("instructions")]
        [global::System.Text.Json.Serialization.JsonConverter(typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>>))]
        public global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>>? Instructions { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxQuestion" /> class.
        /// </summary>
        /// <param name="instructions">
        /// What to locate in the image.
        /// </param>
        /// <param name="type">
        /// Identifies a box question.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public BoxQuestion(
            global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>>? instructions,
            string type = "box")
        {
            this.Type = type;
            this.Instructions = instructions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxQuestion" /> class.
        /// </summary>
        public BoxQuestion()
        {
        }

    }
}