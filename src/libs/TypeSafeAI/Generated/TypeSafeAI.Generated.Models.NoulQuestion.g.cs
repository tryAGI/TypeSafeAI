
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// A yes/no question or statement, answered with the probability of yes or true.
    /// </summary>
    public sealed partial class NoulQuestion
    {
        /// <summary>
        /// Identifies a yes/no question or statement.
        /// </summary>
        /// <default>"noul"</default>
        [global::System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; } = "noul";

        /// <summary>
        /// The yes/no question or statement to evaluate.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("instructions")]
        [global::System.Text.Json.Serialization.JsonConverter(typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>, object>))]
        public global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>, object>? Instructions { get; set; }

        /// <summary>
        /// Criteria clarifying what counts as a yes or no answer.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("criteria")]
        public global::TypeSafeAI.Generated.NoulCriteria? Criteria { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NoulQuestion" /> class.
        /// </summary>
        /// <param name="instructions">
        /// The yes/no question or statement to evaluate.
        /// </param>
        /// <param name="criteria">
        /// Criteria clarifying what counts as a yes or no answer.
        /// </param>
        /// <param name="type">
        /// Identifies a yes/no question or statement.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public NoulQuestion(
            global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>, object>? instructions,
            global::TypeSafeAI.Generated.NoulCriteria? criteria,
            string type = "noul")
        {
            this.Type = type;
            this.Instructions = instructions;
            this.Criteria = criteria;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoulQuestion" /> class.
        /// </summary>
        public NoulQuestion()
        {
        }

    }
}