
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// The selected choice, confidence, and probabilities for a choice question.
    /// </summary>
    public sealed partial class ChoiceAnswer
    {
        /// <summary>
        /// Identifies a selection from the requested choices.
        /// </summary>
        /// <default>"choice"</default>
        [global::System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; } = "choice";

        /// <summary>
        /// The name of the choice with the highest probability among the question's criteria.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("choice")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required string Choice { get; set; }

        /// <summary>
        /// Confidence in the selected choice, from 0 to 1. Higher values indicate greater certainty; use lower values to flag uncertain selections for review.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("confidence")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required double Confidence { get; set; }

        /// <summary>
        /// Probability of each choice in criteria, keyed by choice name, from 0 to 1. Shows how likely the alternatives are; values sum to approximately 1.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("probabilities")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required global::System.Collections.Generic.Dictionary<string, double> Probabilities { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceAnswer" /> class.
        /// </summary>
        /// <param name="choice">
        /// The name of the choice with the highest probability among the question's criteria.
        /// </param>
        /// <param name="confidence">
        /// Confidence in the selected choice, from 0 to 1. Higher values indicate greater certainty; use lower values to flag uncertain selections for review.
        /// </param>
        /// <param name="probabilities">
        /// Probability of each choice in criteria, keyed by choice name, from 0 to 1. Shows how likely the alternatives are; values sum to approximately 1.
        /// </param>
        /// <param name="type">
        /// Identifies a selection from the requested choices.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public ChoiceAnswer(
            string choice,
            double confidence,
            global::System.Collections.Generic.Dictionary<string, double> probabilities,
            string type = "choice")
        {
            this.Type = type;
            this.Choice = choice ?? throw new global::System.ArgumentNullException(nameof(choice));
            this.Confidence = confidence;
            this.Probabilities = probabilities ?? throw new global::System.ArgumentNullException(nameof(probabilities));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceAnswer" /> class.
        /// </summary>
        public ChoiceAnswer()
        {
        }

    }
}