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

    // The CanvasFrame attribute value every canvas is ordered with: "wood-fsc-2-cm", "-3-cm" or
    // "-4-cm" (see docs/gelato-canvas-catalog.md). CanvasSize prices assume the 2 cm frame - reprice
    // them if this changes.
    public string CanvasFrameUid { get; set; } = "wood-fsc-2-cm";

    // "draft" or "order". Gelato has no test/sandbox API key - this is the only thing standing
    // between a submitted order and a real, charged print run. Draft orders are validated and
    // priced but sit in the Gelato dashboard until confirmed by hand. Defaults to "draft" so only
    // an explicit Gelato__OrderType=order (set for the live deployment) ever produces real orders.
    public string OrderType { get; set; } = "draft";

    // Confirmed against the live Product API (GET {ProductApiBaseUrl}/catalogs/canvas) - see
    // docs/gelato-canvas-catalog.md. CanvasFormat's values are titled "500x700 mm" / "24x30 inches";
    // these two values are what GelatoService.GetSupportedCanvasSizesAsync queries against.
    public string CanvasCatalogUid { get; set; } = "canvas";
    public string CanvasSizeAttributeUid { get; set; } = "CanvasFormat";
}
