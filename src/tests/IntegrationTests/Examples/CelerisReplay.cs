/*
order: 65
title: Celeris explanations and replay
slug: celeris-replay

Read typed Celeris explanations from a saved System One response without making a network request.
*/
namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public void Example_CelerisReplay()
    {
        //// Celeris explanations are keyed by question name. The same typed metadata is available on live responses and QuestionSetResult.Response.
        const string json = """
            {"model":"celeris-1-decision","answers":{"refund":{"type":"noul","noul":0.95}},
             "usage":{"input_tokens":122,"output_tokens":15},
             "x_celeris":{"explanations":{"refund":"The customer explicitly requests a refund."}}}
            """;
        var response = TypeSafeClient.FromHttpResponse(json);
        Console.WriteLine(response.Get<NoulAnswer>("refund").Noul);
        Console.WriteLine(response.Celeris!.Explanations["refund"]);

        response.Celeris.Explanations["refund"].Should().Be("The customer explicitly requests a refund.");
    }
}
