using AspireNext.Server.Data;
using AspireNext.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspireNext.Server.Controllers;

[ApiController]
[Route("webhooks/stripe")]
public class StripeWebhookController(StripeService stripeService, OrderService orderService, ILogger<StripeWebhookController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();

        Stripe.Event stripeEvent;
        try
        {
            stripeEvent = stripeService.ConstructWebhookEvent(json, Request.Headers["Stripe-Signature"]!);
        }
        catch (Stripe.StripeException ex)
        {
            logger.LogWarning(ex, "Stripe webhook signature verification failed.");
            return BadRequest();
        }

        if (stripeEvent.Data.Object is Stripe.Checkout.Session session)
        {
            switch (stripeEvent.Type)
            {
                case "checkout.session.completed" or "checkout.session.async_payment_succeeded":
                    if (await orderService.GetOrderByStripeSessionIdAsync(session.Id) is { Status: OrderStatus.PendingPayment } paidOrder)
                        await orderService.MarkOrderPaidAsync(paidOrder, session.PaymentIntentId, ToShippingAddress(session));
                    break;
                case "checkout.session.expired" or "checkout.session.async_payment_failed":
                    if (await orderService.GetOrderByStripeSessionIdAsync(session.Id) is { Status: OrderStatus.PendingPayment } failedOrder)
                        await orderService.MarkOrderFailedAsync(failedOrder);
                    break;
            }
        }

        return Ok();
    }

    // Our account's events are on API version 2023-08-16 (see StripeService.ConstructWebhookEvent)
    // while this field moved to session.CollectedInformation.ShippingDetails in later Stripe API
    // versions - if addresses come back empty, check which shape your account's events actually use.
    private static ShippingAddress? ToShippingAddress(Stripe.Checkout.Session session)
    {
        var shippingDetails = session.CollectedInformation?.ShippingDetails;
        var address = shippingDetails?.Address;
        if (shippingDetails is null || address is null)
            return null;

        var nameParts = (shippingDetails.Name ?? "").Split(' ', 2);
        return new ShippingAddress
        {
            FirstName = nameParts.ElementAtOrDefault(0) ?? "",
            LastName = nameParts.ElementAtOrDefault(1) ?? "",
            AddressLine1 = address.Line1 ?? "",
            AddressLine2 = address.Line2,
            City = address.City ?? "",
            State = address.State,
            PostCode = address.PostalCode ?? "",
            Country = address.Country ?? "",
            Email = session.CustomerDetails?.Email,
            Phone = session.CustomerDetails?.Phone,
        };
    }
}
