# Bounded batch

Evaluate independent states concurrently while limiting parallel requests.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

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