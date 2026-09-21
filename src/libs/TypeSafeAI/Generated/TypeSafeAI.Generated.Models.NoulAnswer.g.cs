
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// The probability of a yes answer or a true statement.
    /// </summary>
    public sealed partial class NoulAnswer
    {
        /// <summary>
        /// Identifies a yes/no answer.
        /// </summary>
        /// <default>"noul"</default>
        [global::System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; } = "noul";

        /// <summary>
        /// Probability of a yes answer or a true statement, from 0 to 1. Values near 1 favor yes or true, values near 0 favor no or false, and values near 0.5 indicate uncertainty.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("noul")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required double Noul { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NoulAnswer" /> class.
        /// </summary>
        /// <param name="noul">
        /// Probability of a yes answer or a true statement, from 0 to 1. Values near 1 favor yes or true, values near 0 favor no or false, and values near 0.5 indicate uncertainty.
        /// </param>
        /// <param name="type">
        /// Identifies a yes/no answer.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public NoulAnswer(
            double noul,
            string type = "noul")
        {
            this.Type = type;
            this.Noul = noul;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoulAnswer" /> class.
        /// </summary>
        public NoulAnswer()
        {
        }

        /// <summary>
        /// Creates a new <see cref="NoulAnswer"/> from its single non-const required field,
        /// hardcoding any const discriminator fields.
        /// </summary>
        public static NoulAnswer FromNoul(double noul)
        {
            return new NoulAnswer
            {
                Noul = noul,
            };
        }

    }
}