namespace TypeSafeAI;

/// <summary>A stable typed handle to an answer in a question set.</summary>
public readonly record struct QuestionHandle<TAnswer>(string Name) where TAnswer : Answer;

/// <summary>A named collection of mixed question types.</summary>
public sealed class QuestionSet : IReadOnlyDictionary<string, Question>
{
    private readonly Dictionary<string, Question> _questions = new(StringComparer.Ordinal);

    public IEnumerable<string> Keys => _questions.Keys;
    public IEnumerable<Question> Values => _questions.Values;
    public int Count => _questions.Count;
    public Question this[string key] => _questions[key];

    public QuestionHandle<NoulAnswer> AddNoul(string name, NoulQuestion question)
    {
        Add(name, question);
        return new(name);
    }

    public QuestionHandle<ChoiceAnswer> AddChoice(string name, ChoiceQuestion question)
    {
        Add(name, question);
        return new(name);
    }

    public ChoiceHandle<TEnum> AddChoice<TEnum>(string name, JsonContent? instructions = null) where TEnum : struct, Enum
    {
        Add(name, ChoiceQuestion.FromEnum<TEnum>(instructions));
        return new(name);
    }

    public ScoreHandle<TEnum> AddScore<TEnum>(string name, JsonContent? instructions = null) where TEnum : struct, Enum
    {
        Add(name, ScoreQuestion.FromEnum<TEnum>(instructions));
        return new(name);
    }

    public QuestionHandle<ScoreAnswer> AddScore(string name, ScoreQuestion question)
    {
        Add(name, question);
        return new(name);
    }

    public QuestionSet Add(string name, Question question)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(question);
        if (!_questions.TryAdd(name, question))
        {
            throw new ArgumentException($"Question '{name}' already exists.", nameof(name));
        }
        return this;
    }

    public bool ContainsKey(string key) => _questions.ContainsKey(key);
    public bool TryGetValue(string key, out Question value) => _questions.TryGetValue(key, out value!);
    public IEnumerator<KeyValuePair<string, Question>> GetEnumerator() => _questions.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>A response tied to the question set that produced it.</summary>
public sealed record QuestionSetResult(SystemOneResponse Response)
{
    public ChoiceAnswer<TEnum> Get<TEnum>(ChoiceHandle<TEnum> handle) where TEnum : struct, Enum =>
        new(Response.Get<ChoiceAnswer>(handle.Name));
    public ScoreAnswer<TEnum> Get<TEnum>(ScoreHandle<TEnum> handle) where TEnum : struct, Enum =>
        new(Response.Get<ScoreAnswer>(handle.Name));
    public TAnswer Get<TAnswer>(QuestionHandle<TAnswer> handle) where TAnswer : Answer =>
        Response.Answers.Get<TAnswer>(handle.Name);
}
