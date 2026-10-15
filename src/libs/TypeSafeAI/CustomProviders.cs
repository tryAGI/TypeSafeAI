namespace TypeSafeAI;

/// <summary>Providers that implement the System One request format.</summary>
public enum SystemOneProvider
{
    TypeSafe,
    Celeris,
    Perplexity,
    Inception,
    Liquid,
    Wity,
    SubQ,
    Sage,
    Vansa,
    Milliseconds,
    Instinct,
    Classifier,
    MicrosoftDecision,
    Compatible,
    Bespoke,
    Surd,
    Fastino,
    Amos,
}

/// <summary>Factories for compatible System One providers.</summary>
public static partial class CustomProviders
{
    /// <summary>The base URL without /v1; the generated transport appends /v1/systemone.</summary>
    public const string CelerisBaseUrl = "https://inference.celeris.ai/celeris-1-decision/";
    public const string CelerisModel = "celeris-1-decision";

    /// <summary>Creates a Celeris Decision client with provider-specific image encoding and validation.</summary>
    /// <param name="apiKey">A Celeris API key.</param>
    /// <param name="httpClient">An optional caller-owned HTTP client.</param>
    /// <param name="configure">Optional timeout, retry, logging, and endpoint configuration.</param>
    public static TypeSafeClient Celeris(
        string apiKey,
        HttpClient? httpClient = null,
        Action<TypeSafeClientOptions>? configure = null)
    {
        var options = CreateCelerisOptions(configure);
        return new TypeSafeClient(apiKey, httpClient, options);
    }

    internal static TypeSafeClientOptions CreateCelerisOptions(Action<TypeSafeClientOptions>? configure)
    {
        var options = new TypeSafeClientOptions
        {
            Provider = SystemOneProvider.Celeris,
            BaseUri = new Uri(CelerisBaseUrl),
            DefaultModel = CelerisModel,
        };
        configure?.Invoke(options);
        if (options.Provider != SystemOneProvider.Celeris)
            throw new ArgumentException("The Celeris factory requires the Celeris provider profile.", nameof(configure));
        return options;
    }
}
