
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// An expected score with its rubric, confidence, and score-level probabilities.
    /// </summary>
    public sealed partial class ScoreAnswer
    {
        /// <summary>
        /// Identifies a rating against the requested score levels.
        /// </summary>
        /// <default>"score"</default>
        [global::System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; } = "score";

        /// <summary>
        /// Expected score: the probability-weighted average of the rubric levels. May fall between integer levels.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("score")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required double Score { get; set; }

        /// <summary>
        /// Confidence in the score, from 0 to 1. Higher values indicate greater certainty; use lower values to flag uncertain ratings for review.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("confidence")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required double Confidence { get; set; }

        /// <summary>
        /// The requested criteria mapped to their score levels, so you can interpret the score.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("legend")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required object Legend { get; set; }

        /// <summary>
        /// Probability of each score level, from 0 to 1, using the same keys as legend. Shows how likely the alternatives are; values sum to approximately 1.
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
        /// Initializes a new instance of the <see cref="ScoreAnswer" /> class.
        /// </summary>
        /// <param name="score">
        /// Expected score: the probability-weighted average of the rubric levels. May fall between integer levels.
        /// </param>
        /// <param name="confidence">
        /// Confidence in the score, from 0 to 1. Higher values indicate greater certainty; use lower values to flag uncertain ratings for review.
        /// </param>
        /// <param name="legend">
        /// The requested criteria mapped to their score levels, so you can interpret the score.
        /// </param>
        /// <param name="probabilities">
        /// Probability of each score level, from 0 to 1, using the same keys as legend. Shows how likely the alternatives are; values sum to approximately 1.
        /// </param>
        /// <param name="type">
        /// Identifies a rating against the requested score levels.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public ScoreAnswer(
            double score,
            double confidence,
            object legend,
            global::System.Collections.Generic.Dictionary<string, double> probabilities,
            string type = "score")
        {
            this.Type = type;
            this.Score = score;
            this.Confidence = confidence;
            this.Legend = legend ?? throw new global::System.ArgumentNullException(nameof(legend));
            this.Probabilities = probabilities ?? throw new global::System.ArgumentNullException(nameof(probabilities));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreAnswer" /> class.
        /// </summary>
        public ScoreAnswer()
        {
        }

    }
}