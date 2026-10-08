namespace AspireNext.Server.Models;

/// <summary>
/// Gelato (https://www.gelato.com) print-on-demand API configuration. Bound from the "Gelato"
/// config section the same way <see cref="StripeOptions"/> is bound from "Stripe" - see
/// AppHost.cs for how these get wired in as environment variables per environment.
/// </summary>
public class GelatoOptions
{
    // PLACEHOLDER - API key from your Gelato dashboard once the account exists:
    // https://dashboard.gelato.com/account/api-keys
    public string ApiKey { get; set; } = "";

    public string OrderApiBaseUrl { get; set; } = "https://order.gelatoapis.com/v4";
    public string ProductApiBaseUrl { get; set; } = "https://product.gelatoapis.com/v3";

    // PLACEHOLDER - the signing secret for the webhook endpoint you configure in the Gelato
    // dashboard to receive order/production status updates (see GelatoWebhookController).
    public string WebhookSecret { get; set; } = "";

    // "normal" or "express" - see https://dashboard.gelato.com/docs/orders/shipment-methods/
    public string DefaultShipmentMethodUid { get; set; } = "normal";

    // PLACEHOLDER - confirm the real catalog/attribute uids once you have API access
    // (GET {ProductApiBaseUrl}/catalogs lists every catalogUid; GET {ProductApiBaseUrl}/catalogs/{catalogUid}
    // returns that catalog's productAttributes, one of which enumerates its sizes). These two
    // values are what GelatoService.GetSupportedCanvasSizesAsync queries against.
    public string CanvasCatalogUid { get; set; } = "canvas";
    public string CanvasSizeAttributeUid { get; set; } = "CanvasSize";
}
