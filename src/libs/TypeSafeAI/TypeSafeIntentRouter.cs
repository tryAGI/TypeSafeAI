namespace TypeSafeAI;

/// <summary>Routes state to a typed enum intent using a choice question.</summary>
public sealed class TypeSafeIntentRouter<TIntent>(TypeSafeClient client, JsonContent? instructions = null)
    where TIntent : struct, Enum
{
    public async Task<TIntent> RouteAsync(
        JsonContent state,
        string? model = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.SystemOneAsync(
            state,
            new Dictionary<string, Question>(StringComparer.Ordinal)
            {
                ["intent"] = ChoiceQuestion.FromEnum<TIntent>(instructions),
            },
            model,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Get<ChoiceAnswer>("intent").AsEnum<TIntent>();
    }
}
