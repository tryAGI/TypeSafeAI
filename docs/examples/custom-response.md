# Custom response projection

Project named answers into an application-owned response type with source-generated JSON metadata.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

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