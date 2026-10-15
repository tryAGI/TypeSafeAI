
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    ///
    /// </summary>
    public enum SystemoneV1SystemonePostTypeSafeDangerZoneLongContext
    {
        /// <summary>
        ///
        /// </summary>
        True,
    }

    /// <summary>
    /// Enum extensions to do fast conversions without the reflection.
    /// </summary>
    public static class SystemoneV1SystemonePostTypeSafeDangerZoneLongContextExtensions
    {
        /// <summary>
        /// Converts an enum to a string.
        /// </summary>
        public static string ToValueString(this SystemoneV1SystemonePostTypeSafeDangerZoneLongContext value)
        {
            return value switch
            {
                SystemoneV1SystemonePostTypeSafeDangerZoneLongContext.True => "true",
                _ => throw new global::System.ArgumentOutOfRangeException(nameof(value), value, null),
            };
        }
        /// <summary>
        /// Converts an string to a enum.
        /// </summary>
        public static SystemoneV1SystemonePostTypeSafeDangerZoneLongContext? ToEnum(string value)
        {
            return value switch
            {
                "true" => SystemoneV1SystemonePostTypeSafeDangerZoneLongContext.True,
                _ => null,
            };
        }
    }
}