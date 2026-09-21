# Typed intent routing

Route arbitrary state to a strongly typed enum intent.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

```csharp
using var client = new TypeSafeClient(apiKey);
var router = new TypeSafeIntentRouter<TicketIntent>(client, "Choose the team that should own this ticket.");

var intent = await router.RouteAsync("I was charged twice for invoice 391.");
```