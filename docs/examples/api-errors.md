# Typed API errors

Catch status-specific exceptions with the response body and retry metadata.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

```csharp
using var client = new TypeSafeClient("invalid-key", options: new TypeSafeClientOptions
{
    Retry = new RetryPolicy { MaxAttempts = 1 },
});

var action = async () => await client.Models.ListAsync();
```