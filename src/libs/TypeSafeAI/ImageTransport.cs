using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace TypeSafeAI.Generated;

public sealed partial class SystemOneRequest
{
    [JsonIgnore]
    internal (string Part, ImageAsset Image)[]? MultipartImages { get; set; }
}

public partial class RawTypeSafeClient
{
    partial void PrepareSystemoneV1SystemonePostRequest(
        HttpClient httpClient,
        HttpRequestMessage httpRequestMessage,
        SystemoneV1SystemonePostTypeSafeDangerZoneImages? typeSafeDangerZoneImages,
        SystemoneV1SystemonePostTypeSafeDangerZoneLongContext? typeSafeDangerZoneLongContext,
        SystemOneRequest request)
    {
        if (request.MultipartImages is not { Length: > 0 } images) return;
        var multipart = new MultipartFormDataContent();
        // Transfer ownership of the generated JSON content; disposing the request disposes all parts.
        multipart.Add(httpRequestMessage.Content!, "request");
        foreach (var (part, image) in images)
        {
            var content = new ByteArrayContent(image.Bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue(image.Format == "png" ? "image/png" : "image/jpeg");
            multipart.Add(content, part, part + "." + image.Format);
        }
        httpRequestMessage.Content = multipart;
    }
}
