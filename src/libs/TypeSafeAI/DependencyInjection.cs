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
