# Images and Box

The SDK supports the [Images](https://docs.typesafe.ai/images) and [Box](https://docs.typesafe.ai/primitives/box) API contracts. Both features are experimental upstream. Box requires `jev-preview` or `jev-1.14.0`; images also work with Jev 1.13. Choose a supported model explicitly.

## Upload and find a target

```csharp
using System.Text.Json.Nodes;
using TypeSafeAI;

using var client = new TypeSafeClient(apiKey);
var photo = await ImageAsset.FromFileAsync("kitchen.jpg");
var options = new RequestOptions();
options.Images.Add(photo.Reference, photo);

var state = JsonContent.FromNode(new JsonObject { ["photo"] = photo.Reference });
var questions = new QuestionSet();
var visible = questions.AddNoul("mug_visible", new NoulQuestion("Is there a mug in `photo`?"));
var mug = questions.AddBox("mug", new BoxQuestion("the coffee mug in `photo`"));
var result = await client.SystemOneAsync(state, questions, "jev-preview", options);

if (result.Get(visible).IsTrue(0.8)) // Tune the presence threshold on your images.
{
    var answer = result.Get(mug);
    var leftPixels = answer.Box.Left * imageWidth;
    var topPixels = answer.Box.Top * imageHeight;
    var rightPixels = answer.Box.Right * imageWidth;
    var bottomPixels = answer.Box.Bottom * imageHeight;
    Console.WriteLine($"Confidence: {answer.Confidence}");
}
```

`imageWidth` and `imageHeight` above are the dimensions of the submitted image. Edges are fractions from the top-left: left/right multiply width; top/bottom multiply height. Each Box question returns one box, even when its target is absent. Use a presence Noul and tune confidence handling for your application. Ask separate Box questions to locate distinct targets. Box takes instructions and no criteria. `response.Answers.Boxes` groups all Box answers; custom response projections can use `BoxAnswer` properties with source-generated JSON metadata.

`ImageAsset.FromBytes(encodedBytes, "png")` accepts encoded PNG or JPEG bytes (`jpg` and `jpeg` are aliases). `FromFileAsync` infers format from the extension. Both copy/read the data into memory; retries reuse those bytes and recreate multipart parts, so uploads remain replayable. Caller-owned HTTP clients are not disposed by the SDK.

## References and placement

Use `photo.Reference` as a string anywhere in nested state, question instructions, or Noul/Choice/Score criteria. For example:

```csharp
var badge = await ImageAsset.FromFileAsync("badge.jpg");
options.Images.Add(badge.Reference, badge);
var samePerson = new NoulQuestion($"Is the person in `photo` the person in this badge: {badge.Reference}?");
var target = new BoxQuestion($"the person in `photo` matching this badge: {badge.Reference}");
var choices = new ChoiceQuestion(new Dictionary<string, JsonContent?>
{
    ["badge_holder"] = badge.Reference,
    ["other"] = "Someone else",
});
```

Add these questions to the request that uses the images. For custom references, use your reference as the key in `options.Images` and in the content. References must occur in content values; object keys do not count. They match within longer strings, so use UUIDs to avoid accidental matches. Upload each distinct image once and reuse its reference. Every reference must be used. Each use expands the image again for context, size, and token accounting; put shared images in state and refer to their state paths from questions.

## Encoding and opt-in

Multipart is the default. The first part is `request` with JSON content, followed by one image part per asset. The SDK creates unique part names and supplies the matching asset entries and media types. To use base64 JSON instead:

```csharp
options.UseBase64Images = true;
```

The SDK sets `TypeSafe-Danger-Zone-Images: true` when `Images` is nonempty or a typed `BoxQuestion` is present. For an experimental raw question without typed assets, set `RequestOptions.EnableImages = true`. Explicit request headers still allow direct control. Image configuration is per request and works with typed, custom-projection, synchronous, and batch calls through `RequestOptions`.

The public generated raw client also exposes `Generated.Asset`, `Generated.BoxQuestion`, and `Generated.BoxAnswer` for direct JSON requests. Its header parameter must be supplied explicitly. Prefer the handwritten client for managed multipart uploads.

## Preparation, limits, and errors

Prepare still PNG or JPEG images before upload. Rotate pixels upright (the API does not apply EXIF orientation), resize within the limits, convert other formats, and use baseline JPEG for best compatibility. The SDK does not resize, rotate, decode, or inspect image contents and introduces no image-processing runtime dependency.

The current upstream limits are:

| Limit | Value |
|---|---|
| Encoded image | At most 16 MiB |
| Each dimension | 256–1568 pixels |
| Total pixels per image | 78,400–2,458,624 |
| Request body | 32 MiB, including base64 or multipart overhead |
| Processed distinct images together | 32 MiB |
| Expanded request, including repeated references | 32 MiB |
| Reference | At most 1,024 UTF-8 bytes each; 65,536 bytes total |

The SDK checks encoded file size, declared format, and reference byte limits. The server validates decoded dimensions, actual format, animation, reference use, processed/expanded sizes, and context length. Rejected requests fail as a whole. Invalid images/assets/multipart or missing opt-in produce HTTP 400; size limits produce 413; context overflow produces 400 with `max_tokens_exceeded`. The SDK preserves status and body in `TypeSafeApiException` (400 uses `BadRequestException`). Image tokens are included in `Usage.InputTokens` and billed by the provider like text input tokens.
