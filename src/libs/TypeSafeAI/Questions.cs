using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TypeSafeAI;

/// <summary>A System One question.</summary>
public abstract record Question
{
    internal abstract void Write(Utf8JsonWriter writer);

    protected static void WriteContent(Utf8JsonWriter writer, string name, JsonContent? content)
    {
        if (content is not { } value)
        {
            return;
        }

        writer.WritePropertyName(name);
        value.Value.WriteTo(writer);
    }
}

/// <summary>Criteria that explain the false and true interpretations of a noul.</summary>
public sealed record NoulCriteria(JsonContent? False = null, JsonContent? True = null);

/// <summary>A yes/no or true/false question.</summary>
public sealed record NoulQuestion(
    JsonContent? Instructions = null,
    NoulCriteria? Criteria = null) : Question
{
    internal override void Write(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "noul");
        WriteContent(writer, "instructions", Instructions);
        if (Criteria is not null)
        {
            writer.WritePropertyName("criteria");
            writer.WriteStartObject();
            WriteContent(writer, "false", Criteria.False);
            WriteContent(writer, "true", Criteria.True);
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }
}

/// <summary>A question that selects one named choice.</summary>
public sealed record ChoiceQuestion : Question
{
    public ChoiceQuestion(IReadOnlyDictionary<string, JsonContent?> criteria, JsonContent? instructions = null)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        if (criteria.Count == 0)
        {
            throw new ArgumentException("At least one choice is required.", nameof(criteria));
        }

        Criteria = criteria;
        Instructions = instructions;
    }

    public IReadOnlyDictionary<string, JsonContent?> Criteria { get; }
    public JsonContent? Instructions { get; }

    public static ChoiceQuestion FromValues(IEnumerable<string> choices, JsonContent? instructions = null) =>
        new(choices.ToDictionary(static value => value, static _ => (JsonContent?)null, StringComparer.Ordinal), instructions);

    public static ChoiceQuestion FromEnum<TEnum>(JsonContent? instructions = null)
        where TEnum : struct, Enum => new(EnumCriteria.Create<TEnum>(), instructions);

    internal override void Write(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "choice");
        WriteContent(writer, "instructions", Instructions);
        writer.WritePropertyName("criteria");
        writer.WriteStartObject();
        foreach (var pair in Criteria)
        {
            writer.WritePropertyName(pair.Key);
            if (pair.Value is { } value)
            {
                value.Value.WriteTo(writer);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}

/// <summary>A question that assigns an expected score over an ordered rubric.</summary>
public sealed record ScoreQuestion : Question
{
    public ScoreQuestion(IEnumerable<JsonContent> criteria, JsonContent? instructions = null)
    {
        Criteria = criteria?.ToArray() ?? throw new ArgumentNullException(nameof(criteria));
        if (Criteria.Count == 0)
        {
            throw new ArgumentException("At least one score level is required.", nameof(criteria));
        }

        Instructions = instructions;
    }

    public IReadOnlyList<JsonContent> Criteria { get; }
    public JsonContent? Instructions { get; }

    public static ScoreQuestion FromValues(IEnumerable<string> levels, JsonContent? instructions = null) =>
        new(levels.Select(static level => (JsonContent)level), instructions);

    public static ScoreQuestion FromEnum<TEnum>(JsonContent? instructions = null)
        where TEnum : struct, Enum =>
        new(Enum.GetNames<TEnum>().Select(static name => (JsonContent)(EnumCriteria.GetLabel<TEnum>(name) ?? name)), instructions);

    internal override void Write(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "score");
        WriteContent(writer, "instructions", Instructions);
        writer.WritePropertyName("criteria");
        writer.WriteStartArray();
        foreach (var criterion in Criteria)
        {
            criterion.Value.WriteTo(writer);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}

/// <summary>A forward-compatible question whose JSON shape is not yet modeled by this SDK.</summary>
public sealed record RawQuestion(JsonElement Value) : Question
{
    internal override void Write(Utf8JsonWriter writer) => Value.WriteTo(writer);
}

/// <summary>Overrides the wire name and supplies a human-readable enum criterion.</summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class LabelAttribute(string label) : Attribute
{
    public string Label { get; } = label;
}

internal static class EnumCriteria
{
    internal static Dictionary<string, JsonContent?> Create<TEnum>() where TEnum : struct, Enum =>
        Enum.GetNames<TEnum>().ToDictionary(
            static name => name,
            static name => GetLabel<TEnum>(name) is { } label ? (JsonContent?)label : null,
            StringComparer.Ordinal);

    internal static string? GetLabel<TEnum>(string name) where TEnum : struct, Enum =>
        typeof(TEnum).GetField(name)?.GetCustomAttribute<LabelAttribute>()?.Label;
}
