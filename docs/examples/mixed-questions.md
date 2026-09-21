# Mixed questions

Ask typed Noul, choice, and score questions in one request.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

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