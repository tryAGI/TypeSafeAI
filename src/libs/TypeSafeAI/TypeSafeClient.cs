using System.Diagnostics;
using System.Diagnostics.Metrics;
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
        _logger = _options.LoggerFactory?.CreateLogger<TypeSafeClient>();
        Models = new ModelsResource(this);
    }

    public TypeSafeClient(RawTypeSafeClient rawClient, TypeSafeClientOptions? options = null)
    {
        _raw = rawClient ?? throw new ArgumentNullException(nameof(rawClient));
        _options = options ?? new TypeSafeClientOptions();
        _raw.Options.Hooks.Add(_retryTracker);
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

        options ??= new TypeSafeClientOptions();
        if (Environment.GetEnvironmentVariable("TYPESAFE_BASE_URL") is { Length: > 0 } baseUrl)
        {
            options.BaseUri = new Uri(baseUrl, UriKind.Absolute);
        }
        if (Environment.GetEnvironmentVariable("TYPESAFE_DEFAULT_MODEL") is { Length: > 0 } model)
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

        try
        {
            _retryTracker.Reset();
            var rawResponse = await _raw.SystemoneV1SystemonePostAsResponseAsync(
                CreateRequest(state, questions, model ?? _options.DefaultModel, requestOptions),
                CreateRequestOptions(requestOptions),
                cancellationToken).ConfigureAwait(false);
            var response = Decode(rawResponse, _retryTracker.RetryCount);
            activity?.SetTag("gen_ai.response.model", response.Model);
            activity?.SetTag("gen_ai.usage.input_tokens", response.Usage.InputTokens);
            Requests.Add(1, new KeyValuePair<string, object?>("operation", "system_one"));
            InputTokens.Add(response.Usage.InputTokens);
            _logger?.LogDebug("TypeSafe System One returned {AnswerCount} answers using {InputTokens} input tokens.", response.Answers.Count, response.Usage.InputTokens);
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
        finally
        {
            Duration.Record(Stopwatch.GetElapsedTime(started).TotalMilliseconds,
                new KeyValuePair<string, object?>("operation", "system_one"));
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

    public async Task<TResponse> SystemOneAsync<TResponse>(
        JsonContent state,
        IReadOnlyDictionary<string, Question> questions,
        JsonTypeInfo<TResponse> responseTypeInfo,
        string? model = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SystemOneAsync(state, questions, model, requestOptions, cancellationToken).ConfigureAwait(false);
        var answerJson = response.RawJson.GetProperty("answers");
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

    private static SystemOneRequest CreateRequest(
        JsonContent state,
        IReadOnlyDictionary<string, Question> questions,
        string model,
        RequestOptions? options)
    {
        var questionElement = WriteQuestions(questions);
        var request = new SystemOneRequest(
            TypeSafeAI.Generated.AnyOf<string, object, IList<object>>.FromValue2(state.Value),
            model,
            questionElement);
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
        return JsonDocument.Parse(stream.ToArray()).RootElement.Clone();
    }

    private static Generated.AutoSDKRequestOptions? CreateRequestOptions(RequestOptions? options)
    {
        if (options is null)
        {
            return null;
        }

        var result = new Generated.AutoSDKRequestOptions { Timeout = options.Timeout };
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
        if (options.Retry is not null)
        {
            result.Retry = ToGenerated(options.Retry);
        }
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            result.Authorizations = [Generated.AutoSDKAuthorizationValue.Bearer(options.ApiKey)];
        }
        return result;
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

    private static SystemOneResponse Decode(
        Generated.AutoSDKHttpResponse<Generated.SystemOneResponse> response,
        int retryCount)
    {
        var requestId = GetHeader(response.Headers, "x-request-id") ?? GetHeader(response.Headers, "request-id");
        var metadata = new ResponseMetadata(response.StatusCode, response.Headers, response.RequestUri, requestId, retryCount);
        var body = response.Body;
        var answers = AsElement(body.Answers, "answers");
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("model", body.Model);
            writer.WritePropertyName("answers");
            answers.WriteTo(writer);
            writer.WritePropertyName("usage");
            writer.WriteStartObject();
            writer.WriteNumber("input_tokens", body.Usage.InputTokens);
            writer.WriteNumber("output_tokens", body.Usage.OutputTokens);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        var raw = JsonDocument.Parse(stream.ToArray()).RootElement.Clone();
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

    private static JsonElement AsElement(object value, string path) => value switch
    {
        JsonElement element => element,
        JsonDocument document => document.RootElement,
        _ => throw new TypeSafeResponseValidationException(
            $"AutoSDK returned an unsupported runtime value '{value.GetType().Name}'. See AutoSDK issue #399.", path),
    };

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
            HttpStatusCode.TooManyRequests => new RateLimitException(message, exception.ResponseBody, ApiException.TryParseRetryAfter(exception.ResponseHeaders), exception),
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
                var response = await client._raw.ModelsV1V1ModelsGetAsync(
                    CreateRequestOptions(requestOptions), cancellationToken).ConfigureAwait(false);
                return response.Models.Select(static model =>
                    new ModelMetadata(model.Name, model.Description, model.ReleaseDate)).ToArray();
            }
            catch (ApiException exception)
            {
                throw MapException(exception);
            }
        }
    }

    private sealed class RetryTrackingHook : Generated.AutoSDKHook
    {
        private readonly AsyncLocal<Holder?> _current = new();
        public int RetryCount => _current.Value?.RetryCount ?? 0;

        public void Reset() => _current.Value = new Holder();

        public override Task OnBeforeRequestAsync(Generated.AutoSDKHookContext context)
        {
            (_current.Value ??= new Holder()).RetryCount = Math.Max(0, context.Attempt - 1);
            return Task.CompletedTask;
        }

        private sealed class Holder
        {
            public int RetryCount { get; set; }
        }
    }
}
