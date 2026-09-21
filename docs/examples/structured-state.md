# Structured state

Send structured JSON state without reflection, including in NativeAOT applications.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

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