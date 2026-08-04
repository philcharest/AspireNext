using AspireNext.Server.Models;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace AspireNext.Server.Data;

public class StripeService(IOptions<StripeOptions> options)
{
    private readonly SessionService _sessionService = new(new StripeClient(options.Value.SecretKey));

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

    public Event ConstructWebhookEvent(string json, string signatureHeader) =>
        EventUtility.ConstructEvent(json, signatureHeader, options.Value.WebhookSecret);
}
