
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    ///
    /// </summary>
    public enum SystemoneV1SystemonePostTypeSafeDangerZoneImages
    {
        /// <summary>
        ///
        /// </summary>
        True,
    }

    /// <summary>
    /// Enum extensions to do fast conversions without the reflection.
    /// </summary>
    public static class SystemoneV1SystemonePostTypeSafeDangerZoneImagesExtensions
    {
        /// <summary>
        /// Converts an enum to a string.
        /// </summary>
        public static string ToValueString(this SystemoneV1SystemonePostTypeSafeDangerZoneImages value)
        {
            return value switch
            {
                SystemoneV1SystemonePostTypeSafeDangerZoneImages.True => "true",
                _ => throw new global::System.ArgumentOutOfRangeException(nameof(value), value, null),
            };
        }
        /// <summary>
        /// Converts an string to a enum.
        /// </summary>
        public static SystemoneV1SystemonePostTypeSafeDangerZoneImages? ToEnum(string value)
        {
            return value switch
            {
                "true" => SystemoneV1SystemonePostTypeSafeDangerZoneImages.True,
                _ => null,
            };
        }
    }
}