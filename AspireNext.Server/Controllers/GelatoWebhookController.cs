using AspireNext.Server.Data;
using AspireNext.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspireNext.Server.Controllers;

/// <summary>
/// Receives order/production/shipping status updates Gelato pushes to the webhook URL you
/// configure for your store - https://dashboard.gelato.com/docs/webhooks/
///
/// PLACEHOLDER: Gelato's dashboard lets you attach a custom header to every webhook call it
/// makes, which is what WebhookSecretHeaderName/GelatoOptions.WebhookSecret check below - set a
/// header with that name and a random secret value when you configure the webhook, so this
/// endpoint can reject calls that don't have it. Confirm this is still how it works, and the
/// exact payload shape (event/fulfillmentStatus field names), against the real docs once you have
/// dashboard access - GelatoWebhookPayload's shape and the status mapping here are best-effort.
/// </summary>
[ApiController]
[Route("webhooks/gelato")]
public class GelatoWebhookController(OrderService orderService, IOptions<GelatoOptions> options, ILogger<GelatoWebhookController> logger) : ControllerBase
{
    private const string WebhookSecretHeaderName = "X-Webhook-Secret";

    [HttpPost]
    public async Task<IActionResult> HandleWebhook(GelatoWebhookPayload payload)
    {
        var configuredSecret = options.Value.WebhookSecret;
        if (!string.IsNullOrEmpty(configuredSecret) && Request.Headers[WebhookSecretHeaderName] != configuredSecret)
        {
            logger.LogWarning("Gelato webhook secret mismatch.");
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(payload.FulfillmentStatus))
            return Ok();

        try
        {
            await orderService.UpdateFulfillmentStatusAsync(payload.OrderId, MapFulfillmentStatus(payload.FulfillmentStatus));
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Gelato webhook referenced unknown order {GelatoOrderId}.", payload.OrderId);
        }

        return Ok();
    }

    private static FulfillmentStatus MapFulfillmentStatus(string gelatoStatus) => gelatoStatus.ToLowerInvariant() switch
    {
        var s when s.Contains("ship") => FulfillmentStatus.Shipped,
        var s when s.Contains("deliver") => FulfillmentStatus.Delivered,
        var s when s.Contains("cancel") || s.Contains("fail") => FulfillmentStatus.Failed,
        var s when s.Contains("print") || s.Contains("production") || s.Contains("passed") => FulfillmentStatus.InProduction,
        _ => FulfillmentStatus.Submitted,
    };
}
