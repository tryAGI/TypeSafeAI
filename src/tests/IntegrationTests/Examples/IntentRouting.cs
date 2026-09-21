/*
order: 70
title: Typed intent routing
slug: intent-routing

Route arbitrary state to a strongly typed enum intent.
*/
namespace TypeSafeAI.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public async Task Example_TypedIntentRoutingAsync()
    {
        using var client = GetAuthenticatedClient();
        var router = new TypeSafeIntentRouter<TicketIntent>(client, "Choose the team that should own this ticket.");

        var intent = await router.RouteAsync("I was charged twice for invoice 391.");

        Enum.IsDefined(intent).Should().BeTrue();
    }

    private enum TicketIntent
    {
        [Label("Payments, invoices, refunds, or duplicate charges")] Billing,
        [Label("Technical problems or how-to questions")] Support,
        [Label("Purchasing, upgrades, or product evaluation")] Sales,
    }
}
