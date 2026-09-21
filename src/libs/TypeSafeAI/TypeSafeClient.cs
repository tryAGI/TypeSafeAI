using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using TypeSafeAI.Generated;

namespace TypeSafeAI;

/// <summary>First-class client for TypeSafe AI System One.</summary>
public sealed class TypeSafeClient : ITypeSafeClient, IDisposable, IAsyncDisposable
{
    private const string ActivitySourceName = "TypeSafeAI";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    private static readonly Meter Meter = new(ActivitySourceName);
    private static readonly Counter<long> Requests = Meter.CreateCounter<long>("typesafe.requests");
    private static readonly Counter<long> Errors = Meter.CreateCounter<long>("typesafe.errors");
    private static readonly Counter<long> InputTokens = Meter.CreateCounter<long>("typesafe.input_tokens");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("typesafe.request.duration", "ms");
    private static readonly HashSet<string> ProtectedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization", "Content-Type", "Content-Length", "Host",
    };

    private readonly RawTypeSafeClient _raw;
    private readonly TypeSafeClientOptions _options;
    private readonly ILogger<TypeSafeClient>? _logger;
    private readonly RetryTrackingHook _retryTracker = new();

    public TypeSafeClient(string apiKey, HttpClient? httpClient = null, TypeSafeClientOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        _options = options ?? new TypeSafeClientOptions();
        var generatedOptions = CreateGeneratedOptions(_options);
        _raw = new RawTypeSafeClient(
            httpClient,
            _options.BaseUri,
            authorizations: null,
            options: generatedOptions,
            disposeHttpClient: httpClient is null);
        _raw.AuthorizeUsingBearer(apiKey);
        _raw.Options.Hooks.Add(_retryTracker);
        ConfigureSerialization();
        _logger = _options.LoggerFactory?.CreateLogger<TypeSafeClient>();
        Models = new ModelsResource(this);
    }

    public TypeSafeClient(RawTypeSafeClient rawClient, TypeSafeClientOptions? options = null)
    {
        _raw = rawClient ?? throw new ArgumentNullException(nameof(rawClient));
        _options = options ?? new TypeSafeClientOptions();
        _raw.Options.Hooks.Add(_retryTracker);
        ConfigureSerialization();
        _logger = _options.LoggerFactory?.CreateLogger<TypeSafeClient>();
        Models = new ModelsResource(this);
    }

    public RawTypeSafeClient RawClient => _raw;
    public ModelsResource Models { get; }

    public static TypeSafeClient CreateFromEnvironment(HttpClient? httpClient = null, TypeSafeClientOptions? options = null)
    {
        var apiKey = Environment.GetEnvironmentVariable("TYPESAFE_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("TYPESAFE_API_KEY is not set.");
        }

        var useEnvironment = options is null;
        options ??= new TypeSafeClientOptions();
        if (useEnvironment && Environment.GetEnvironmentVariable("TYPESAFE_BASE_URL") is { Length: > 0 } baseUrl)
        {
            options.BaseUri = new Uri(baseUrl, UriKind.Absolute);
        }
        if (useEnvironment && Environment.GetEnvironmentVariable("TYPESAFE_DEFAULT_MODEL") is { Length: > 0 } model)
        {
            options.DefaultModel = model;
        }
        return new TypeSafeClient(apiKey, httpClient, options);
    }

    public SystemOneResponse SystemOne(
        JsonContent state,
        IReadOnlyDictionary<string, Question> questions,
        string? model = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        SystemOneAsync(state, questions, model, requestOptions, cancellationToken).GetAwaiter().GetResult();

    public Task<SystemOneResponse> SystemOneAsync(
        string state,
        IReadOnlyDictionary<string, Question> questions,
        string? model = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        SystemOneAsync((JsonContent)state, questions, model, requestOptions, cancellationToken);

    public async Task<SystemOneResponse> SystemOneAsync(
        JsonContent state,
        IReadOnlyDictionary<string, Question> questions,
        string? model = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(questions);
        if (questions.Count == 0)
        {
            throw new ArgumentException("At least one question is required.", nameof(questions));
        }

        using var activity = _options.EnableTelemetry ? ActivitySource.StartActivity("system_one", ActivityKind.Client) : null;
        activity?.SetTag("gen_ai.system", "typesafe-ai");
        activity?.SetTag("gen_ai.request.model", model ?? _options.DefaultModel);
        activity?.SetTag("typesafe.question_count", questions.Count);
        var started = Stopwatch.GetTimestamp();
        var succeeded = false;

        try
        {
            _retryTracker.Reset();
            var request = CreateRequest(state, questions, model ?? _options.DefaultModel, requestOptions);
            var rawResponse = await SendWithRetryAsync(
                (options, ct) => _raw.SystemoneV1SystemonePostAsResponseAsync(request, options, ct),
                requestOptions, cancellationToken).ConfigureAwait(false);
            var response = Decode(rawResponse, _retryTracker.RetryCount);
            activity?.SetTag("gen_ai.response.model", response.Model);
            activity?.SetTag("gen_ai.usage.input_tokens", response.Usage.InputTokens);
            if (_options.EnableTelemetry)
            {
                InputTokens.Add(response.Usage.InputTokens);
            }
            _logger?.LogDebug("TypeSafe System One returned {AnswerCount} answers using {InputTokens} input tokens.", response.Answers.Count, response.Usage.InputTokens);
            succeeded = true;
            return response;
        }
        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
        {
            throw new TypeSafeUserAbortException("The TypeSafe request was cancelled by the caller.", exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new TypeSafeTimeoutException("The TypeSafe request timed out.", exception);
        }
        catch (HttpRequestException exception)
        {
            throw new TypeSafeConnectionException("Unable to connect to the TypeSafe API.", exception);
        }
        catch (ApiException exception)
        {
            throw MapException(exception);
        }
        catch (JsonException exception)
        {
            throw new TypeSafeResponseValidationException("The response did not match the API contract.", exception.Path, exception);
        }
        finally
        {
            if (_options.EnableTelemetry)
            {
                Requests.Add(1, new KeyValuePair<string, object?>("operation", "system_one"));
                if (!succeeded)
                {
                    Errors.Add(1, new KeyValuePair<string, object?>("operation", "system_one"));
                    activity?.SetStatus(ActivityStatusCode.Error);
                }
                Duration.Record(Stopwatch.GetElapsedTime(started).TotalMilliseconds,
                    new KeyValuePair<string, object?>("operation", "system_one"));
            }
        }
    }

    public async Task<QuestionSetResult> SystemOneAsync(
        string state,
        QuestionSet questions,
        string? model = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        await SystemOneAsync((JsonContent)state, questions, model, requestOptions, cancellationToken).ConfigureAwait(false);

    public async Task<QuestionSetResult> SystemOneAsync(
        JsonContent state,
        QuestionSet questions,
        string? model = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default) =>
        new(await SystemOneAsync(state, (IReadOnlyDictionary<string, Question>)questions, model, requestOptions, cancellationToken).ConfigureAwait(false));

    public async Task<TResponse> SystemOneAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TResponse>(
        JsonContent state,
        IReadOnlyDictionary<string, Question> questions,
        JsonTypeInfo<TResponse> responseTypeInfo,
        string? model = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SystemOneAsync(state, questions, model, requestOptions, cancellationToken).ConfigureAwait(false);
        var answerJson = response.RawJson.GetProperty("answers");
        foreach (var property in responseTypeInfo.Properties)
        {
            if (!typeof(Answer).IsAssignableFrom(property.PropertyType) && !property.IsRequired) continue;
            var clrProperty = typeof(TResponse).GetProperties().FirstOrDefault(p =>
                string.Equals(p.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>()?.Name ??
                    responseTypeInfo.Options.PropertyNamingPolicy?.ConvertName(p.Name) ?? p.Name, property.Name, StringComparison.Ordinal));
            var optional = clrProperty?.IsDefined(typeof(OptionalAnswerAttribute), inherit: true) == true;
            if (!answerJson.TryGetProperty(property.Name, out var value) || value.ValueKind == JsonValueKind.Null)
            {
                if (!optional) throw new TypeSafeResponseValidationException("A required answer is missing.", property.Name);
                continue;
            }
            var expected = property.PropertyType == typeof(NoulAnswer) ? "noul" : property.PropertyType == typeof(ChoiceAnswer) ? "choice" : property.PropertyType == typeof(ScoreAnswer) ? "score" : null;
            if (expected is not null && (!value.TryGetProperty("type", out var discriminator) || discriminator.GetString() != expected))
                throw new TypeSafeResponseValidationException($"Expected a {expected} answer.", property.Name);
        }
        try
        {
            return JsonSerializer.Deserialize(answerJson, responseTypeInfo)
                ?? throw new TypeSafeResponseValidationException("The custom response was null.");
        }
        catch (JsonException exception)
        {
            throw new TypeSafeResponseValidationException("The custom response did not match its JSON contract.", exception.Path, exception);
        }
    }

    public async Task<IReadOnlyList<SystemOneResponse>> SystemOneManyAsync(
        IEnumerable<JsonContent> states,
        IReadOnlyDictionary<string, Question> questions,
        int maxConcurrency = 4,
        string? model = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrency, 1);
        var values = states?.ToArray() ?? throw new ArgumentNullException(nameof(states));
        using var gate = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var tasks = values.Select(async state =>
        {
            await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await SystemOneAsync(state, questions, model, requestOptions, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                gate.Release();
            }
        });
        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public static SystemOneResponse FromHttpResponse(string json, ResponseMetadata? metadata = null)
    {
        using var document = JsonDocument.Parse(json);
        return Decode(document.RootElement, metadata ?? new ResponseMetadata(HttpStatusCode.OK,
            new Dictionary<string, IEnumerable<string>>(), null, null));
    }

    public void Dispose() => _raw.Dispose();
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    private void ConfigureSerialization()
    {
        var options = new JsonSerializerOptions(_raw.JsonSerializerContext.Options);
        options.Converters.Insert(0, new ForwardCompatibleQuestionConverter());
        options.Converters.Insert(0, new ForwardCompatibleAnswerConverter());
        _raw.JsonSerializerContext = new SourceGenerationContext(options);
    }

    private SystemOneRequest CreateRequest(
        JsonContent state,
        IReadOnlyDictionary<string, Question> questions,
        string model,
        RequestOptions? options)
    {
        var questionElement = WriteQuestions(questions);
        var request = new SystemOneRequest(
            TypeSafeAI.Generated.AnyOf<string, object, IList<object>>.FromValue2(state.Value),
            model,
            questionElement.EnumerateObject().ToDictionary(
                static property => property.Name,
                property => JsonSerializer.Deserialize(property.Value,
                    (JsonTypeInfo<Generated.Question>)_raw.JsonSerializerContext.GetTypeInfo(typeof(Generated.Question))!),
                StringComparer.Ordinal));
        if (options is not null)
        {
            foreach (var pair in options.ExtraBody)
            {
                if (pair.Key is "state" or "model" or "questions")
                {
                    throw new ArgumentException($"ExtraBody cannot override required field '{pair.Key}'.", nameof(options));
                }
                request.AdditionalProperties[pair.Key] = pair.Value;
            }
        }
        return request;
    }

    private static JsonElement WriteQuestions(IReadOnlyDictionary<string, Question> questions)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var pair in questions)
            {
                writer.WritePropertyName(pair.Key);
                pair.Value.Write(writer);
            }
            writer.WriteEndObject();
        }
        using var document = JsonDocument.Parse(stream.ToArray());
        return document.RootElement.Clone();
    }

    private static Generated.AutoSDKRequestOptions CreateRequestOptions(RequestOptions? options)
    {
        var result = new Generated.AutoSDKRequestOptions { Timeout = options?.Timeout,
            Retry = new Generated.AutoSDKRetryOptions { MaxAttempts = 1 } };
        if (options is null)
        {
            return result;
        }
        foreach (var pair in options.Headers)
        {
            if (ProtectedHeaders.Contains(pair.Key))
            {
                throw new ArgumentException($"Header '{pair.Key}' is managed by the SDK.", nameof(options));
            }
            result.Headers[pair.Key] = pair.Value;
        }
        foreach (var pair in options.QueryParameters)
        {
            result.QueryParameters[pair.Key] = pair.Value;
        }
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            result.Authorizations = [Generated.AutoSDKAuthorizationValue.Bearer(options.ApiKey)];
        }
        return result;
    }

    private async Task<T> SendWithRetryAsync<T>(Func<AutoSDKRequestOptions, CancellationToken, Task<T>> send,
        RequestOptions? requestOptions, CancellationToken ct)
    {
        var policy = requestOptions?.Retry ?? _options.Retry;
        ArgumentOutOfRangeException.ThrowIfLessThan(policy.MaxAttempts, 1);
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (policy.TotalTimeout is { } timeout) budget.CancelAfter(timeout);
        for (var attempt = 1; ; attempt++)
        {
            budget.Token.ThrowIfCancellationRequested();
            _retryTracker.SetAttempt(attempt);
            try
            {
                return await send(CreateRequestOptions(requestOptions), budget.Token).ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < policy.MaxAttempts && !budget.IsCancellationRequested &&
                (ex switch
                {
                    ApiException api => policy.HttpStatuses.Contains(api.StatusCode),
                    HttpRequestException => policy.RetryConnectionErrors,
                    OperationCanceledException => policy.RetryTimeouts,
                    _ => false,
                } || policy.Predicate?.Invoke(ex) == true))
            {
                var milliseconds = Math.Min(policy.MaximumDelay.TotalMilliseconds,
                    policy.InitialDelay.TotalMilliseconds * Math.Pow(Math.Max(1, policy.BackoffMultiplier), attempt - 1));
                milliseconds *= 1 + (System.Security.Cryptography.RandomNumberGenerator.GetInt32(10000) / 5000.0 - 1) * Math.Clamp(policy.JitterRatio, 0, 1);
                var delay = TimeSpan.FromMilliseconds(Math.Clamp(milliseconds, 0, policy.MaximumDelay.TotalMilliseconds));
                if (policy.UseRetryAfterHeader && ex is ApiException api && ReadRetryAfter(api.ResponseHeaders, policy.TimeProvider) is { } retryAfter)
                    delay = retryAfter > policy.MaximumRetryAfter ? policy.MaximumRetryAfter : retryAfter;
                await Task.Delay(delay, policy.TimeProvider, budget.Token).ConfigureAwait(false);
            }
        }
    }

    private static TimeSpan? ReadRetryAfter(IDictionary<string, IEnumerable<string>>? headers, TimeProvider clock)
    {
        if (headers is null) return null;
        string? Read(string name) => headers.FirstOrDefault(pair => string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase)).Value?.FirstOrDefault();
        if (double.TryParse(Read("retry-after-ms"), NumberStyles.Float, CultureInfo.InvariantCulture, out var milliseconds) &&
            double.IsFinite(milliseconds) && milliseconds >= 0 && milliseconds < TimeSpan.MaxValue.TotalMilliseconds)
            return TimeSpan.FromMilliseconds(milliseconds);
        var raw = Read("retry-after");
        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) &&
            double.IsFinite(seconds) && seconds >= 0 && seconds < TimeSpan.MaxValue.TotalSeconds)
            return TimeSpan.FromSeconds(seconds);
        if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
            return date > clock.GetUtcNow() ? date - clock.GetUtcNow() : TimeSpan.Zero;
        return null;
    }

    private static Generated.AutoSDKClientOptions CreateGeneratedOptions(TypeSafeClientOptions options)
    {
        var result = new Generated.AutoSDKClientOptions
        {
            Timeout = options.Timeout,
            Retry = ToGenerated(options.Retry),
        };
        foreach (var pair in options.DefaultHeaders)
        {
            if (ProtectedHeaders.Contains(pair.Key))
            {
                throw new ArgumentException($"Header '{pair.Key}' is managed by the SDK.", nameof(options));
            }
            result.Headers[pair.Key] = pair.Value;
        }
        return result;
    }

    private static Generated.AutoSDKRetryOptions ToGenerated(RetryPolicy retry) => new()
    {
        MaxAttempts = retry.MaxAttempts,
        InitialDelay = retry.InitialDelay,
        MaxDelay = retry.MaximumDelay,
        BackoffMultiplier = retry.BackoffMultiplier,
        JitterRatio = retry.JitterRatio,
        UseRetryAfterHeader = retry.UseRetryAfterHeader,
    };

    private SystemOneResponse Decode(
        Generated.AutoSDKHttpResponse<Generated.SystemOneResponse> response,
        int retryCount)
    {
        var requestId = GetHeader(response.Headers, "x-typesafe-request-id") ?? GetHeader(response.Headers, "x-request-id") ?? GetHeader(response.Headers, "request-id");
        var metadata = new ResponseMetadata(response.StatusCode, response.Headers, response.RequestUri, requestId, retryCount);
        var raw = JsonSerializer.SerializeToElement(response.Body,
            (JsonTypeInfo<Generated.SystemOneResponse>)_raw.JsonSerializerContext.GetTypeInfo(typeof(Generated.SystemOneResponse))!);
        return Decode(raw, metadata);
    }

    private static SystemOneResponse Decode(JsonElement root, ResponseMetadata metadata)
    {
        try
        {
            var model = root.GetProperty("model").GetString()
                ?? throw new TypeSafeResponseValidationException("Response model is null.", "model");
            var answers = new Dictionary<string, Answer>(StringComparer.Ordinal);
            foreach (var property in root.GetProperty("answers").EnumerateObject())
            {
                answers[property.Name] = DecodeAnswer(property.Value, $"answers.{property.Name}");
            }
            var usageElement = root.GetProperty("usage");
            var usage = new Usage(usageElement.GetProperty("input_tokens").GetInt32(), usageElement.GetProperty("output_tokens").GetInt32());
            return new SystemOneResponse(model, new AnswerCollection(answers), usage, metadata, root.Clone());
        }
        catch (KeyNotFoundException exception)
        {
            throw new TypeSafeResponseValidationException("The response omitted a required field.", inner: exception);
        }
        catch (InvalidOperationException exception)
        {
            throw new TypeSafeResponseValidationException("The response contained an invalid field value.", inner: exception);
        }
    }

    private static Answer DecodeAnswer(JsonElement element, string path)
    {
        var type = element.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
        return type switch
        {
            "noul" => new NoulAnswer(element.GetProperty("noul").GetDouble()) { RawJson = element.Clone() },
            "choice" => new ChoiceAnswer(
                element.GetProperty("choice").GetString() ?? throw new TypeSafeResponseValidationException("Choice is null.", path + ".choice"),
                element.GetProperty("confidence").GetDouble(),
                ReadDoubleDictionary(element.GetProperty("probabilities"))) { RawJson = element.Clone() },
            "score" => new ScoreAnswer(
                element.GetProperty("score").GetDouble(),
                element.GetProperty("confidence").GetDouble(),
                ReadElementDictionary(element.GetProperty("legend")),
                ReadDoubleDictionary(element.GetProperty("probabilities"))) { RawJson = element.Clone() },
            _ => new RawAnswer(type ?? "unknown", element.Clone()) { RawJson = element.Clone() },
        };
    }

    private static IReadOnlyDictionary<string, double> ReadDoubleDictionary(JsonElement element) =>
        element.EnumerateObject().ToDictionary(static p => p.Name, static p => p.Value.GetDouble(), StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, JsonElement> ReadElementDictionary(JsonElement element) =>
        element.EnumerateObject().ToDictionary(static p => p.Name, static p => p.Value.Clone(), StringComparer.Ordinal);

    private static string? GetHeader(IReadOnlyDictionary<string, IEnumerable<string>> headers, string name) =>
        headers.TryGetValue(name, out var values) ? values.FirstOrDefault() : null;

    private static TypeSafeApiException MapException(ApiException exception)
    {
        var message = string.IsNullOrWhiteSpace(exception.Message) ? "The TypeSafe API rejected the request." : exception.Message;
        return exception.StatusCode switch
        {
            HttpStatusCode.BadRequest => new BadRequestException(message, exception.ResponseBody, exception),
            HttpStatusCode.Unauthorized => new AuthenticationException(message, exception.ResponseBody, exception),
            HttpStatusCode.Forbidden => new PermissionDeniedException(message, exception.ResponseBody, exception),
            HttpStatusCode.NotFound => new NotFoundException(message, exception.ResponseBody, exception),
            HttpStatusCode.UnprocessableEntity => new UnprocessableEntityException(message, exception.ResponseBody, exception),
            HttpStatusCode.TooManyRequests => new RateLimitException(message, exception.ResponseBody, ReadRetryAfter(exception.ResponseHeaders, TimeProvider.System), exception),
            >= HttpStatusCode.InternalServerError => new InternalServerException(message, exception.StatusCode, exception.ResponseBody, exception),
            _ => new TypeSafeApiException(message, exception.StatusCode, exception.ResponseBody, exception),
        };
    }

    public sealed class ModelsResource(TypeSafeClient client)
    {
        public IReadOnlyList<ModelMetadata> List(CancellationToken cancellationToken = default) =>
            ListAsync(cancellationToken: cancellationToken).GetAwaiter().GetResult();

        public async Task<IReadOnlyList<ModelMetadata>> ListAsync(
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                client._retryTracker.Reset();
                var response = await client.SendWithRetryAsync(
                    (options, ct) => client._raw.ModelsV1V1ModelsGetAsync(options, ct), requestOptions, cancellationToken).ConfigureAwait(false);
                return response.Models.Select(static model =>
                    new ModelMetadata(model.Name, model.Description, model.ReleaseDate)).ToArray();
            }
            catch (ApiException exception)
            {
                throw MapException(exception);
            }
            catch (JsonException exception)
            {
                throw new TypeSafeResponseValidationException("The response did not match the API contract.", exception.Path, exception);
            }
            catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
            {
                throw new TypeSafeUserAbortException("The TypeSafe request was cancelled by the caller.", exception);
            }
            catch (OperationCanceledException exception)
            {
                throw new TypeSafeTimeoutException("The TypeSafe request timed out.", exception);
            }
            catch (HttpRequestException exception)
            {
                throw new TypeSafeConnectionException("Unable to connect to the TypeSafe API.", exception);
            }
        }
    }

    private sealed class RetryTrackingHook : Generated.AutoSDKHook
    {
        private readonly AsyncLocal<Holder?> _current = new();
        public int RetryCount => _current.Value?.RetryCount ?? 0;

        public void Reset() => _current.Value = new Holder();
        public void SetAttempt(int attempt) => (_current.Value ??= new Holder()).RetryCount = attempt - 1;

        public override Task OnBeforeRequestAsync(Generated.AutoSDKHookContext context)
        {
            return Task.CompletedTask;
        }

        private sealed class Holder
        {
            public int RetryCount { get; set; }
        }
    }
}
