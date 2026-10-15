using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TypeSafeAI;

public static class TypeSafeServiceCollectionExtensions
{
    public static IHttpClientBuilder AddTypeSafeClient(
        this IServiceCollection services,
        string apiKey,
        Action<TypeSafeClientOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        var options = new TypeSafeClientOptions();
        configure?.Invoke(options);
        return AddClient(services, apiKey, options);
    }

    /// <summary>Registers a Celeris Decision client with its endpoint, model, and provider profile.</summary>
    public static IHttpClientBuilder AddCelerisClient(
        this IServiceCollection services,
        string apiKey,
        Action<TypeSafeClientOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        return AddClient(services, apiKey, CustomProviders.CreateCelerisOptions(configure));
    }

    /// <summary>Registers Celeris using CELERIS_API_KEY, rejecting missing or empty credentials.</summary>
    public static IHttpClientBuilder AddCelerisClientFromEnvironment(
        this IServiceCollection services,
        Action<TypeSafeClientOptions>? configure = null)
    {
        var apiKey = Environment.GetEnvironmentVariable("CELERIS_API_KEY") is { Length: > 0 } value && !string.IsNullOrWhiteSpace(value)
            ? value : throw new InvalidOperationException("CELERIS_API_KEY is not set.");
        return services.AddCelerisClient(apiKey, configure);
    }

    private static IHttpClientBuilder AddClient(IServiceCollection services, string apiKey, TypeSafeClientOptions options)
    {
        services.TryAddSingleton(options);
        services.TryAddTransient<ITypeSafeClient>(provider => provider.GetRequiredService<TypeSafeClient>());
        return services.AddHttpClient<TypeSafeClient>(client => client.BaseAddress = options.BaseUri)
            .AddTypedClient(client => new TypeSafeClient(apiKey, client, options));
    }

    public static IHttpClientBuilder AddTypeSafeClientFromEnvironment(
        this IServiceCollection services,
        Action<TypeSafeClientOptions>? configure = null)
    {
        var apiKey = Environment.GetEnvironmentVariable("TYPESAFE_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("TYPESAFE_API_KEY is not set.");
        }
        return services.AddTypeSafeClient(apiKey, configure);
    }
}
