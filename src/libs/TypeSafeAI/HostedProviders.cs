namespace TypeSafeAI;

public static partial class CustomProviders
{
    /// <summary>Creates a Perplexity Decider client using /v1/decisions.</summary>
    public static TypeSafeClient Perplexity(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Perplexity, "https://api.perplexity.ai/", "pplx-decider-v1.1-27b", apiKey, httpClient, configure);

    /// <summary>Creates an Inception Mercury Decide client using /v1/decisions.</summary>
    public static TypeSafeClient Inception(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Inception, "https://api.inceptionlabs.ai/", "mercury-decide", apiKey, httpClient, configure);

    /// <summary>Creates a Liquid d1 client using the documented decisions service prefix.</summary>
    public static TypeSafeClient Liquid(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Liquid, "https://api.liquid.ai/decisions/", "d1", apiKey, httpClient, configure);

    /// <summary>Creates a Wity client. Native reasoning options can be supplied through ExtraBody.</summary>
    public static TypeSafeClient Wity(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Wity, "https://api.wity.alphanimble.com/", "wity-1", apiKey, httpClient, configure);

    /// <summary>Creates a SubQ Snap public preview client.</summary>
    public static TypeSafeClient SubQ(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.SubQ, "https://api.subq.ai/", "subq-snap-preview", apiKey, httpClient, configure);

    /// <summary>Creates a Levanto Sage SystemOne client. Images require Sage's separate /decide API.</summary>
    public static TypeSafeClient Sage(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Sage, "https://sage.levanto.ai/", "sage-latest", apiKey, httpClient, configure);

    /// <summary>Creates a Vansa text decision client.</summary>
    public static TypeSafeClient Vansa(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Vansa, "https://api.vansa.org/", "vansa-3.4", apiKey, httpClient, configure);

    /// <summary>Creates a milliseconds.ai SystemOne client. Images require its native API.</summary>
    public static TypeSafeClient Milliseconds(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Milliseconds, "https://api.milliseconds.ai/", "decision-machine-1", apiKey, httpClient, configure);

    /// <summary>Creates a ZooWork Instinct preview client. The paid ZooData route uses separate credentials and a BaseUri override.</summary>
    public static TypeSafeClient Instinct(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Instinct, "https://api.zoowork.ai/", "instinct", apiKey, httpClient, configure);

    /// <summary>Creates a classifier.dev client. Supply its workspace key or "unused" for documented anonymous access.</summary>
    public static TypeSafeClient Classifier(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Classifier, "https://classifier.dev/", "jev", apiKey, httpClient, configure);

    /// <summary>Creates a Bespoke Labs Nimble text decision client.</summary>
    public static TypeSafeClient Bespoke(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Bespoke, "https://api.bespokelabs.ai/", "nimble-latest", apiKey, httpClient, configure);

    /// <summary>Creates a Surd AI SPX-CD Flash text decision client. Pro is selected by DefaultModel or per call.</summary>
    public static TypeSafeClient Surd(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Surd, "https://api.simplexai.net/", "spx-cd-flash", apiKey, httpClient, configure);

    /// <summary>Creates a Fastino GLiDE text decision client, including bounded warm-up retries.</summary>
    public static TypeSafeClient Fastino(string apiKey, HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Fastino, "https://api.fastino.ai/", "fastino/GLiDE", apiKey, httpClient, options =>
        {
            options.Timeout = TimeSpan.FromSeconds(300);
            options.Retry.TotalTimeout = TimeSpan.FromMinutes(6);
            options.Retry.HttpStatuses.Add((System.Net.HttpStatusCode)425);
            configure?.Invoke(options);
        });

    /// <summary>Creates a Scira Amos text decision client. Its public endpoint currently needs no key; "unused" is an ignored bearer placeholder.</summary>
    public static TypeSafeClient Amos(string apiKey = "unused", HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null) =>
        CreateHosted(SystemOneProvider.Amos, "https://api.amos.scira.ai/", "amos-1-large", apiKey, httpClient, configure);

    /// <summary>Creates a Microsoft Foundry Decision client using a caller-managed Entra bearer token.</summary>
    /// <param name="accessToken">An Entra access token. Refresh it through RequestOptions.ApiKey for subsequent calls.</param>
    /// <param name="resourceEndpoint">The Foundry resource endpoint, without /providers/microsoft/v1.</param>
    /// <param name="deploymentName">The deployment name sent as model.</param>
    /// <param name="httpClient">An optional caller-owned HTTP client.</param>
    /// <param name="configure">Optional transport configuration.</param>
    public static TypeSafeClient MicrosoftDecision(string accessToken, Uri resourceEndpoint, string deploymentName,
        HttpClient? httpClient = null, Action<TypeSafeClientOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(resourceEndpoint);
        ValidateBaseUri(resourceEndpoint);
        return CreateHosted(SystemOneProvider.MicrosoftDecision, resourceEndpoint.AbsoluteUri.TrimEnd('/') + "/providers/microsoft/",
            deploymentName, accessToken, httpClient, configure);
    }

    /// <summary>Creates a text client for a caller-verified SystemOne-compatible service. No discovery or image capability is assumed.</summary>
    /// <param name="apiKey">A credential issued by the selected service.</param>
    /// <param name="baseUri">The service base, without /v1/systemone. Prefix paths are preserved.</param>
    /// <param name="model">The service's model identifier.</param>
    /// <param name="httpClient">An optional caller-owned HTTP client.</param>
    /// <param name="configure">Optional transport configuration.</param>
    public static TypeSafeClient SystemOne(string apiKey, Uri baseUri, string model, HttpClient? httpClient = null,
        Action<TypeSafeClientOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        ValidateBaseUri(baseUri);
        return CreateHosted(SystemOneProvider.Compatible, baseUri.AbsoluteUri, model, apiKey, httpClient, configure);
    }

    private static TypeSafeClient CreateHosted(SystemOneProvider provider, string baseUrl, string model, string apiKey,
        HttpClient? httpClient, Action<TypeSafeClientOptions>? configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        var options = new TypeSafeClientOptions { Provider = provider, BaseUri = new Uri(baseUrl), DefaultModel = model };
        configure?.Invoke(options);
        if (options.Provider != provider) throw new ArgumentException("The factory requires its own provider profile.", nameof(configure));
        ValidateBaseUri(options.BaseUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.DefaultModel);
        return new TypeSafeClient(apiKey, httpClient, options);
    }

    private static void ValidateBaseUri(Uri baseUri)
    {
        if (!baseUri.IsAbsoluteUri || baseUri.Scheme != Uri.UriSchemeHttps || baseUri.Query.Length > 0 || baseUri.Fragment.Length > 0 || baseUri.UserInfo.Length > 0)
            throw new ArgumentException("A provider base URI must be an absolute HTTPS URL without query, fragment, or credentials.", nameof(baseUri));
    }
}
