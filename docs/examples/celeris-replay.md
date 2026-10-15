# Celeris explanations and replay

Read typed Celeris explanations from a saved System One response without making a network request.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

```csharp
// Celeris explanations are keyed by question name. The same typed metadata is available on live responses and QuestionSetResult.Response.
const string json = """
    {"model":"celeris-1-decision","answers":{"refund":{"type":"noul","noul":0.95}},
     "usage":{"input_tokens":122,"output_tokens":15},
     "x_celeris":{"explanations":{"refund":"The customer explicitly requests a refund."}}}
    """;
var response = TypeSafeClient.FromHttpResponse(json);
Console.WriteLine(response.Get<NoulAnswer>("refund").Noul);
Console.WriteLine(response.Celeris!.Explanations["refund"]);
```