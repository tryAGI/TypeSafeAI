#pragma warning disable CS0618 // Type or member is obsolete

#nullable enable

namespace TypeSafeAI.Generated
{
    /// <summary>
    /// A question about the supplied content.
    /// </summary>
    public readonly partial struct Question : global::System.IEquatable<Question>
    {
        /// <summary>
        ///
        /// </summary>
        public global::TypeSafeAI.Generated.QuestionDiscriminatorType? Type { get; }

        /// <summary>
        /// A yes/no question or statement, answered with the probability of yes or true.
        /// </summary>
#if NET6_0_OR_GREATER
        public global::TypeSafeAI.Generated.NoulQuestion? Noul { get; init; }
#else
        public global::TypeSafeAI.Generated.NoulQuestion? Noul { get; }
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
            out global::TypeSafeAI.Generated.NoulQuestion? value)
        {
            value = Noul;
            return IsNoul;
        }

        /// <summary>
        ///
        /// </summary>
        public global::TypeSafeAI.Generated.NoulQuestion PickNoul() => Noul is { } value
            ? value
            : throw new global::System.InvalidOperationException($"Expected union variant 'Noul' but the value was {ToString()}.");

        /// <summary>
        /// A question that selects one option from the choices you define.
        /// </summary>
#if NET6_0_OR_GREATER
        public global::TypeSafeAI.Generated.ChoiceQuestion? Choice { get; init; }
#else
        public global::TypeSafeAI.Generated.ChoiceQuestion? Choice { get; }
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
            out global::TypeSafeAI.Generated.ChoiceQuestion? value)
        {
            value = Choice;
            return IsChoice;
        }

        /// <summary>
        ///
        /// </summary>
        public global::TypeSafeAI.Generated.ChoiceQuestion PickChoice() => Choice is { } value
            ? value
            : throw new global::System.InvalidOperationException($"Expected union variant 'Choice' but the value was {ToString()}.");

        /// <summary>
        /// A question that assigns a score using an ordered rubric.
        /// </summary>
#if NET6_0_OR_GREATER
        public global::TypeSafeAI.Generated.ScoreQuestion? Score { get; init; }
#else
        public global::TypeSafeAI.Generated.ScoreQuestion? Score { get; }
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
            out global::TypeSafeAI.Generated.ScoreQuestion? value)
        {
            value = Score;
            return IsScore;
        }

        /// <summary>
        ///
        /// </summary>
        public global::TypeSafeAI.Generated.ScoreQuestion PickScore() => Score is { } value
            ? value
            : throw new global::System.InvalidOperationException($"Expected union variant 'Score' but the value was {ToString()}.");

        /// <summary>
        /// A question that locates what its instructions describe in the request's image. Requests with box questions must send the header TypeSafe-Danger-Zone-Images: true.
        /// </summary>
#if NET6_0_OR_GREATER
        public global::TypeSafeAI.Generated.BoxQuestion? Box { get; init; }
#else
        public global::TypeSafeAI.Generated.BoxQuestion? Box { get; }
#endif

        /// <summary>
        ///
        /// </summary>
#if NET6_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Box))]
#endif
        public bool IsBox => Box != null;

        /// <summary>
        ///
        /// </summary>
        public bool TryPickBox(
#if NET6_0_OR_GREATER
            [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            out global::TypeSafeAI.Generated.BoxQuestion? value)
        {
            value = Box;
            return IsBox;
        }

        /// <summary>
        ///
        /// </summary>
        public global::TypeSafeAI.Generated.BoxQuestion PickBox() => Box is { } value
            ? value
            : throw new global::System.InvalidOperationException($"Expected union variant 'Box' but the value was {ToString()}.");
        /// <summary>
        ///
        /// </summary>
        public static implicit operator Question(global::TypeSafeAI.Generated.NoulQuestion value) => new Question((global::TypeSafeAI.Generated.NoulQuestion?)value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator global::TypeSafeAI.Generated.NoulQuestion?(Question @this) => @this.Noul;

        /// <summary>
        ///
        /// </summary>
        public Question(global::TypeSafeAI.Generated.NoulQuestion? value)
        {
            Noul = value;
        }

        /// <summary>
        ///
        /// </summary>
        public static Question FromNoul(global::TypeSafeAI.Generated.NoulQuestion? value) => new Question(value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator Question(global::TypeSafeAI.Generated.ChoiceQuestion value) => new Question((global::TypeSafeAI.Generated.ChoiceQuestion?)value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator global::TypeSafeAI.Generated.ChoiceQuestion?(Question @this) => @this.Choice;

        /// <summary>
        ///
        /// </summary>
        public Question(global::TypeSafeAI.Generated.ChoiceQuestion? value)
        {
            Choice = value;
        }

        /// <summary>
        ///
        /// </summary>
        public static Question FromChoice(global::TypeSafeAI.Generated.ChoiceQuestion? value) => new Question(value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator Question(global::TypeSafeAI.Generated.ScoreQuestion value) => new Question((global::TypeSafeAI.Generated.ScoreQuestion?)value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator global::TypeSafeAI.Generated.ScoreQuestion?(Question @this) => @this.Score;

        /// <summary>
        ///
        /// </summary>
        public Question(global::TypeSafeAI.Generated.ScoreQuestion? value)
        {
            Score = value;
        }

        /// <summary>
        ///
        /// </summary>
        public static Question FromScore(global::TypeSafeAI.Generated.ScoreQuestion? value) => new Question(value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator Question(global::TypeSafeAI.Generated.BoxQuestion value) => new Question((global::TypeSafeAI.Generated.BoxQuestion?)value);

        /// <summary>
        ///
        /// </summary>
        public static implicit operator global::TypeSafeAI.Generated.BoxQuestion?(Question @this) => @this.Box;

        /// <summary>
        ///
        /// </summary>
        public Question(global::TypeSafeAI.Generated.BoxQuestion? value)
        {
            Box = value;
        }

        /// <summary>
        ///
        /// </summary>
        public static Question FromBox(global::TypeSafeAI.Generated.BoxQuestion? value) => new Question(value);

        /// <summary>
        ///
        /// </summary>
        public Question(
            global::TypeSafeAI.Generated.QuestionDiscriminatorType? type,
            global::TypeSafeAI.Generated.NoulQuestion? noul,
            global::TypeSafeAI.Generated.ChoiceQuestion? choice,
            global::TypeSafeAI.Generated.ScoreQuestion? score,
            global::TypeSafeAI.Generated.BoxQuestion? box
            )
        {
            Type = type;

            Noul = noul;
            Choice = choice;
            Score = score;
            Box = box;
        }

        /// <summary>
        ///
        /// </summary>
        public object? Object =>
            Box as object ??
            Score as object ??
            Choice as object ??
            Noul as object
            ;

        /// <summary>
        ///
        /// </summary>
        public override string? ToString() =>
            Noul?.ToString() ??
            Choice?.ToString() ??
            Score?.ToString() ??
            Box?.ToString()
            ;

        /// <summary>
        ///
        /// </summary>
        public bool Validate()
        {
            return IsNoul && !IsChoice && !IsScore && !IsBox || !IsNoul && IsChoice && !IsScore && !IsBox || !IsNoul && !IsChoice && IsScore && !IsBox || !IsNoul && !IsChoice && !IsScore && IsBox;
        }

        /// <summary>
        ///
        /// </summary>
        public TResult? Match<TResult>(
            global::System.Func<global::TypeSafeAI.Generated.NoulQuestion, TResult>? noul = null,
            global::System.Func<global::TypeSafeAI.Generated.ChoiceQuestion, TResult>? choice = null,
            global::System.Func<global::TypeSafeAI.Generated.ScoreQuestion, TResult>? score = null,
            global::System.Func<global::TypeSafeAI.Generated.BoxQuestion, TResult>? box = null,
            bool validate = true)
        {
            if (validate)
            {
                Validate();
            }

            if (Noul is { } __value0 && noul != null)
            {
                return noul(__value0);
            }
            else if (Choice is { } __value1 && choice != null)
            {
                return choice(__value1);
            }
            else if (Score is { } __value2 && score != null)
            {
                return score(__value2);
            }
            else if (Box is { } __value3 && box != null)
            {
                return box(__value3);
            }

            return default(TResult);
        }

        /// <summary>
        ///
        /// </summary>
        public void Match(
            global::System.Action<global::TypeSafeAI.Generated.NoulQuestion>? noul = null,

            global::System.Action<global::TypeSafeAI.Generated.ChoiceQuestion>? choice = null,

            global::System.Action<global::TypeSafeAI.Generated.ScoreQuestion>? score = null,

            global::System.Action<global::TypeSafeAI.Generated.BoxQuestion>? box = null,
            bool validate = true)
        {
            if (validate)
            {
                Validate();
            }

            if (Noul is { } __value0)
            {
                noul?.Invoke(__value0);
            }
            else if (Choice is { } __value1)
            {
                choice?.Invoke(__value1);
            }
            else if (Score is { } __value2)
            {
                score?.Invoke(__value2);
            }
            else if (Box is { } __value3)
            {
                box?.Invoke(__value3);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Switch(
            global::System.Action<global::TypeSafeAI.Generated.NoulQuestion>? noul = null,
            global::System.Action<global::TypeSafeAI.Generated.ChoiceQuestion>? choice = null,
            global::System.Action<global::TypeSafeAI.Generated.ScoreQuestion>? score = null,
            global::System.Action<global::TypeSafeAI.Generated.BoxQuestion>? box = null,
            bool validate = true)
        {
            if (validate)
            {
                Validate();
            }

            if (Noul is { } __value0)
            {
                noul?.Invoke(__value0);
            }
            else if (Choice is { } __value1)
            {
                choice?.Invoke(__value1);
            }
            else if (Score is { } __value2)
            {
                score?.Invoke(__value2);
            }
            else if (Box is { } __value3)
            {
                box?.Invoke(__value3);
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
                typeof(global::TypeSafeAI.Generated.NoulQuestion),
                Choice,
                typeof(global::TypeSafeAI.Generated.ChoiceQuestion),
                Score,
                typeof(global::TypeSafeAI.Generated.ScoreQuestion),
                Box,
                typeof(global::TypeSafeAI.Generated.BoxQuestion),
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
        public bool Equals(Question other)
        {
            return
                global::System.Collections.Generic.EqualityComparer<global::TypeSafeAI.Generated.NoulQuestion?>.Default.Equals(Noul, other.Noul) &&
                global::System.Collections.Generic.EqualityComparer<global::TypeSafeAI.Generated.ChoiceQuestion?>.Default.Equals(Choice, other.Choice) &&
                global::System.Collections.Generic.EqualityComparer<global::TypeSafeAI.Generated.ScoreQuestion?>.Default.Equals(Score, other.Score) &&
                global::System.Collections.Generic.EqualityComparer<global::TypeSafeAI.Generated.BoxQuestion?>.Default.Equals(Box, other.Box)
                ;
        }

        /// <summary>
        ///
        /// </summary>
        public static bool operator ==(Question obj1, Question obj2)
        {
            return global::System.Collections.Generic.EqualityComparer<Question>.Default.Equals(obj1, obj2);
        }

        /// <summary>
        ///
        /// </summary>
        public static bool operator !=(Question obj1, Question obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        ///
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Question o && Equals(o);
        }
    }
}
