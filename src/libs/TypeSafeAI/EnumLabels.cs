using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

namespace TypeSafeAI;

#pragma warning disable CA1000 // Each closed enum type intentionally owns one cached label table.

/// <summary>Wire labels and ordered score levels for an enum. Numeric enum values are not score indices.</summary>
public static class EnumLabels<TEnum> where TEnum : struct, Enum
{
    private static readonly IReadOnlyList<Entry> Entries = Build();
    public static IReadOnlyList<TEnum> Members { get; } = Array.AsReadOnly(Entries.Select(static x => x.Value).ToArray());
    public static IReadOnlyList<string> Labels { get; } = Array.AsReadOnly(Entries.Select(static x => x.Label).ToArray());
    public static string GetLabel(TEnum value) => Entries[IndexOf(value)].Label;
    public static string? GetDescription(TEnum value) => Entries[IndexOf(value)].Description;
    public static int IndexOf(TEnum value)
    {
        for (var i = 0; i < Entries.Count; i++)
            if (EqualityComparer<TEnum>.Default.Equals(Entries[i].Value, value)) return i;
        throw new ArgumentOutOfRangeException(nameof(value));
    }
    public static TEnum AtIndex(int index) => Entries[index].Value;
    public static bool TryParse(string label, out TEnum value, bool ignoreCase = true)
    {
        var match = Entries.FirstOrDefault(x => string.Equals(x.Label, label, StringComparison.Ordinal))
            ?? (ignoreCase ? Entries.FirstOrDefault(x => string.Equals(x.Label, label, StringComparison.OrdinalIgnoreCase)) : null);
        value = match is null ? default : match.Value;
        return match is not null;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2090", Justification = "Enum fields and their attributes are preserved when the enum is used.")]
    private static IReadOnlyList<Entry> Build()
    {
        var entries = Enum.GetNames<TEnum>().Select(name =>
        {
            var field = typeof(TEnum).GetField(name)!;
            var label = field.GetCustomAttribute<LabelAttribute>();
            return new Entry(Enum.Parse<TEnum>(name), label?.Label ?? field.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? name,
                label?.Description ?? field.GetCustomAttribute<DescriptionAttribute>()?.Description);
        }).OrderBy(static x => x.Value).ToArray();
        if (entries.Length == 0 || entries.Select(static x => x.Label).Distinct(StringComparer.Ordinal).Count() != entries.Length
            || entries.Select(static x => x.Value).Distinct().Count() != entries.Length)
            throw new InvalidOperationException("Question enums must have members with unique values and wire labels.");
        return entries;
    }

    private sealed record Entry(TEnum Value, string Label, string? Description);
}
