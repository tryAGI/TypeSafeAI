#pragma warning disable CS0618 // Type or member is obsolete

#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// An answer whose type matches the corresponding question.
    /// </summary>
    public readonly partial struct Answer : global::System.IEquatable<Answer>
    {
        /// <summary>
        ///
        /// </summary>
        public global::TypeSafeAI.Generated.AnswerDiscriminatorType? Type { get; }

        /// <summary>
        /// The probability of a yes answer or a true statement.
        /// </summary>
#if NET6_0_OR_GREATER
        public global::TypeSafeAI.Generated.NoulAnswer? Noul { get; init; }
#else
        public global::TypeSafeAI.Generated.NoulAnswer? Noul { get; }
#endif

        /// <summary>
        ///
        /// </summary>
#if NET6_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Noul))]
#endif
        public bool IsNoul => Noul != null;

        /// <summary>
        ///
        /// </summary>
        public bool TryPickNoul(
#if NET6_0_OR_GREATER
            [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            out global::TypeSafeAI.Generated.NoulAnswer? value)
        {
            value = Noul;
            return IsNoul;
        }

        /// <summary>
        ///
        /// </summary>
        public global::TypeSafeAI.Generated.NoulAnswer PickNoul() => IsNoul
            ? Noul!
            : throw new global::System.InvalidOperationException($"Expected union variant 'Noul' but the value was {ToString()}.");

        /// <summary>
        /// An expected score with its rubric, confidence, and score-level probabilities.
        /// </summary>
#if NET6_0_OR_GREATER
        public global::TypeSafeAI.Generated.ScoreAnswer? Score { get; init; }
#else
        public global::TypeSafeAI.Generated.ScoreAnswer? Score { get; }
#endif

        /// <summary>
        ///
        /// </summary>
#if NET6_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Score))]
#endif
        public bool IsScore => Score != null;

        /// <summary>
        ///
        /// </summary>
        public bool TryPickScore(
#if NET6_0_OR_GREATER
            [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            out global::TypeSafeAI.Generated.ScoreAnswer? value)
        {
            value = Score;
            return IsScore;
        }

        /// <summary>
        ///
        /// </summary>
        public global::TypeSafeAI.Generated.ScoreAnswer PickScore() => IsScore
            ? Score!
            : throw new global::System.InvalidOperationException($"Expected union variant 'Score' but the value was {ToString()}.");

        /// <summary>
        /// The selected choice, confidence, and probabilities for a choice question.
        /// </summary>
#if NET6_0_OR_GREATER
        public global::TypeSafeAI.Generated.ChoiceAnswer? Choice { get; init; }
#else
        public global::TypeSafeAI.Generated.ChoiceAnswer? Choice { get; }
#endif

        /// <summary>
        ///
        /// </summary>
#if NET6_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Choice))]
#endif
        public bool IsChoice => Choice != null;

        /// <summary>
        ///
        /// </summary>
        public bool TryPickChoice(
#if NET6_0_OR_GREATER
            [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            out global::TypeSafeAI.Generated.ChoiceAnswer? value)
        {
            value = Choice;
            return IsChoice;
        }

        /// <summary>
        ///
        /// </summary>
        public global::TypeSafeAI.Generated.ChoiceAnswer PickChoice() => IsChoice
            ? Choice!
            : throw new global::System.InvalidOperationException($"Expected union variant 'Choice' but the value was {ToString()}.");
        /// <summary>
        ///
        /// </summary>
        public static implicit operator Answer(global::TypeSafeAI.Generated.NoulAnswer value) => new Answer((global::TypeSafeAI.Generated.NoulAnswer?)value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator global::TypeSafeAI.Generated.NoulAnswer?(Answer @this) => @this.Noul;

        /// <summary>
        ///
        /// </summary>
        public Answer(global::TypeSafeAI.Generated.NoulAnswer? value)
        {
            Noul = value;
        }

        /// <summary>
        ///
        /// </summary>
        public static Answer FromNoul(global::TypeSafeAI.Generated.NoulAnswer? value) => new Answer(value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator Answer(global::TypeSafeAI.Generated.ScoreAnswer value) => new Answer((global::TypeSafeAI.Generated.ScoreAnswer?)value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator global::TypeSafeAI.Generated.ScoreAnswer?(Answer @this) => @this.Score;

        /// <summary>
        ///
        /// </summary>
        public Answer(global::TypeSafeAI.Generated.ScoreAnswer? value)
        {
            Score = value;
        }

        /// <summary>
        ///
        /// </summary>
        public static Answer FromScore(global::TypeSafeAI.Generated.ScoreAnswer? value) => new Answer(value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator Answer(global::TypeSafeAI.Generated.ChoiceAnswer value) => new Answer((global::TypeSafeAI.Generated.ChoiceAnswer?)value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator global::TypeSafeAI.Generated.ChoiceAnswer?(Answer @this) => @this.Choice;

        /// <summary>
        ///
        /// </summary>
        public Answer(global::TypeSafeAI.Generated.ChoiceAnswer? value)
        {
            Choice = value;
        }

        /// <summary>
        ///
        /// </summary>
        public static Answer FromChoice(global::TypeSafeAI.Generated.ChoiceAnswer? value) => new Answer(value);

        /// <summary>
        ///
        /// </summary>
        public Answer(
            global::TypeSafeAI.Generated.AnswerDiscriminatorType? type,
            global::TypeSafeAI.Generated.NoulAnswer? noul,
            global::TypeSafeAI.Generated.ScoreAnswer? score,
            global::TypeSafeAI.Generated.ChoiceAnswer? choice
            )
        {
            Type = type;

            Noul = noul;
            Score = score;
            Choice = choice;
        }

        /// <summary>
        ///
        /// </summary>
        public object? Object =>
            Choice as object ??
            Score as object ??
            Noul as object
            ;

        /// <summary>
        ///
        /// </summary>
        public override string? ToString() =>
            Noul?.ToString() ??
            Score?.ToString() ??
            Choice?.ToString()
            ;

        /// <summary>
        ///
        /// </summary>
        public bool Validate()
        {
            return IsNoul && !IsScore && !IsChoice || !IsNoul && IsScore && !IsChoice || !IsNoul && !IsScore && IsChoice;
        }

        /// <summary>
        ///
        /// </summary>
        public TResult? Match<TResult>(
            global::System.Func<global::TypeSafeAI.Generated.NoulAnswer, TResult>? noul = null,
            global::System.Func<global::TypeSafeAI.Generated.ScoreAnswer, TResult>? score = null,
            global::System.Func<global::TypeSafeAI.Generated.ChoiceAnswer, TResult>? choice = null,
            bool validate = true)
        {
            if (validate)
            {
                Validate();
            }

            if (IsNoul && noul != null)
            {
                return noul(Noul!);
            }
            else if (IsScore && score != null)
            {
                return score(Score!);
            }
            else if (IsChoice && choice != null)
            {
                return choice(Choice!);
            }

            return default(TResult);
        }

        /// <summary>
        ///
        /// </summary>
        public void Match(
            global::System.Action<global::TypeSafeAI.Generated.NoulAnswer>? noul = null,

            global::System.Action<global::TypeSafeAI.Generated.ScoreAnswer>? score = null,

            global::System.Action<global::TypeSafeAI.Generated.ChoiceAnswer>? choice = null,
            bool validate = true)
        {
            if (validate)
            {
                Validate();
            }

            if (IsNoul)
            {
                noul?.Invoke(Noul!);
            }
            else if (IsScore)
            {
                score?.Invoke(Score!);
            }
            else if (IsChoice)
            {
                choice?.Invoke(Choice!);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Switch(
            global::System.Action<global::TypeSafeAI.Generated.NoulAnswer>? noul = null,
            global::System.Action<global::TypeSafeAI.Generated.ScoreAnswer>? score = null,
            global::System.Action<global::TypeSafeAI.Generated.ChoiceAnswer>? choice = null,
            bool validate = true)
        {
            if (validate)
            {
                Validate();
            }

            if (IsNoul)
            {
                noul?.Invoke(Noul!);
            }
            else if (IsScore)
            {
                score?.Invoke(Score!);
            }
            else if (IsChoice)
            {
                choice?.Invoke(Choice!);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override int GetHashCode()
        {
            var fields = new object?[]
            {
                Noul,
                typeof(global::TypeSafeAI.Generated.NoulAnswer),
                Score,
                typeof(global::TypeSafeAI.Generated.ScoreAnswer),
                Choice,
                typeof(global::TypeSafeAI.Generated.ChoiceAnswer),
            };
            const int offset = unchecked((int)2166136261);
            const int prime = 16777619;
            static int HashCodeAggregator(int hashCode, object? value) => value == null
                ? (hashCode ^ 0) * prime
                : (hashCode ^ value.GetHashCode()) * prime;

            return global::System.Linq.Enumerable.Aggregate(fields, offset, HashCodeAggregator);
        }

        /// <summary>
        ///
        /// </summary>
        public bool Equals(Answer other)
        {
            return
                global::System.Collections.Generic.EqualityComparer<global::TypeSafeAI.Generated.NoulAnswer?>.Default.Equals(Noul, other.Noul) &&
                global::System.Collections.Generic.EqualityComparer<global::TypeSafeAI.Generated.ScoreAnswer?>.Default.Equals(Score, other.Score) &&
                global::System.Collections.Generic.EqualityComparer<global::TypeSafeAI.Generated.ChoiceAnswer?>.Default.Equals(Choice, other.Choice)
                ;
        }

        /// <summary>
        ///
        /// </summary>
        public static bool operator ==(Answer obj1, Answer obj2)
        {
            return global::System.Collections.Generic.EqualityComparer<Answer>.Default.Equals(obj1, obj2);
        }

        /// <summary>
        ///
        /// </summary>
        public static bool operator !=(Answer obj1, Answer obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        ///
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Answer o && Equals(o);
        }
    }
}
