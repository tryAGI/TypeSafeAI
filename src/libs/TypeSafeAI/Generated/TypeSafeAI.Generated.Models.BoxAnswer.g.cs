
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// A located box with normalized edge coordinates and confidence.
    /// </summary>
    public sealed partial class BoxAnswer
    {
        /// <summary>
        /// Identifies a box answer.
        /// </summary>
        /// <default>"box"</default>
        [global::System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; } = "box";

        /// <summary>
        /// The box's edges as fractions of image height and width, from 0 to 1. Each coordinate is the expected value of its edge's predicted position.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("box")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required global::TypeSafeAI.Generated.BoxCoordinates Box { get; set; }

        /// <summary>
        /// Confidence for the given box, from 0 to 1, with 1 being more confident. Null when the model does not provide confidence.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("confidence")]
        public double? Confidence { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxAnswer" /> class.
        /// </summary>
        /// <param name="box">
        /// The box's edges as fractions of image height and width, from 0 to 1. Each coordinate is the expected value of its edge's predicted position.
        /// </param>
        /// <param name="confidence">
        /// Confidence for the given box, from 0 to 1, with 1 being more confident. Null when the model does not provide confidence.
        /// </param>
        /// <param name="type">
        /// Identifies a box answer.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public BoxAnswer(
            global::TypeSafeAI.Generated.BoxCoordinates box,
            double? confidence,
            string type = "box")
        {
            this.Type = type;
            this.Box = box ?? throw new global::System.ArgumentNullException(nameof(box));
            this.Confidence = confidence;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxAnswer" /> class.
        /// </summary>
        public BoxAnswer()
        {
        }

        /// <summary>
        /// Creates a new <see cref="BoxAnswer"/> from its single non-const required field,
        /// hardcoding any const discriminator fields.
        /// </summary>
        public static BoxAnswer FromBox(global::TypeSafeAI.Generated.BoxCoordinates box)
        {
            return new BoxAnswer
            {
                Box = box,
            };
        }

    }
}