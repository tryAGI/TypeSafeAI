# Generate

Create a client from `TYPESAFE_API_KEY` and discover the models available to the account.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

```csharp
using var client = TypeSafeClient.CreateFromEnvironment();

var models = await client.Models.ListAsync();
```