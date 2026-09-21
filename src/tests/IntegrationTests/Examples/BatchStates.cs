/*
order: 50
title: Bounded batch
slug: bounded-batch

Evaluate independent states concurrently while limiting parallel requests.
*/
namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task Example_BatchStatesAsync()
    {
        using var client = GetAuthenticatedClient();
        var questions = new Dictionary<string, Question>
        {
            ["spam"] = new NoulQuestion("Is this unsolicited advertising?"),
        };

        var responses = await client.SystemOneManyAsync(
            ["Buy now! Limited offer!", "Can we move our meeting to Tuesday?"],
            questions,
            maxConcurrency: 2);

        responses.Should().HaveCount(2);
        responses.Should().OnlyContain(response => response.Answers.ContainsKey("spam"));
    }
}
