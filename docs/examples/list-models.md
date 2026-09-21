# List models

Discover the model names and aliases available to the authenticated account.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

```csharp
using var client = new TypeSafeClient(apiKey);

var models = await client.Models.ListAsync();
```