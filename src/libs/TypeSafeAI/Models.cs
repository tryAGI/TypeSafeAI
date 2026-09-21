using System.Net;
using System.Text.Json;

namespace TypeSafeAI;

/// <summary>A model name or alias accepted by System One.</summary>
public sealed record ModelMetadata(string Name, string Description, string ReleaseDate)
{
    public DateTimeOffset? ReleasedAt => DateTimeOffset.TryParse(ReleaseDate, out var value) ? value : null;
}

/// <summary>A decoded System One response.</summary>
public sealed record SystemOneResponse(
    string Model,
    AnswerCollection Answers,
    Usage Usage,
    ResponseMetadata Metadata,
    JsonElement RawJson)
{
    public TAnswer Get<TAnswer>(string name) where TAnswer : Answer => Answers.Get<TAnswer>(name);
}

/// <summary>HTTP metadata retained alongside a successful response.</summary>
public sealed record ResponseMetadata(
    HttpStatusCode StatusCode,
    IReadOnlyDictionary<string, IEnumerable<string>> Headers,
    Uri? RequestUri,
    string? RequestId,
    int RetryCount = 0);

/// <summary>Marks a property as not required when projecting named answers into a custom response type.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OptionalAnswerAttribute : Attribute;
