
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// A question that assigns a score using an ordered rubric.
    /// </summary>
    public sealed partial class ScoreQuestion
    {
        /// <summary>
        /// Identifies a question that rates the content using the levels in criteria.
        /// </summary>
        /// <default>"score"</default>
        [global::System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; } = "score";

        /// <summary>
        /// What the model should rate.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("instructions")]
        [global::System.Text.Json.Serialization.JsonConverter(typeof(global::TypeSafeAI.Generated.JsonConverters.AnyOfJsonConverter<string, object, global::System.Collections.Generic.IList<object>, object>))]
        public global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>, object>? Instructions { get; set; }

        /// <summary>
        /// Ordered descriptions of the score levels. Each description's position determines its score, starting at zero.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("criteria")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>>> Criteria { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreQuestion" /> class.
        /// </summary>
        /// <param name="criteria">
        /// Ordered descriptions of the score levels. Each description's position determines its score, starting at zero.
        /// </param>
        /// <param name="instructions">
        /// What the model should rate.
        /// </param>
        /// <param name="type">
        /// Identifies a question that rates the content using the levels in criteria.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public ScoreQuestion(
            global::System.Collections.Generic.IList<global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>>> criteria,
            global::TypeSafeAI.Generated.AnyOf<string, object, global::System.Collections.Generic.IList<object>, object>? instructions,
            string type = "score")
        {
            this.Type = type;
            this.Instructions = instructions;
            this.Criteria = criteria ?? throw new global::System.ArgumentNullException(nameof(criteria));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreQuestion" /> class.
        /// </summary>
        public ScoreQuestion()
        {
        }

    }
}