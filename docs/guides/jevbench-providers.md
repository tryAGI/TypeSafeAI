# Hosted decision providers and JevBench

Contract review: **2026-10-16**. This inventory covers the hosted offerings and
author demos on [Benchmark Heaven's API board](https://benchmarkheaven.com/jev-models/api)
and the five services in [Celeris's launch comparison](https://celeris.ai/celeris-1-decision).
It also records newly documented compatible services found during that review.
It is a dated inventory, not an exhaustive claim about future providers.

## Choose a factory

All named factories return `TypeSafeClient` and accept an optional caller-owned
`HttpClient` and configuration callback. Credentials must be issued by the chosen
service. Base URLs below omit `/v1`; the generated transport preserves any prefix.
The Perplexity and Inception profiles change the final operation to `/v1/decisions`.

| Factory | Base URL | Default model | Managed images | Model discovery |
|---|---|---|---|---|
| `Celeris` | `https://inference.celeris.ai/celeris-1-decision/` | `celeris-1-decision` | Named `x_celeris.images` | Supported |
| `Perplexity` | `https://api.perplexity.ai/` | `pplx-decider-v1.1-27b` | Inline `image_url` parts in state | Not documented for this profile |
| `Inception` | `https://api.inceptionlabs.ai/` | `mercury-decide` | Inline `image_url` parts in state | Supported |
| `Liquid` | `https://api.liquid.ai/decisions/` | `d1` | Root `images` data URLs | Not documented for this profile |
| `Wity` | `https://api.wity.alphanimble.com/` | `wity-1` | One root `image` data URL | Not documented |
| `SubQ` | `https://api.subq.ai/` | `subq-snap-preview` | Root `images` objects with `mediaType` and `base64` | Supported |
| `Sage` | `https://sage.levanto.ai/` | `sage-latest` | Separate native API required | Supported |
| `Vansa` | `https://api.vansa.org/` | `vansa-3.4` | Text profile | Supported |
| `Milliseconds` | `https://api.milliseconds.ai/` | `decision-machine-1` | Separate native API required | Not documented |
| `Instinct` | `https://api.zoowork.ai/` | `instinct` | Text profile | Not documented |
| `Classifier` | `https://classifier.dev/` | `jev` | Root `images` data URLs with model `imajev-4b` | Supported |
| `Bespoke` | `https://api.bespokelabs.ai/` | `nimble-latest` | Text profile | Not documented for this profile |
| `Surd` | `https://api.simplexai.net/` | `spx-cd-flash` | Text profile | Supported |
| `Fastino` | `https://api.fastino.ai/` | `fastino/GLiDE` | Text profile | Uses a different catalog API |
| `Amos` | `https://api.amos.scira.ai/` | `amos-1-large` | This adapter implements its text subset | Not documented for this profile |
| `MicrosoftDecision` | Caller-supplied Foundry resource + `/providers/microsoft/` | Caller-supplied deployment name | Text profile | Deployment management is separate |

```csharp
using var client = CustomProviders.Inception("INCEPTION_API_KEY");
var questions = new QuestionSet();
var refund = questions.AddNoul("refund", new NoulQuestion("Is a refund requested?"));
var result = await client.SystemOneAsync("I was charged twice; please refund one payment.", questions);
Console.WriteLine(result.Get(refund).Noul);
```

Choose alternate models per call or through `DefaultModel`, for example
`instinct-dual-4b`, `nimble-v4-max`, `spx-cd-pro`, or `amos-1-medium`. Model names and
availability belong to each provider; discovery returns the provider's catalog,
which can include models unsupported by its decision endpoint.

Instinct's paid ZooData endpoint uses `https://api.zoodata.ai/` and separate keys;
change `BaseUri` explicitly. Classifier accepts its own workspace key or the
literal `"unused"` for documented anonymous access. Amos's default `"unused"`
bearer value is a placeholder for its currently public endpoint. MicrosoftDecision
accepts a caller-managed Entra bearer token, the resource endpoint, and deployment
name; refresh tokens via per-call `RequestOptions.ApiKey`. Azure API-key auth and
automatic Entra refresh are not implemented by this helper.

## Images and response semantics

`RequestOptions.Images` accepts PNG/JPEG `ImageAsset` instances. The adapter always
uses the provider's inline JSON shape. On Perplexity/Inception it preserves the
existing state, appends a text label `Image named <name>:`, then an `image_url`
part for each image. Existing array items keep their order. Root image arrays use
dictionary insertion order; their names do not become provider-side asset handles.
Raw inline content and other documented native options can be passed as structured
state or `ExtraBody`. Typed/raw image-field collisions throw. Raw fields remain
caller-owned and server-validated.

The SDK enforces selected documented question, criterion, image-count, byte, and
serialized-body limits. These are early checks, not full provider validation;
pixel dimensions, rendered token limits, account tiers, and remaining schemas are
checked by the server. SubQ allows four images, 4 MiB each and 8 MiB total;
Inception permits 16 images of up to 10 MiB each; Wity permits one image. The
smaller whole-body limit can bind before decoded image limits. Box, Celeris-only
options, TypeSafe experimental headers, and gzip are rejected by these profiles.
The Amos adapter currently exposes Noul/choice/score, not its multi/span/region
primitives. Images for Sage and milliseconds.ai require their separate native APIs.

GLiDE returns an integer winning `score` and a fractional `expected_level`.
For responses whose model is `glide`, the SDK maps `expected_level` to
`ScoreAnswer.Score`, including projections and replay, preserving the SDK's
expected-score semantics. The original integer and provider metadata remain in
`RawJson`. GLiDE adds bounded HTTP 425 warm-up retries; its default request timeout
is 300 seconds and total retry budget six minutes. Caller configuration can
replace those defaults. Other providers keep the existing retry policy.

`SystemOneResponse.RawJson` and individual answer `RawJson` preserve unknown
metadata, including Wity reasoning and classifier abstention fields. A typed
choice is not permission to ignore an `abstained` flag. Reported confidence and
probabilities are retained without imposing one provider's formula on another.
All tests use synthetic handlers: they establish serialization and SDK behavior,
not model acceptance, uptime, accuracy, latency, calibration, or billed usage.

## Remaining benchmark offerings

A benchmark row may use a custom adapter, a temporary demo, or ordinary chat
inference. It does not establish a public System One service. The following
entries complete the reviewed board inventory beyond the factories above.

| Offering | Integration status and source |
|---|---|
| OpenAI Decisions, GPT-6 Luna decision comparator | Use the generated Decisions surface in [tryAGI.OpenAI](https://github.com/tryAGI/OpenAI); its predicate/array schema differs from System One. |
| Mercury Decide on OpenRouter | Direct Inception profile supported above; OpenRouter routing is a separate protocol/host contract. |
| Liquid d1 on OpenRouter/Vercel | Direct Liquid profile supported above; gateway access is separate. |
| attest-31B-v1.0 | Public page could not be retrieved during review. An older [author submission](https://github.com/fstandhartinger/jevbench/issues/170) describes a different 12B `attest` build; do not assume those IDs or contracts identify the current 31B service. |
| Gemma 4 (Autoloops) | [Provider](https://www.autoloops.ai) is a benchmark lead; a supported public System One contract was not established. |
| SimpleJev Qwen3.6-35B-A3B / Qwen3.8-27B | [Author implementation](https://github.com/featherless-ai/simple-jev), author-hosted demos rather than a stable credentialed factory. |
| system-one-open | [Author repository](https://github.com/mithalouni/system-one-open); use a verified deployment with `CustomProviders.SystemOne`. |
| openjev-sglang | [Author repository](https://github.com/ekzhang/openjev-sglang); verify its deployment/adapter contract before selecting a custom endpoint. |
| JevAct | [Author submission](https://github.com/fstandhartinger/jevbench/issues/66) uses a different native protocol; requires a dedicated adapter. |
| Fastino GLiNER-2.5-Decide | [Native GLiNER schema](https://docs.fastino.ai/concepts/gliner-2-5-decide-call), distinct from GLiDE's System One route. |
| BB-Qwen3.5-4B-LoRA | [Author submission](https://github.com/fstandhartinger/jevbench/issues/156) describes a temporary authenticated tunnel; obtain current endpoint/key before using a custom profile. |
| GPT-6 Luna low/medium, GPT-5.6 Luna chat comparators | Use the [OpenAI SDK](https://github.com/tryAGI/OpenAI) chat/Responses APIs, not the System One profile. |
| Gemini 3.1 Flash-Lite | Use the [Google Gemini SDK](https://github.com/tryAGI/Google_Generative_AI) native contract. |
| DeepSeek V4.1 Flash / Qwen3.8 27B on Chutes | General-purpose hosted comparators; use the provider's documented chat API, not an inferred System One route. |
| Wity auto/always/off variants | Same Wity factory; choose documented `reasoning` through `ExtraBody`. |
| Classifier fast wrapper | Classifier factory above; it routes to Jev and is not an independent model. |
| Sage 1.4 / Nimble-v4 and max, additional or dated listings | Sage/Bespoke factories above; use current catalog or documented model IDs rather than assuming frozen benchmark versions remain served. |

For a provider with a verified public System One contract and no named factory:

```csharp
using var client = CustomProviders.SystemOne("PROVIDER_KEY",
    new Uri("https://your-verified-provider.example/prefix/"), "provider-model");
```

This sends Noul/choice/score over generated Bearer-authenticated transport to
`<base>/v1/systemone`. It assumes neither image support nor model discovery.
It does not translate incompatible APIs or certify an arbitrary endpoint.

## Which JevBench?

Celeris's launch comparison evaluates a 3,210-item/22-configuration Jev-bench
corpus and also reports typed-decisions results. Benchmark Heaven's API board
uses its own versioned JevBench protocol, full and equated subsets, calibration,
cost, speed, and composite/capability views. Decision Index and Decision Bench are
additional evaluations referenced by vendors. Scores from these different
corpora, methods, and dates are not interchangeable. This SDK embeds no ranking,
price, or latency claim; evaluate representative requests with the exact hosted
model and record response model IDs and raw usage.

## Primary contract sources

- [Celeris API reference](https://docs.celeris.ai/api-reference) and [Decisions](https://docs.celeris.ai/decisions).
- [Perplexity Decisions](https://docs.perplexity.ai/docs/decisions/quickstart).
- [Inception Mercury Decide](https://docs.inceptionlabs.ai/capabilities/decisions) and [model listing](https://docs.inceptionlabs.ai/api-reference/models/list-available-models).
- [Liquid d1 launch contract](https://www.liquid.ai/blog/d1-decision-model). Its decision-guide examples use an older base/model; this profile follows the explicit multimodal d1 endpoint in the launch contract.
- [Wity API](https://wity.alphanimble.com/docs/api) and [limits](https://wity.alphanimble.com/docs/limits).
- [SubQ Decisions](https://docs.subq.ai/endpoints/decisions-api/) and [catalog](https://docs.subq.ai/endpoints/models-api/).
- [Levanto SystemOne contract](https://docs.levanto.ai/systemone.md).
- [Vansa full API reference](https://docs.vansa.org/llms-full.txt).
- [milliseconds.ai compatibility contract](https://docs.milliseconds.ai/typesafe/overview).
- [Instinct author contract and both access routes](https://github.com/fstandhartinger/jevbench/issues/69), [Dual 4B](https://github.com/fstandhartinger/jevbench/issues/123).
- [classifier.dev contract](https://classifier.dev) and [OpenAPI](https://classifier.dev/openapi.json).
- [Nimble quickstart](https://docs.bespokelabs.ai/nimble/quickstart).
- [Surd public API docs](https://simplexai.net/en/platform/docs).
- [Fastino GLiDE contract and migration differences](https://docs.fastino.ai/inference/systemone).
- [Amos author model card](https://huggingface.co/mukaddamzaid/amos-1-large).
- [Microsoft Foundry Decision API](https://learn.microsoft.com/en-us/azure/foundry/foundry-models/how-to/use-foundry-models-microsoft-decision).
