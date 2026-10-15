
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// The image format, required with data_base64. The actual format is detected from the bytes.
    /// </summary>
    public enum AssetFormat
    {
        /// <summary>
        ///
        /// </summary>
        Jpeg,
        /// <summary>
        ///
        /// </summary>
        Jpg,
        /// <summary>
        ///
        /// </summary>
        Png,
    }

    /// <summary>
    /// Enum extensions to do fast conversions without the reflection.
    /// </summary>
    public static class AssetFormatExtensions
    {
        /// <summary>
        /// Converts an enum to a string.
        /// </summary>
        public static string ToValueString(this AssetFormat value)
        {
            return value switch
            {
                AssetFormat.Jpeg => "jpeg",
                AssetFormat.Jpg => "jpg",
                AssetFormat.Png => "png",
                _ => throw new global::System.ArgumentOutOfRangeException(nameof(value), value, null),
            };
        }
        /// <summary>
        /// Converts an string to a enum.
        /// </summary>
        public static AssetFormat? ToEnum(string value)
        {
            return value switch
            {
                "jpeg" => AssetFormat.Jpeg,
                "jpg" => AssetFormat.Jpg,
                "png" => AssetFormat.Png,
                _ => null,
            };
        }
    }
}