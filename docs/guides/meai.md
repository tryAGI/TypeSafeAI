# Microsoft.Extensions.AI Integration

!!! tip "Cross-SDK comparison"
    See the [centralized MEAI documentation](https://tryagi.github.io/docs/meai/) for feature matrices and comparisons across all tryAGI SDKs.

The TypeSafeAI SDK provides integration with [Microsoft.Extensions.AI](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai), enabling seamless interoperability with the unified .NET AI abstractions.

## Installation

```bash
dotnet add package tryAGI.TypeSafeAI.Extensions.AI
```

## Usage

```csharp
using Microsoft.Extensions.AI;
using TypeSafeAI;
using TypeSafeAI.Extensions.AI;

using var judgments = TypeSafeClient.CreateFromEnvironment();
var hazards = new QuestionSet();
var unsafeContent = hazards.AddNoul("unsafe", new NoulQuestion("Is this unsafe?"));

IChatClient guarded = new ChatClientBuilder(innerChatClient)
    .UseTypeSafeGuardrail(judgments, options =>
    {
        options.InputQuestions = hazards;
        options.Decide = assessment => assessment.Get(unsafeContent).Noul >= 0.7
            ? GuardrailDecision.Block("unsafe content")
            : GuardrailDecision.Allow;
    })
    .Build();
```

The integration package includes:

- input and output guardrails, with optional buffered streaming checks;
- `TypeSafeRoutingChatClient` for calibrated intent or confidence routing;
- `TypeSafeAIFunctions.Create` for agent-callable judgments;
- `TypeSafeEvaluator` for `Microsoft.Extensions.AI.Evaluation` numeric, boolean, and string metrics.

## Next Steps

- Check the [Examples](../index.md) for complete working code
- See the [centralized MEAI docs](https://tryagi.github.io/docs/meai/) for cross-SDK comparisons
- Visit the [Microsoft.Extensions.AI documentation](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai) for framework details
