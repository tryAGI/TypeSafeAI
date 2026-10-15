using System.Text.Json;
using System.Text.Json.Nodes;
using TypeSafeAI.Generated;

namespace TypeSafeAI;

internal static class HostedProvider
{
    internal static void Prepare(SystemOneProvider provider, SystemOneRequest request, JsonElement state,
        JsonElement questions, RequestOptions? options)
    {
        if (options?.Celeris is not null)
            throw new NotSupportedException("Celeris options require the Celeris profile.");
        if (request.Compression != RequestCompression.None || options?.EnableImages == true || options?.EnableLongContext == true)
            throw new NotSupportedException("This provider does not use TypeSafe's compression or experimental opt-in headers.");
        if (request.AdditionalProperties.ContainsKey("assets") || request.AdditionalProperties.ContainsKey("x_celeris"))
            throw new ArgumentException("TypeSafe assets and Celeris extensions require their respective provider profiles.", nameof(options));
        if (state.ValueKind is not (JsonValueKind.String or JsonValueKind.Object or JsonValueKind.Array))
            throw new ArgumentException("State must be text, an object, or an array.", nameof(state));

        request.UseDecisionsPath = provider is SystemOneProvider.Perplexity or SystemOneProvider.Inception;
        var (maxQuestions, minCriteria, maxChoices, maxLevels) = provider switch
        {
            SystemOneProvider.Perplexity => (128, 1, 255, 10),
            SystemOneProvider.Inception => (int.MaxValue, 1, 255, 10),
            SystemOneProvider.Wity => (int.MaxValue, 2, 256, 10),
            SystemOneProvider.SubQ => (16, 2, 255, 10),
            SystemOneProvider.Sage => (int.MaxValue, 2, 120, 26),
            SystemOneProvider.Vansa => (32, 2, 255, 32),
            SystemOneProvider.Fastino => (int.MaxValue, 2, 255, 255),
            SystemOneProvider.Amos => (int.MaxValue, 1, 255, int.MaxValue),
            SystemOneProvider.Milliseconds => (32, 2, 64, 10),
            SystemOneProvider.Instinct when request.Model == "instinct" => (int.MaxValue, 2, 26, int.MaxValue),
            _ => (int.MaxValue, 1, int.MaxValue, int.MaxValue),
        };
        if (questions.GetPropertyCount() > maxQuestions)
            throw new ArgumentException($"{provider} accepts up to {maxQuestions} questions.", nameof(questions));
        foreach (var property in questions.EnumerateObject())
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(property.Name);
            if (provider == SystemOneProvider.SubQ && property.Name.Length > 100 || provider == SystemOneProvider.Wity && property.Name.Length > 128)
                throw new ArgumentException("The question name exceeds this provider's length limit.", nameof(questions));
            var question = property.Value;
            var type = question.GetProperty("type").GetString();
            if (type is not ("noul" or "choice" or "score"))
                throw new NotSupportedException($"{provider} supports Noul, choice, and score questions through this profile.");
            if (provider == SystemOneProvider.Fastino &&
                (!question.TryGetProperty("instructions", out var fastinoInstructions) || fastinoInstructions.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(fastinoInstructions.GetString())))
                throw new ArgumentException("GLiDE requires nonempty string instructions.", nameof(questions));
            if (provider == SystemOneProvider.Wity)
            {
                if (!question.TryGetProperty("instructions", out var instructions) || instructions.ValueKind != JsonValueKind.String || instructions.GetString()!.Length > 4000)
                    throw new ArgumentException("Wity requires string instructions of up to 4,000 characters.", nameof(questions));
            }
            if (type == "noul") continue;
            var criteria = question.GetProperty("criteria");
            var count = type == "choice" && criteria.ValueKind == JsonValueKind.Object ? criteria.GetPropertyCount() :
                type == "score" && criteria.ValueKind == JsonValueKind.Array ? criteria.GetArrayLength() : 0;
            var maximum = type == "choice" ? maxChoices : maxLevels;
            var minimum = provider == SystemOneProvider.Fastino && type == "score" ? 1 : minCriteria;
            if (count < minimum || count > maximum)
                throw new ArgumentException($"{provider} {type} requires {minimum} to {maximum} criteria.", nameof(questions));
        }
        if (provider == SystemOneProvider.Wity && (state.ValueKind == JsonValueKind.String ? state.GetString()!.Length : state.GetRawText().Length) > 32000)
            throw new ArgumentException("Wity state exceeds 32,000 characters.", nameof(state));
        request.MaximumRequestBytes = provider switch
        {
            SystemOneProvider.Perplexity => 32 * 1024 * 1024,
            SystemOneProvider.Inception => 20 * 1024 * 1024,
            SystemOneProvider.Wity => 12_000_000,
            SystemOneProvider.SubQ => 13 * 1024 * 1024,
            SystemOneProvider.Vansa => 512_000,
            SystemOneProvider.Surd => 15_000_000,
            SystemOneProvider.Classifier => 1_000_000,
            _ => null,
        };
        if (options?.Images.Count is not > 0) return;
        var maxImages = provider switch
        {
            SystemOneProvider.Perplexity or SystemOneProvider.Liquid => int.MaxValue,
            SystemOneProvider.Inception => 16,
            SystemOneProvider.Wity => 1,
            SystemOneProvider.SubQ => 4,
            SystemOneProvider.Classifier when request.Model == "imajev-4b" => 2,
            _ => 0,
        };
        if (options.Images.Count > maxImages)
            throw new NotSupportedException($"The {provider} profile for '{request.Model}' accepts up to {maxImages} images through RequestOptions.Images.");
        var field = provider == SystemOneProvider.Wity ? "image" : "images";
        if (request.AdditionalProperties.ContainsKey(field))
            throw new ArgumentException($"Configure {field} through typed options or ExtraBody, once.", nameof(options));
        var images = new JsonArray();
        var inlineState = new JsonArray();
        if (provider is SystemOneProvider.Perplexity or SystemOneProvider.Inception)
        {
            if (state.ValueKind == JsonValueKind.Array)
                foreach (var item in state.EnumerateArray()) inlineState.Add(JsonNode.Parse(item.GetRawText()));
            else inlineState.Add(JsonNode.Parse(state.GetRawText()));
        }
        long totalBytes = 0;
        foreach (var (name, image) in options.Images)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(image);
            totalBytes += image.Bytes.Length;
            if (provider == SystemOneProvider.Inception && image.Bytes.Length > 10 * 1024 * 1024 ||
                provider == SystemOneProvider.SubQ && (image.Bytes.Length > 4 * 1024 * 1024 || totalBytes > 8 * 1024 * 1024))
                throw new ArgumentException("Images exceed this provider's decoded byte limit.", nameof(options));
            var mediaType = image.Format == "png" ? "image/png" : "image/jpeg";
            var base64 = Convert.ToBase64String(image.Bytes);
            var url = "data:" + mediaType + ";base64," + base64;
            if (provider is SystemOneProvider.Perplexity or SystemOneProvider.Inception)
            {
                inlineState.Add((JsonNode)new JsonObject { ["type"] = "text", ["text"] = "Image named " + name + ":" });
                inlineState.Add((JsonNode)new JsonObject { ["type"] = "image_url", ["image_url"] = new JsonObject { ["url"] = url } });
            }
            else if (provider == SystemOneProvider.SubQ)
                images.Add((JsonNode)new JsonObject { ["mediaType"] = mediaType, ["base64"] = base64 });
            else images.Add((JsonNode)JsonValue.Create(url)!);
        }
        if (provider is SystemOneProvider.Perplexity or SystemOneProvider.Inception)
            request.State = AnyOf<string, object, IList<object>>.FromValue2(JsonContent.FromNode(inlineState).Value);
        else request.AdditionalProperties[field] = JsonContent.FromNode(provider == SystemOneProvider.Wity ? images[0]!.DeepClone() : images).Value;
    }
}
