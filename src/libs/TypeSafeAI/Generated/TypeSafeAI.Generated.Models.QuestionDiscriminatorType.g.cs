
#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    ///
    /// </summary>
    public enum QuestionDiscriminatorType
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
    public static class QuestionDiscriminatorTypeExtensions
    {
        /// <summary>
        /// Converts an enum to a string.
        /// </summary>
        public static string ToValueString(this QuestionDiscriminatorType value)
        {
            return value switch
            {
                QuestionDiscriminatorType.Choice => "choice",
                QuestionDiscriminatorType.Noul => "noul",
                QuestionDiscriminatorType.Score => "score",
                _ => throw new global::System.ArgumentOutOfRangeException(nameof(value), value, null),
            };
        }
        /// <summary>
        /// Converts an string to a enum.
        /// </summary>
        public static QuestionDiscriminatorType? ToEnum(string value)
        {
            return value switch
            {
                "choice" => QuestionDiscriminatorType.Choice,
                "noul" => QuestionDiscriminatorType.Noul,
                "score" => QuestionDiscriminatorType.Score,
                _ => null,
            };
        }
    }
}