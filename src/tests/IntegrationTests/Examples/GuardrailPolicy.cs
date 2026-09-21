/*
order: 80
title: Guardrail policy
slug: guardrail-policy

Combine calibrated hazard probability and severity into an allow, review, or block decision.
*/
using TypeSafeAI.Extensions.AI;

namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public void Example_GuardrailThresholdPolicy()
    {
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

        policy(assessment).Action.Should().Be(GuardrailAction.Block);
    }
}
