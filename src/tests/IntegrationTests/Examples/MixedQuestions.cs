/*
order: 20
title: Mixed questions
slug: mixed-questions

Ask typed Noul, choice, and score questions in one request.
*/
namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task Example_MixedQuestionsAsync()
    {
        using var client = GetAuthenticatedClient();
        var questions = new QuestionSet();
        var billing = questions.AddNoul("billing", new NoulQuestion("Is this message about billing?"));
        var tone = questions.AddChoice("tone", ChoiceQuestion.FromValues(
            ["angry", "calm", "excited"], "What is the tone?"));
        var urgency = questions.AddScore("urgency", ScoreQuestion.FromValues(
            ["Can wait", "Needs attention this week", "Needs attention today"], "How urgent is this?"));

        var result = await client.SystemOneAsync("I was charged twice. Please help today.", questions);

        result.Get(billing).Noul.Should().BeInRange(0, 1);
        result.Get(tone).Probabilities.Values.Sum().Should().BeApproximately(1, 0.02);
        result.Get(urgency).MostLikely.Should().BeInRange(0, 2);
    }
}
