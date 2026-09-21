using Microsoft.Extensions.DependencyInjection;
using TypeSafeAI;
using TypeSafeAI.Extensions.AI;

namespace Microsoft.Extensions.AI;

public static class TypeSafeChatClientBuilderExtensions
{
    public static ChatClientBuilder UseTypeSafeGuardrail(
        this ChatClientBuilder builder,
        ITypeSafeClient typeSafeClient,
        Action<GuardrailOptions> configure)
    {
        var options = new GuardrailOptions();
        configure(options);
        return builder.Use(inner => new TypeSafeGuardrailChatClient(inner, typeSafeClient, options));
    }

    public static ChatClientBuilder UseTypeSafeGuardrail(
        this ChatClientBuilder builder,
        Action<GuardrailOptions> configure)
    {
        var options = new GuardrailOptions();
        configure(options);
        return builder.Use((inner, services) => new TypeSafeGuardrailChatClient(
            inner, services.GetRequiredService<ITypeSafeClient>(), options));
    }

    public static ChatClientBuilder UseTypeSafeRouter(
        this ChatClientBuilder builder,
        ITypeSafeClient typeSafeClient,
        QuestionSet questions,
        Func<TypeSafeRoutingContext, IChatClient?> select,
        Action<RoutingOptions>? configure = null)
    {
        var options = new RoutingOptions();
        configure?.Invoke(options);
        return builder.Use(inner => new TypeSafeRoutingChatClient(typeSafeClient, questions, select, inner, options));
    }

    public static ChatClientBuilder UseTypeSafeRouter(
        this ChatClientBuilder builder,
        QuestionSet questions,
        Func<TypeSafeRoutingContext, IChatClient?> select,
        Action<RoutingOptions>? configure = null)
    {
        var options = new RoutingOptions();
        configure?.Invoke(options);
        return builder.Use((inner, services) => new TypeSafeRoutingChatClient(
            services.GetRequiredService<ITypeSafeClient>(), questions, select, inner, options));
    }
}
