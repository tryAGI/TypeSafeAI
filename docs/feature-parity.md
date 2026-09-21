# Feature parity

This SDK deliberately combines a generated raw transport with a first-class handwritten API. The transport is regenerated from the official OpenAPI document; no file under `Generated/` is edited by hand.

| Capability | Official JS/Python | TypeSafeAI.Net | typesafe-sdk-dotnet | Jev.Net | tryAGI.TypeSafeAI |
|---|---:|---:|---:|---:|---:|
| Noul, choice, and score questions | Yes | Yes | Yes | Yes | Yes |
| Mixed named questions and grouped answers | Yes | Yes | Yes | Yes | Yes |
| Structured JSON state/instructions | Yes | Yes | Yes | Yes | Yes |
| Generic/custom typed response projection | Yes | — | — | Yes | Yes |
| Raw response, request ID, replay | Yes | Yes | Yes | Yes | Yes |
| Per-call headers, query, timeout, retries, auth | Yes | Yes | Yes | Yes | Yes |
| Retry-After, exponential backoff, jitter | Yes | Yes | Yes | Yes | Yes, policy over generated transport |
| Typed HTTP/connection/timeout/cancel errors | Yes | Yes | Yes | Yes | Yes |
| Model discovery | Yes | Yes | Yes | Yes | Yes |
| Environment configuration | Yes | Yes | Yes | Yes | Yes |
| Bounded parallel state evaluation | — | Yes | — | — | Yes |
| Enum labels and typed question handles | — | Yes | — | — | Yes |
| Intent router | — | Yes | — | Yes (example) | Yes |
| DI and `IHttpClientFactory` | — | Yes | Yes | documented | Yes |
| Activity, metrics, and content-safe logs | logging | Yes | Yes | Yes | Yes |
| Protected security headers | Yes | — | — | Yes | Yes |
| Unknown future answers/questions | — | — | — | Yes | Yes |
| Guardrail chat middleware | — | Yes | — | — | Yes |
| Calibrated MEAI chat routing | — | Yes | — | — | Yes |
| Agent-callable `AIFunction` | — | Yes | — | — | Yes |
| `IEvaluator` adapter and interpretations | — | Yes | — | — | Yes |
| NativeAOT/source-generated JSON | — | Yes | — | Yes | Yes |
| Generated OpenAPI transport | official wire models | — | — | — | Yes |

## Examples audited

Executable examples cover mixed questions, Noul classification, choice selection, scoring, structured state, model discovery, custom projections, error handling, intent routing, batching, replay, guardrail policies, AI functions, and evaluation. Transport regression tests exercise future union values, enum wire labels, request IDs, configurable retries, and cancellation. Chat middleware behavior requires its own tests; a documentation example is not evidence of exhaustive parity.

## Packaging and generation

All Microsoft.Extensions.AI integrations now live in `tryAGI.TypeSafeAI`. The old `tryAGI.TypeSafeAI.Extensions.AI` package is a type-forwarding compatibility facade, not an additional requirement. Existing namespaces remain unchanged.

AutoSDK issue #399 is fixed. Regeneration is pinned to `0.35.0-preview.2.21` and produces typed dictionaries of question/answer unions. Handwritten converters outside `Generated/` retain unknown discriminators and original union payloads; these are forward-compatibility behavior, not a replacement HTTP transport.

## Scope of parity

The matrix describes capabilities, not identical public APIs or proof that every upstream test passes. .NET 10 and source-generated serialization are intentional: Python dynamic subclass projection, JavaScript runtime packaging, and older .NET target frameworks are not copied. Custom projections accept caller-provided `JsonTypeInfo<T>`; answer-typed properties are required unless marked `OptionalAnswer`.

Retry policy additionally supports configurable HTTP statuses, connection/timeout switches, custom predicates, a total budget, and an injectable delay clock. The SDK does not claim automatic retries are free: ambiguous transport failures may already have incurred server work. Use `RetryPolicy.None` for latency-sensitive or non-replayable calls.

## Verification

On 2026-09-21, all 22 tests passed with live credentials and the opt-in latency lane. A subsequent standard run passed 21 tests and skipped only the opt-in latency benchmark. AutoSDK's trimming check reported no trimming warnings. The native executable in `src/tests/NativeAotSmoke` also passed on macOS arm64, covering generated transport, enum attributes, typed projections, unknown future answers, and the AI-function adapter without network access.

Run the native smoke with `dotnet publish src/tests/NativeAotSmoke -c Release -o artifacts/native-smoke`, then execute the resulting `NativeAotSmoke` binary. Local Apple machines with host build admission should use their admission wrapper for publication.

## Audit snapshot

The feature review was performed against these source revisions:

- official `typesafe-sdk-js`: `66880ccded6cb642dc1809620c2b108c33730214`
- official `typesafe-sdk-python`: `2ce5c65f13646cab6e6f782328194c9d85f3300a`
- `Hawxy/TypeSafeAI.Net`: `09b1932252eb5397df126e6b42aef24136c54b39`
- `hardkoded/typesafe-sdk-dotnet`: `0f08011d7b45f80ea98bd38e1e72064384d02db9`
- `brightshore/jev-net`: `abd67b726e26bda03bcb5a2a25696dc8a87c0f88`

The current TypeSafe service exposes HTTP endpoints only; no official AsyncAPI contract or realtime channel was present at the time of this audit.
