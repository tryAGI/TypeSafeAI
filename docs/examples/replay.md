# Replay and forward compatibility

Decode cached HTTP bodies and preserve answer variants introduced by future API versions.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

```csharp
const string json = """
    {"model":"jev-next","answers":{"known":{"type":"noul","noul":0.92},"future":{"type":"ranking","items":["a","b"]}},"usage":{"input_tokens":42,"output_tokens":4}}
    """;

var response = TypeSafeClient.FromHttpResponse(json);
```