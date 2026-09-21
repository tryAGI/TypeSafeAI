
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// Criteria defining what counts as a yes or no answer.
    /// </summary>
    public sealed partial class NoulCriteria
    {
        /// <summary>
        /// What counts as a yes answer.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("true")]
        [global::System.Text.Json.Serialization.JsonConverter(typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>, object>))]
        public global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>, object>? True { get; set; }

        /// <summary>
        /// What counts as a no answer.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("false")]
        [global::System.Text.Json.Serialization.JsonConverter(typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>, object>))]
        public global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>, object>? False { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NoulCriteria" /> class.
        /// </summary>
        /// <param name="true">
        /// What counts as a yes answer.
        /// </param>
        /// <param name="false">
        /// What counts as a no answer.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public NoulCriteria(
            global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>, object>? @true,
            global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>, object>? @false)
        {
            this.True = @true;
            this.False = @false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoulCriteria" /> class.
        /// </summary>
        public NoulCriteria()
        {
        }

    }
}