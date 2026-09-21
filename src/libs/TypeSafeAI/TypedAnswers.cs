namespace TypeSafeAI;

public readonly record struct ChoiceHandle<TEnum>(string Name) where TEnum : struct, Enum;
public readonly record struct ScoreHandle<TEnum>(string Name) where TEnum : struct, Enum;

/// <summary>Enum-safe choice answer retaining the original wire distribution.</summary>
public sealed record ChoiceAnswer<TEnum>(ChoiceAnswer Answer) where TEnum : struct, Enum
{
    public TEnum Choice => Answer.AsEnum<TEnum>();
    public double Confidence => Answer.Confidence;
    public IReadOnlyDictionary<string, double> Probabilities => Answer.Probabilities;
    public double Probability(TEnum value) => Answer.Probability(EnumLabels<TEnum>.GetLabel(value));
    public IReadOnlyList<TEnum> Ranked => EnumLabels<TEnum>.Members.OrderByDescending(Probability).ToArray();
}

/// <summary>Enum-safe score answer using ordinal level indices, not numeric enum values.</summary>
public sealed record ScoreAnswer<TEnum>(ScoreAnswer Answer) where TEnum : struct, Enum
{
    public double Score => Answer.Score;
    public double Confidence => Answer.Confidence;
    public TEnum MostLikely => Answer.MostLikelyAsEnum<TEnum>();
    public TEnum Nearest => Answer.NearestAsEnum<TEnum>();
    public double Probability(TEnum level) => Answer.Probability(EnumLabels<TEnum>.IndexOf(level));
    public IReadOnlyList<TEnum> Ranked => EnumLabels<TEnum>.Members.OrderByDescending(Probability).ToArray();
}
