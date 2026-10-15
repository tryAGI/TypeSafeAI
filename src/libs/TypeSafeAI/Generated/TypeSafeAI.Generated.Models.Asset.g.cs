
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// An image that replaces its placeholder: its bytes in data_base64 with their format, or, in a multipart request, the name of the image part that holds them. Requests with images must send the header TypeSafe-Danger-Zone-Images: true.
    /// </summary>
    public sealed partial class Asset
    {
        /// <summary>
        /// Identifies an image asset.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// The image format, required with data_base64. The actual format is detected from the bytes.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("format")]
        [global::System.Text.Json.Serialization.JsonConverter(typeof(global::TypeSafeAI.Generated.JsonConverters.AssetFormatJsonConverter))]
        public global::TypeSafeAI.Generated.AssetFormat? Format { get; set; }

        /// <summary>
        /// Base64-encoded image data.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("data_base64")]
        public string? DataBase64 { get; set; }

        /// <summary>
        /// In a multipart request, the name of the image part this asset uses. With part, data_base64 is ignored.
        /// </summary>
        [global::System.Text.Json.Serialization.JsonPropertyName("part")]
        public string? Part { get; set; }

        /// <summary>
        /// Raw JSON properties that are not explicitly defined in the schema
        /// </summary>
        [global::System.Text.Json.Serialization.JsonExtensionData]
        public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement> AdditionalProperties { get; set; } = new global::System.Collections.Generic.Dictionary<string, global::System.Text.Json.JsonElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Asset" /> class.
        /// </summary>
        /// <param name="type">
        /// Identifies an image asset.
        /// </param>
        /// <param name="format">
        /// The image format, required with data_base64. The actual format is detected from the bytes.
        /// </param>
        /// <param name="dataBase64">
        /// Base64-encoded image data.
        /// </param>
        /// <param name="part">
        /// In a multipart request, the name of the image part this asset uses. With part, data_base64 is ignored.
        /// </param>
#if NET7_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
#endif
        public Asset(
            string? type,
            global::TypeSafeAI.Generated.AssetFormat? format,
            string? dataBase64,
            string? part)
        {
            this.Type = type;
            this.Format = format;
            this.DataBase64 = dataBase64;
            this.Part = part;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asset" /> class.
        /// </summary>
        public Asset()
        {
        }

    }
}