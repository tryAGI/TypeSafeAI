/*
order: 60
title: Replay and forward compatibility
slug: replay

Decode cached HTTP bodies and preserve answer variants introduced by future API versions.
*/
namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public void Example_ReplayAndForwardCompatibility()
    {
        const string json = """
            {"model":"jev-next","answers":{"known":{"type":"noul","noul":0.92},"future":{"type":"ranking","items":["a","b"]}},"usage":{"input_tokens":42,"output_tokens":4}}
            """;

        var response = TypeSafeClient.FromHttpResponse(json);

        response.Get<NoulAnswer>("known").IsTrue().Should().BeTrue();
        response.Get<RawAnswer>("future").Discriminator.Should().Be("ranking");
    }
}
