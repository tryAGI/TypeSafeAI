using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TypeSafeAI;

/// <summary>Configuration for <see cref="TypeSafeClient"/>.</summary>
public sealed class TypeSafeClientOptions
{
    public Uri BaseUri { get; set; } = new("https://api.typesafe.ai/");
    public string DefaultModel { get; set; } = "jev-latest";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    public RetryPolicy Retry { get; set; } = new();
    public IDictionary<string, string> DefaultHeaders { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public ILoggerFactory? LoggerFactory { get; set; }
    public bool EnableTelemetry { get; set; } = true;
}

/// <summary>Retry policy backed by AutoSDK's generated transport.</summary>
public sealed class RetryPolicy
{
    public int MaxAttempts { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(500);
    public TimeSpan MaximumDelay { get; set; } = TimeSpan.FromSeconds(30);
    public double BackoffMultiplier { get; set; } = 2;
    public double JitterRatio { get; set; } = 0.2;
    public bool UseRetryAfterHeader { get; set; } = true;
}

/// <summary>Per-call timeout, retry, header, query, auth, and body overrides.</summary>
public sealed class RequestOptions
{
    public TimeSpan? Timeout { get; set; }
    public RetryPolicy? Retry { get; set; }
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public IDictionary<string, string> QueryParameters { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IDictionary<string, JsonElement> ExtraBody { get; } = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
    public string? ApiKey { get; set; }
}
