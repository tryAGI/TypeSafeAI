namespace TypeSafeAI;

/// <summary>Testable abstraction over TypeSafe AI System One.</summary>
public interface ITypeSafeClient
{
    Task<SystemOneResponse> SystemOneAsync(
        JsonContent state,
        IReadOnlyDictionary<string, Question> questions,
        string? model = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    Task<QuestionSetResult> SystemOneAsync(
        JsonContent state,
        QuestionSet questions,
        string? model = null,
        RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);
}
