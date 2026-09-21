# Protected headers

Credential and payload headers cannot be accidentally replaced by caller options.

This example assumes `using TypeSafeAI;` is in scope and `apiKey` contains your TypeSafe AI API key.

```csharp
var options = new TypeSafeClientOptions();
options.DefaultHeaders["Authorization"] = "Bearer something-else";

var action = () => new TypeSafeClient("safe-placeholder", options: options);
```