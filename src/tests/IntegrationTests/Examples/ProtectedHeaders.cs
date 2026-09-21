/*
order: 100
title: Protected headers
slug: protected-headers

Credential and payload headers cannot be accidentally replaced by caller options.
*/
namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public void Example_ProtectedHeadersCannotBeOverridden()
    {
        var options = new TypeSafeClientOptions();
        options.DefaultHeaders["Authorization"] = "Bearer something-else";

        var action = () => new TypeSafeClient("safe-placeholder", options: options);

        action.Should().Throw<ArgumentException>();
    }
}
