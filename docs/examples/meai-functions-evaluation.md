# AI functions and evaluation

Use TypeSafe judgments as agent tools and evaluation metrics. These integrations ship in the main package.
Import Microsoft.Extensions.AI, Microsoft.Extensions.AI.Evaluation, TypeSafeAI.Extensions.AI, and TypeSafeAI.Extensions.AI.Evaluation.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

```csharp
using var client = new TypeSafeClient(apiKey);
var questions = new Dictionary<string, Question> { ["relevant"] = new NoulQuestion("Is the response relevant to the question?") };

// Expose fixed, reviewed questions to the calling agent; only state is supplied at invocation.
var function = TypeSafeAIFunctions.Create(client, questions, "judge_relevance", "Judge relevance.");
var result = await function.InvokeAsync(new AIFunctionArguments { ["state"] = "Question: hello. Response: hello." });

// The same judgment produces numeric evaluation metrics without a generative judge.
var evaluator = new TypeSafeEvaluator(client, questions, new TypeSafeEvaluatorOptions
{
    Interpret = TypeSafeInterpretations.ByQuestion(new Dictionary<string, Func<TypeSafeMetricContext, EvaluationMetricInterpretation?>>
    {
        ["relevant"] = TypeSafeInterpretations.NoulAtLeast(0.8),
    }),
});
var evaluation = await evaluator.EvaluateAsync([new ChatMessage(ChatRole.User, "hello")],
    new ChatResponse(new ChatMessage(ChatRole.Assistant, "hello")));
```