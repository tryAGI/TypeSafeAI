# Request options and response metadata

The [HTTP reference](https://docs.typesafe.ai/api), [model documentation](https://docs.typesafe.ai/models), and SDK references expose request configuration alongside the question primitives.

```csharp
var settings = new TypeSafeClientOptions
{
    DefaultModel = "jev-1.14.0",
    Compression = RequestCompression.Gzip,
};
using var client = new TypeSafeClient(apiKey, options: settings);
var options = new RequestOptions
{
    EnableLongContext = true,
    Timeout = TimeSpan.FromSeconds(90),
    Retry = new RetryPolicy { MaxAttempts = 3, TotalTimeout = null },
};
var response = await client.SystemOneAsync(state, questions, requestOptions: options);
```

Long context is explicitly enabled per call by `EnableLongContext`; it sends `TypeSafe-Danger-Zone-Long-Context: true`. It does not alter the model. Consult the provider's current limits and evaluate accuracy on your data. For a default header across calls, use `TypeSafeClientOptions.DefaultHeaders`.

Compression defaults to `RequestCompression.None`. `Gzip` uses .NET's built-in implementation and compresses bodies of at least 1 KiB, including multipart image uploads. The original JSON/multipart content type is preserved; there is exactly one HTTP `Content-Encoding` layer. Each retry recreates and recompresses the content. Set per-call `Compression = RequestCompression.None` to override a client default. The provider applies the 32 MiB limit before and after decompression.

Python/Node can use native zstd in those runtimes. This .NET package uses the documented gzip alternative and does not bundle a zstd/native dependency. Compression selection and defaults therefore differ from those SDKs. `Content-Encoding` is SDK-managed; select compression through options rather than manually setting the header.

Other options include per-call headers, query parameters, API keys, timeout, retry policy, and `ExtraBody`. Required state/model/questions and managed image assets cannot be overwritten by extra fields. Use `RawQuestion` for future question fields or kinds; unknown answers remain accessible as `RawAnswer` and in `RawJson`.

`SystemOneResponse.Metadata` retains HTTP status, headers, request ID, request URI, and retry count. `RawJson` supports inspection and `TypeSafeClient.FromHttpResponse` supports replay. `Models.ListAsync` discovers model names and aliases; the actual resolved version is available in each System One response.

`Usage.InputTokens` and `Usage.OutputTokens` are nullable: null means the server did not report that count, rather than zero tokens. Numeric counts remain unchanged. This accommodates the published SDK response contract while the provider's OpenAPI still describes required integers. Telemetry and MEAI evaluation metadata omit unknown input counts. Callers doing arithmetic should handle null explicitly.

For cancellation, pass a `CancellationToken`; the client maps user cancellation, timeouts, connection failures, and provider HTTP errors to its exception types. Default retries include 408, 429, and all 5xx statuses, including 529. `RetryPolicy` controls attempts, statuses, backoff, jitter, retry headers, and a total budget; set `TotalTimeout = null` when only per-attempt timeouts are desired. Python/JS retry defaults and partial-override semantics are not identical to this .NET options model.

Reuse clients or configure dependency injection/`IHttpClientFactory`. A caller-owned `HttpClient` can supply handlers, proxies, connection limits, and transport hooks, and is not disposed by the SDK. Logging integrates with `ILoggerFactory`; activity/metrics are enabled independently with `EnableTelemetry`. Logging reports counts and operation metadata without copying request bodies or credentials into logs.
