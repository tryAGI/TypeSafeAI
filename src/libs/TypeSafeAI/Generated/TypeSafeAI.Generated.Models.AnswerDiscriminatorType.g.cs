
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    ///
    /// </summary>
    public enum AnswerDiscriminatorType
    {
        /// <summary>
        ///
        /// </summary>
        Choice,
        /// <summary>
        ///
        /// </summary>
        Noul,
        /// <summary>
        ///
        /// </summary>
        Score,
    }

    /// <summary>
    /// Enum extensions to do fast conversions without the reflection.
    /// </summary>
    public static class AnswerDiscriminatorTypeExtensions
    {
        /// <summary>
        /// Converts an enum to a string.
        /// </summary>
        public static string ToValueString(this AnswerDiscriminatorType value)
        {
            return value switch
            {
                AnswerDiscriminatorType.Choice => "choice",
                AnswerDiscriminatorType.Noul => "noul",
                AnswerDiscriminatorType.Score => "score",
                _ => throw new global::System.ArgumentOutOfRangeException(nameof(value), value, null),
            };
        }
        /// <summary>
        /// Converts an string to a enum.
        /// </summary>
        public static AnswerDiscriminatorType? ToEnum(string value)
        {
            return value switch
            {
                "choice" => AnswerDiscriminatorType.Choice,
                "noul" => AnswerDiscriminatorType.Noul,
                "score" => AnswerDiscriminatorType.Score,
                _ => null,
            };
        }
    }
}