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
                        await orderService.MarkOrderPaidAsync(paidOrder, session.PaymentIntentId);
                    break;
                case "checkout.session.expired" or "checkout.session.async_payment_failed":
                    if (await orderService.GetOrderByStripeSessionIdAsync(session.Id) is { Status: OrderStatus.PendingPayment } failedOrder)
                        await orderService.MarkOrderFailedAsync(failedOrder);
                    break;
            }
        }

        return Ok();
    }
}
