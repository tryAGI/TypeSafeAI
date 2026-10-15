# Providers and Celeris Decision

`tryAGI.TypeSafeAI` supports TypeSafe's System One API and Celeris Decision's
compatible System One endpoint, plus the hosted decision providers listed in
[the provider inventory](jevbench-providers.md). The handwritten client selects provider-specific
image encoding and capability checks; the HTTP transport remains AutoSDK-generated.

## Celeris quickstart

```csharp
using TypeSafeAI;

using var client = CustomProviders.Celeris("CELERIS_API_KEY");
var questions = new QuestionSet();
var refund = questions.AddNoul("refund", new NoulQuestion("The customer asks for a refund."));
var options = new RequestOptions
{
    Celeris = new CelerisDecisionOptions { Explain = true },
};
var result = await client.SystemOneAsync("I was charged twice. Please refund one payment.",
    questions, requestOptions: options);
Console.WriteLine(result.Get(refund).Noul);
Console.WriteLine(result.Response.Celeris!.Explanations["refund"]);
```

The factory uses base URL `https://inference.celeris.ai/celeris-1-decision/`,
model `celeris-1-decision`, and Bearer authentication. The generated transport
appends `/v1/systemone`, so do not append `/v1` to this base URL. Set a timeout,
retry policy, or logger through the factory's `configure` callback:

```csharp
using var client = CustomProviders.Celeris("CELERIS_API_KEY", configure: options =>
{
    options.Timeout = TimeSpan.FromSeconds(10);
    options.Retry = RetryPolicy.None;
});
```

An optional injected `HttpClient` remains caller-owned. Existing Noul, choice,
score, structured state, `QuestionSet`, batching, custom projection, routing,
guardrails, AI functions, and evaluation APIs use this client through
`ITypeSafeClient`. These composition features do not imply identical calibration
or model quality across providers. Custom projections return answers only; read
explanations from `SystemOneResponse.Celeris` or `QuestionSetResult.Response.Celeris`.

## Images and resolution

```csharp
var image = await ImageAsset.FromFileAsync("receipt.png");
var options = new RequestOptions
{
    Celeris = new CelerisDecisionOptions
    {
        Explain = true,
        ImageResolution = CelerisImageResolution.Tokens560,
    },
};
options.Images.Add("receipt", image);
var questions = new QuestionSet();
var matches = questions.AddNoul("matches", new NoulQuestion(
    "The total on the image named receipt matches the claim amount."));
var result = await client.SystemOneAsync(JsonContent.Parse("""{"claim":{"amount":13.50}}"""),
    questions, requestOptions: options);
Console.WriteLine(result.Get(matches).Noul);
Console.WriteLine(result.Response.Celeris!.Explanations["matches"]);
```

The adapter maps `RequestOptions.Images` to `x_celeris.images`, preserving each
name and encoding PNG/JPEG bytes as inline data URLs. It sends JSON, without
TypeSafe `assets`, multipart parts, or automatic experimental headers.
`UseBase64Images` has no effect on this provider: its encoding is always base64 JSON.
Do not set `EnableImages`; it selects a TypeSafe experimental header rather than
enabling Celeris images.

`CelerisImageResolution` offers `Tokens70`, `Tokens140`, `Tokens280`, `Tokens560`,
and `Tokens1120`. It maps to `x_celeris.max_soft_tokens` for every image. `Default`
or an omitted resolution leaves the field unset (currently 280 on the server).
Higher resolution can help with small text and costs more input tokens. The SDK
does not resize images, decode their pixels, or rotate EXIF orientation.

## Explanations, replay, and raw extensions

`CelerisDecisionOptions.Explain` maps to `x_celeris.explain`. Explanations are
returned as `CelerisDecisionResponse.Explanations`, keyed by question name, and
add processing time. Without the extension, `response.Celeris` is null. Metadata
also survives `TypeSafeClient.FromHttpResponse` replay. Unknown provider fields
remain available in `response.RawJson`.

Advanced callers can continue to supply an `x_celeris` JSON object through
`RequestOptions.ExtraBody`. Unknown extension fields are preserved. Typed and raw
options may be combined when they configure different fields; configuring the
same `explain`, `max_soft_tokens`, or `images` field twice throws instead of
silently replacing it. Request options and raw JSON objects are not mutated.

## Provider capability matrix

| Capability | TypeSafe profile (default) | Celeris profile |
|---|---|---|
| Noul, choice, score; mixed named questions | Supported | Supported |
| Structured JSON state | Supported | Supported |
| Typed Box questions | Supported on compatible Jev models | Rejected before HTTP |
| Image transport | Multipart by default, or TypeSafe base64 assets | Named data URLs under `x_celeris.images` |
| Managed image formats | PNG/JPEG | PNG/JPEG |
| Experimental image/long-context opt-in | Supported | Rejected before HTTP |
| Gzip request compression | Supported | Rejected; this adapter uses the documented JSON contract |
| Typed explanation and image resolution options | Rejected | Supported |
| Model | Configurable Jev model/alias | `celeris-1-decision` |
| Model discovery | Generated TypeSafe model metadata | OpenAI-style `data`/`id` metadata, adapted through generated transport |
| Streaming | Not implemented by System One | Not supported by System One |
| Raw response, replay, retry/cancellation/timeout options | Supported | Supported |

## Model discovery and dependency injection

```csharp
var models = await client.Models.ListAsync();
foreach (var model in models)
    Console.WriteLine($"{model.Name}: {model.OwnedBy}, created {model.CreatedAt}");

services.AddCelerisClient("CELERIS_API_KEY", options =>
{
    options.Timeout = TimeSpan.FromSeconds(10);
    options.Retry = RetryPolicy.None;
});
// Alternatively: services.AddCelerisClientFromEnvironment();
```

`AddCelerisClient` shares the factory's endpoint, default model, and profile, and
registers `TypeSafeClient` and `ITypeSafeClient`. Configure its HTTP handler through
the returned `IHttpClientBuilder`. The environment helper reads `CELERIS_API_KEY`
and rejects missing, empty, or whitespace-only values. As with `AddTypeSafeClient`,
the unkeyed registration represents one selected provider per service collection.

Discovery calls the model-specific base plus `/v1/models`. Celeris documents an
OpenAI-style `data` array with `id`, `created`, and `owned_by`; it is adapted without
replacing generated auth, retries, cancellation, or request options. `Name` maps to
`id`, `CreatedAt` parses Unix seconds, and `OwnedBy` retains the owner. Missing
description and release date are empty strings; `ReleasedAt` stays null rather
than treating creation time as a release. `RawJson` preserves each model's full
payload, including future context and capability fields. A compatible native
`models` array also works. `Models.List()` is the synchronous equivalent.

The profile is captured when the client is constructed. Calls through `RawClient`
bypass the handwritten provider adapter; raw callers own encoding and validation.

## Limits and verification

The SDK checks Celeris's documented limits of 512 questions (64 when explanations
are enabled), 2–512 choice options, 2–64 score levels, 8 images, supported
resolution values, and an 8 MiB serialized JSON body. These checks also apply to
modeled raw question types and the known fields of a raw `x_celeris` object.
TypeSafe retains its existing validation and limits; see [Images and Box](images.md).

`ImageAsset` has a shared 16 MiB encoded-image limit. Celeris's smaller whole-body
limit includes base64 expansion and JSON overhead, and normally binds first.
The server validates actual image contents, the 25-megapixel image limit, the
100,000-JSON-value limit, context capacity, and all other provider constraints.
The SDK's count and body-size checks are not full provider validation.

Local contract tests cover image translation, all resolution values, typed
explanations and replay, request bounds, unsupported capabilities, extension
collisions, retries, HTTP errors, caller-owned transport, and AI-function reuse.
The native smoke executable covers the Celeris image/options/explanation path
without network access, including discovery and dependency injection. These checks do not establish live model acceptance,
accuracy, latency, calibration, pricing, or billing. Provider errors retain their
HTTP status and body through the existing TypeSafe exception hierarchy.

Provider contracts reviewed on 2026-10-15:
[Celeris API reference](https://docs.celeris.ai/api-reference),
[Celeris Decisions](https://docs.celeris.ai/decisions),
[Celeris image resolution](https://docs.celeris.ai/images#image-resolution),
and [TypeSafe images](https://docs.typesafe.ai/images).

For Celeris chat, Responses, or the alternative OpenAI Decisions request format,
use the Celeris factories in `tryAGI.OpenAI`. This package exposes System One.
