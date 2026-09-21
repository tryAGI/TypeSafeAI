/*
order: 80
title: AI functions and evaluation
slug: meai-functions-evaluation

Use TypeSafe judgments as agent tools and evaluation metrics. These integrations ship in the main package.
Import Microsoft.Extensions.AI, Microsoft.Extensions.AI.Evaluation, TypeSafeAI.Extensions.AI, and TypeSafeAI.Extensions.AI.Evaluation.
*/
using System.Net;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using TypeSafeAI.Extensions.AI;
using TypeSafeAI.Extensions.AI.Evaluation;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task Example_AIFunctionAndEvaluatorAsync()
    {
        using var client = GetAuthenticatedClient();
        var questions = new Dictionary<string, Question> { ["relevant"] = new NoulQuestion("Is the response relevant to the question?") };

        //// Expose fixed, reviewed questions to the calling agent; only state is supplied at invocation.
        var function = TypeSafeAIFunctions.Create(client, questions, "judge_relevance", "Judge relevance.");
        var result = await function.InvokeAsync(new AIFunctionArguments { ["state"] = "Question: hello. Response: hello." });
        ((JsonNode)result!)["answers"]!["relevant"]!["noul"]!.GetValue<double>().Should().BeInRange(0, 1);

        //// The same judgment produces numeric evaluation metrics without a generative judge.
        var evaluator = new TypeSafeEvaluator(client, questions, new TypeSafeEvaluatorOptions
        {
            Interpret = TypeSafeInterpretations.ByQuestion(new Dictionary<string, Func<TypeSafeMetricContext, EvaluationMetricInterpretation?>>
            {
                ["relevant"] = TypeSafeInterpretations.NoulAtLeast(0.8),
            }),
        });
        var evaluation = await evaluator.EvaluateAsync([new ChatMessage(ChatRole.User, "hello")],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, "hello")));
        ((NumericMetric)evaluation.Metrics["relevant"]).Value.Should().BeInRange(0, 1);
    }
}
