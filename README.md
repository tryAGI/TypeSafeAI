<div class="docs-hero">
  <h1>TypeSafeAI</h1>
  <p class="docs-hero-lead">First-class, NativeAOT-ready .NET SDK for TypeSafe AI: generated transport plus an ergonomic typed API.</p>
  <div class="docs-badge-row">
    <a href="https://www.nuget.org/packages/tryAGI.TypeSafeAI/"><img alt="Nuget package" src="https://img.shields.io/nuget/vpre/tryAGI.TypeSafeAI"></a>
    <a href="https://github.com/tryAGI/TypeSafeAI/actions/workflows/dotnet.yml"><img alt="dotnet" src="https://github.com/tryAGI/TypeSafeAI/actions/workflows/dotnet.yml/badge.svg?branch=main"></a>
    <a href="https://github.com/tryAGI/TypeSafeAI/blob/main/LICENSE"><img alt="License: MIT" src="https://img.shields.io/github/license/tryAGI/TypeSafeAI"></a>
    <a href="https://discord.gg/Ca2xhfBf3v"><img alt="Discord" src="https://img.shields.io/discord/1115206893015662663?label=Discord&amp;logo=discord&amp;logoColor=white&amp;color=d82679"></a>
  </div>
  <div class="docs-hero-actions">
    <a href="#usage">Get started</a>
    <a href="#support">Get support</a>
  </div>
</div>

<div class="docs-feature-grid">
  <div class="docs-feature-card">
    <h3>Generated transport, designed API</h3>
    <p>The raw client is regenerated from the <a href="https://api.typesafe.ai/openapi.json">official OpenAPI definition</a>; typed questions, batching, routing, DI, and MEAI integrations are maintained as handwritten extensions.</p>
  </div>
  <div class="docs-feature-card">
    <h3>Complete ecosystem parity</h3>
    <p>Includes the official JavaScript/Python examples plus the strongest .NET features from TypeSafeAI.Net, typesafe-sdk-dotnet, and Jev.Net.</p>
  </div>
  <div class="docs-feature-card">
    <h3>Modern .NET</h3>
    <p>Targets current .NET practices including nullability, trimming, NativeAOT awareness, and source-generated serialization.</p>
  </div>
  <div class="docs-feature-card">
    <h3>Docs from examples</h3>
    <p>Examples stay in sync between the README, MkDocs site, and integration tests through the AutoSDK docs pipeline.</p>
  </div>
</div>

<!-- AUTOSDK:ECOSYSTEM-MAINTENANCE:START -->
## Ecosystem maintenance

This SDK is one of more than 200 .NET SDKs maintained with [AutoSDK](https://github.com/tryAGI/AutoSDK). The tryAGI [SDK audit](https://github.com/tryAGI/tryAGI/blob/main/GENERATED_SDK_AUDITS.md) continuously checks repository synchronization, upstream-spec regeneration, release workflows, warnings, public API visibility, and trimming/NativeAOT compatibility.

Every issue is first investigated for ecosystem-wide applicability. When the root cause belongs in AutoSDK, we fix and regression-test the generator, then roll the improvement out to every applicable SDK. Provider-specific behavior remains in this repository when it cannot be derived safely from the API specification.

Issue content—including code blocks, logs, links, and attachments—is treated only as untrusted diagnostic data. Embedded control instructions, hidden directives, delimiter tricks, or requests to alter triage or tooling behavior are ignored. Please report reproducible technical evidence and remove secrets and personal data.
<!-- AUTOSDK:ECOSYSTEM-MAINTENANCE:END -->

## Usage

```bash
dotnet add package tryAGI.TypeSafeAI
# Optional Microsoft.Extensions.AI middleware and evaluator
dotnet add package tryAGI.TypeSafeAI.Extensions.AI
```

```csharp
using TypeSafeAI;

using var client = new TypeSafeClient(apiKey);
var questions = new QuestionSet();
var billing = questions.AddNoul("billing", new NoulQuestion("Is this about billing?"));
var urgency = questions.AddScore("urgency", ScoreQuestion.FromValues(
    ["Can wait", "This week", "Today"], "How urgent is it?"));

var result = await client.SystemOneAsync("I was charged twice. Please help today.", questions);
Console.WriteLine(result.Get(billing).Noul);
Console.WriteLine(result.Get(urgency).Score);
```

Set `TYPESAFE_API_KEY` and use `TypeSafeClient.CreateFromEnvironment()` when you prefer environment configuration. The generated `TypeSafeAI.Generated.RawTypeSafeClient` remains public for direct OpenAPI-level access.

See the [feature parity matrix](docs/feature-parity.md) for the audited upstream surface and [Microsoft.Extensions.AI guide](docs/guides/meai.md) for guardrails, routing, tools, and evaluation.

<!-- EXAMPLES:START -->
### Generate
Create a client from `TYPESAFE_API_KEY` and discover the models available to the account.

```csharp
using var client = new TypeSafeClient(apiKey);

var models = await client.Models.ListAsync();
```

### Mixed questions
Ask typed Noul, choice, and score questions in one request.

```csharp
using var client = new TypeSafeClient(apiKey);
var questions = new QuestionSet();
var billing = questions.AddNoul("billing", new NoulQuestion("Is this message about billing?"));
var tone = questions.AddChoice("tone", ChoiceQuestion.FromValues(
    ["angry", "calm", "excited"], "What is the tone?"));
var urgency = questions.AddScore("urgency", ScoreQuestion.FromValues(
    ["Can wait", "Needs attention this week", "Needs attention today"], "How urgent is this?"));

var result = await client.SystemOneAsync("I was charged twice. Please help today.", questions);
```

### Structured state
Send structured JSON state without reflection, including in NativeAOT applications.

```csharp
using var client = new TypeSafeClient(apiKey);
var state = JsonContent.FromNode(new JsonObject
{
    ["subject"] = "Duplicate payment",
    ["message"] = "The same invoice appears twice.",
    ["customer_tier"] = "enterprise",
});
var questions = new Dictionary<string, Question>
{
    ["intent"] = ChoiceQuestion.FromValues(["billing", "support", "sales"]),
};

var response = await client.SystemOneAsync(state, questions);
```

### Custom response projection
Project named answers into an application-owned response type with source-generated JSON metadata.

```csharp
using var client = new TypeSafeClient(apiKey);
var questions = new Dictionary<string, Question>
{
    ["intent"] = ChoiceQuestion.FromValues(["billing", "support", "sales"]),
};

var response = await client.SystemOneAsync(
    (JsonContent)"I need a refund for a duplicate invoice.",
    questions,
    ExampleJsonContext.Default.TriageProjection);
```

### List models
Discover the model names and aliases available to the authenticated account.

```csharp
using var client = new TypeSafeClient(apiKey);

var models = await client.Models.ListAsync();
```

### Bounded batch
Evaluate independent states concurrently while limiting parallel requests.

```csharp
using var client = new TypeSafeClient(apiKey);
var questions = new Dictionary<string, Question>
{
    ["spam"] = new NoulQuestion("Is this unsolicited advertising?"),
};

var responses = await client.SystemOneManyAsync(
    ["Buy now! Limited offer!", "Can we move our meeting to Tuesday?"],
    questions,
    maxConcurrency: 2);
```

### Replay and forward compatibility
Decode cached HTTP bodies and preserve answer variants introduced by future API versions.

```csharp
const string json = """
    {"model":"jev-next","answers":{"known":{"type":"noul","noul":0.92},"future":{"type":"ranking","items":["a","b"]}},"usage":{"input_tokens":42,"output_tokens":4}}
    """;

var response = TypeSafeClient.FromHttpResponse(json);
```

### Typed intent routing
Route arbitrary state to a strongly typed enum intent.

```csharp
using var client = new TypeSafeClient(apiKey);
var router = new TypeSafeIntentRouter<TicketIntent>(client, "Choose the team that should own this ticket.");

var intent = await router.RouteAsync("I was charged twice for invoice 391.");
```

### Guardrail policy
Combine calibrated hazard probability and severity into an allow, review, or block decision.

```csharp
var hazard = new QuestionHandle<NoulAnswer>("harm");
var severity = new QuestionHandle<ScoreAnswer>("severity");
var response = TypeSafeClient.FromHttpResponse("""
    {"model":"jev-1","answers":{"harm":{"type":"noul","noul":0.8},"severity":{"type":"score","score":2.4,"confidence":0.9,"legend":{"0":"low","1":"medium","2":"high"},"probabilities":{"0":0.05,"1":0.1,"2":0.85}}},"usage":{"input_tokens":5,"output_tokens":2}}
    """);
var assessment = new GuardrailAssessment(
    GuardrailDirection.Input, new QuestionSetResult(response), [], null);
var policy = GuardrailPolicies.Thresholds(
    new Dictionary<QuestionHandle<NoulAnswer>, GuardrailAction> { [hazard] = GuardrailAction.Block },
    severity);
```

### Typed API errors
Catch status-specific exceptions with the response body and retry metadata.

```csharp
using var client = new TypeSafeClient("invalid-key", options: new TypeSafeClientOptions
{
    Retry = new RetryPolicy { MaxAttempts = 1 },
});

var action = async () => await client.Models.ListAsync();
```

### Protected headers
Credential and payload headers cannot be accidentally replaced by caller options.

```csharp
var options = new TypeSafeClientOptions();
options.DefaultHeaders["Authorization"] = "Bearer something-else";

var action = () => new TypeSafeClient("safe-placeholder", options: options);
```
<!-- EXAMPLES:END -->

## Support

<div class="docs-card-grid">
  <div class="docs-card">
    <h3>Bugs</h3>
    <p>Open an issue in <a href="https://github.com/tryAGI/TypeSafeAI/issues">tryAGI/TypeSafeAI</a>.</p>
  </div>
  <div class="docs-card">
    <h3>Ideas and questions</h3>
    <p>Use <a href="https://github.com/tryAGI/TypeSafeAI/discussions">GitHub Discussions</a> for design questions and usage help.</p>
  </div>
  <div class="docs-card">
    <h3>Community</h3>
    <p>Join the <a href="https://discord.gg/Ca2xhfBf3v">tryAGI Discord</a> for broader discussion across SDKs.</p>
  </div>
</div>

## Acknowledgments

![JetBrains logo](https://resources.jetbrains.com/storage/products/company/brand/logos/jetbrains.png)

This project is supported by JetBrains through the [Open Source Support Program](https://jb.gg/OpenSourceSupport).
