
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// Normalized box edges. Vertical coordinates are measured from the top of the image; horizontal coordinates are measured from the left.
    /// </summary>
    public sealed partial class BoxCoordinates
    {
        /// <summary>
        /// Top edge of the box, as a fraction of image height from the top (0-1).
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("top")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required double Top { get; set; }

        /// <summary>
        /// Bottom edge of the box, as a fraction of image height from the top (0-1).
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("bottom")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required double Bottom { get; set; }

        /// <summary>
        /// Left edge of the box, as a fraction of image width from the left (0-1).
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("left")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required double Left { get; set; }

        /// <summary>
        /// Right edge of the box, as a fraction of image width from the left (0-1).
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("right")]
        [global::System.Text.Json.Serialization.JsonRequired]
        public required double Right { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxCoordinates" /> class.
        /// </summary>
        /// <param name="top">
        /// Top edge of the box, as a fraction of image height from the top (0-1).
        /// </param>
        /// <param name="bottom">
        /// Bottom edge of the box, as a fraction of image height from the top (0-1).
        /// </param>
        /// <param name="left">
        /// Left edge of the box, as a fraction of image width from the left (0-1).
        /// </param>
        /// <param name="right">
        /// Right edge of the box, as a fraction of image width from the left (0-1).
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public BoxCoordinates(
            double top,
            double bottom,
            double left,
            double right)
        {
            this.Top = top;
            this.Bottom = bottom;
            this.Left = left;
            this.Right = right;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxCoordinates" /> class.
        /// </summary>
        public BoxCoordinates()
        {
        }

    }
}