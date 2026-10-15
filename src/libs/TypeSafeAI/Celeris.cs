using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using TypeSafeAI.Generated;

namespace TypeSafeAI;

/// <summary>Documented Celeris image resolution budgets, applied to every image in a request.</summary>
public enum CelerisImageResolution
{
    /// <summary>Omit max_soft_tokens and use the provider's default.</summary>
    Default = 0,
    Tokens70 = 70,
    Tokens140 = 140,
    Tokens280 = 280,
    Tokens560 = 560,
    Tokens1120 = 1120,
}

/// <summary>Typed x_celeris request options. Omitted values use the provider's defaults.</summary>
public sealed class CelerisDecisionOptions
{
    /// <summary>Request one explanation per named question. This adds latency and limits requests to 64 questions.</summary>
    public bool? Explain { get; set; }
    /// <summary>Maps to x_celeris.max_soft_tokens. The documented default is 280.</summary>
    public CelerisImageResolution? ImageResolution { get; set; }
}

/// <summary>Celeris metadata retained alongside typed answers and the full raw response.</summary>
public sealed class CelerisDecisionResponse
{
    private CelerisDecisionResponse(Dictionary<string, string> explanations) =>
        Explanations = new ReadOnlyDictionary<string, string>(explanations);

    /// <summary>Explanations keyed by the same names as the request's questions.</summary>
    public IReadOnlyDictionary<string, string> Explanations { get; }

    internal static CelerisDecisionResponse? Decode(JsonElement root)
    {
        if (!root.TryGetProperty("x_celeris", out var extension)) return null;
        if (extension.ValueKind != JsonValueKind.Object)
            throw new TypeSafeResponseValidationException("x_celeris must be an object.", "x_celeris");
        var explanations = new Dictionary<string, string>(StringComparer.Ordinal);
        if (extension.TryGetProperty("explanations", out var values))
        {
            if (values.ValueKind != JsonValueKind.Object)
                throw new TypeSafeResponseValidationException("System One explanations must be keyed by question name.", "x_celeris.explanations");
            foreach (var property in values.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.String)
                    throw new TypeSafeResponseValidationException("An explanation must be a string.", "x_celeris.explanations." + property.Name);
                explanations.Add(property.Name, property.Value.GetString()!);
            }
        }
        return new CelerisDecisionResponse(explanations);
    }
}

internal static class CelerisProvider
{
    internal const int MaximumRequestBytes = 8 * 1024 * 1024;

    internal static void Prepare(SystemOneRequest request, JsonElement questions, RequestOptions? options)
    {
        if (request.Model != CustomProviders.CelerisModel)
            throw new ArgumentException("Celeris Decision requires model 'celeris-1-decision'.", nameof(request));
        if (request.Compression != RequestCompression.None || options?.EnableLongContext == true || options?.EnableImages == true)
            throw new NotSupportedException("Celeris does not use TypeSafe's compression or experimental image/long-context opt-ins. Add images through RequestOptions.Images.");
        if (request.AdditionalProperties.ContainsKey("assets"))
            throw new ArgumentException("Celeris images belong in x_celeris.images, not assets.", nameof(options));

        var extension = request.AdditionalProperties.TryGetValue("x_celeris", out var raw)
            ? JsonNode.Parse(raw.GetRawText()) as JsonObject
                ?? throw new ArgumentException("x_celeris must be an object.", nameof(options))
            : new JsonObject();
        if (options?.Celeris?.Explain is { } explain)
            Add(extension, "explain", JsonValue.Create(explain));
        if (options?.Celeris?.ImageResolution is { } resolution)
        {
            if (!Enum.IsDefined(resolution)) throw new ArgumentOutOfRangeException(nameof(options), "Unsupported Celeris image resolution.");
            if (resolution != CelerisImageResolution.Default)
                Add(extension, "max_soft_tokens", JsonValue.Create((int)resolution));
        }
        if (extension.TryGetPropertyValue("explain", out var explainValue) &&
            (explainValue is not JsonValue boolean || !boolean.TryGetValue<bool>(out _)))
            throw new ArgumentException("x_celeris.explain must be a boolean.", nameof(options));
        if (extension.TryGetPropertyValue("max_soft_tokens", out var resolutionValue) &&
            (resolutionValue is not JsonValue number || !number.TryGetValue<int>(out var tokens) ||
                tokens == 0 || !Enum.IsDefined((CelerisImageResolution)tokens)))
            throw new ArgumentException("x_celeris.max_soft_tokens must be 70, 140, 280, 560, or 1120.", nameof(options));

        ValidateQuestions(questions, extension["explain"]?.GetValue<bool>() == true ? 64 : 512);
        if (options?.Images.Count > 0)
        {
            if (options.Images.Count > 8)
                throw new ArgumentException("Celeris accepts up to eight named images.", nameof(options));
            var images = new JsonObject();
            foreach (var (name, image) in options.Images)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(name);
                ArgumentNullException.ThrowIfNull(image);
                var mediaType = image.Format == "png" ? "image/png" : "image/jpeg";
                images.Add(name, JsonValue.Create("data:" + mediaType + ";base64," + Convert.ToBase64String(image.Bytes)));
            }
            Add(extension, "images", images);
        }
        if (extension.TryGetPropertyValue("images", out var suppliedImages))
        {
            if (suppliedImages is not JsonObject images || images.Count is < 1 or > 8)
                throw new ArgumentException("Celeris accepts one to eight named images.", nameof(options));
            foreach (var (name, value) in images)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(name);
                if (value is not JsonValue image || !image.TryGetValue<string>(out var url) ||
                    !(url.StartsWith("data:image/png;base64,", StringComparison.Ordinal) || url.StartsWith("data:image/jpeg;base64,", StringComparison.Ordinal)))
                    throw new ArgumentException("Celeris images must be inline PNG or JPEG data URLs.", nameof(options));
            }
        }
        if (extension.Count > 0)
            request.AdditionalProperties["x_celeris"] = JsonContent.FromNode(extension).Value;
        request.MaximumRequestBytes = MaximumRequestBytes;
    }

    private static void Add(JsonObject extension, string name, JsonNode value)
    {
        if (extension.ContainsKey(name))
            throw new ArgumentException($"x_celeris.{name} is configured through both typed options and ExtraBody. Configure it once.");
        extension.Add(name, value);
    }

    private static void ValidateQuestions(JsonElement questions, int maximum)
    {
        if (questions.GetPropertyCount() > maximum)
            throw new ArgumentException($"Celeris accepts up to {maximum} questions with the selected explanation setting.", nameof(questions));
        foreach (var property in questions.EnumerateObject())
        {
            var question = property.Value;
            var type = question.GetProperty("type").GetString();
            if (type is not ("noul" or "choice" or "score"))
                throw new NotSupportedException("Celeris supports Noul, choice, and score questions; Box questions are not supported.");
            if (type == "noul") continue;
            var criteria = question.GetProperty("criteria");
            var count = type == "choice" && criteria.ValueKind == JsonValueKind.Object
                ? criteria.GetPropertyCount()
                : type == "score" && criteria.ValueKind == JsonValueKind.Array ? criteria.GetArrayLength() : 0;
            var limit = type == "choice" ? 512 : 64;
            if (count < 2 || count > limit)
                throw new ArgumentException($"Celeris {type} questions require between 2 and {limit} criteria.", nameof(questions));
        }
    }
}
