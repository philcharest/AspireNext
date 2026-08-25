using AspireNext.Server.Models;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace AspireNext.Server.Data;

public class StripeService(IOptions<StripeOptions> options)
{
    private readonly SessionService _sessionService = new(new StripeClient(options.Value.SecretKey));
    private readonly RefundService _refundService = new(new StripeClient(options.Value.SecretKey));

    public Task<Session> CreateCheckoutSessionAsync(Order order, string? customerEmail, string frontendBaseUrl)
    {
        var sessionOptions = new SessionCreateOptions
        {
            Mode = "payment",
            PaymentMethodTypes = ["card"],
            ClientReferenceId = order.Id.ToString(),
            CustomerEmail = customerEmail,
            SuccessUrl = $"{frontendBaseUrl}/orders/{order.Id}?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{frontendBaseUrl}/cart?canceled=true",
            LineItems = [.. order.Items.Select(item => new SessionLineItemOptions
            {
                Quantity = item.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmountDecimal = item.Price * 100,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.ProductName,
                    },
                },
            })],
        };

        return _sessionService.CreateAsync(sessionOptions);
    }

    public Task<Refund> RefundAsync(string paymentIntentId, decimal amount) =>
        _refundService.CreateAsync(new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount = (long)(amount * 100),
            Reason = "requested_by_customer",
        });

    public Event ConstructWebhookEvent(string json, string signatureHeader) =>
        // Our account's events are on API version 2023-08-16, older than what Stripe.net 52.2.0
        // expects by default - we only read stable scalar fields (session Id / PaymentIntentId)
        // off the event, so a version mismatch here doesn't risk misreading what we actually use.
        EventUtility.ConstructEvent(json, signatureHeader, options.Value.WebhookSecret, throwOnApiVersionMismatch: false);
}
