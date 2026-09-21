# Guardrail policy

Combine calibrated hazard probability and severity into an allow, review, or block decision.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

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