namespace AspireNext.Server.Models;

// Request/response shapes for Gelato's Order API v4 (https://dashboard.gelato.com/docs/orders/)
// and Product Catalog API v3 (https://dashboard.gelato.com/docs/products/). Property names are
// PascalCase for C# convention - GelatoService serializes/deserializes these with a
// JsonSerializerOptions using JsonNamingPolicy.CamelCase to match Gelato's camelCase JSON.

public record GelatoOrderRequest(
    string OrderType,
    string OrderReferenceId,
    string CustomerReferenceId,
    string Currency,
    List<GelatoOrderItemRequest> Items,
    GelatoAddress ShippingAddress,
    string ShipmentMethodUid);

public record GelatoOrderItemRequest(
    string ItemReferenceId,
    string ProductUid,
    List<GelatoFile> Files,
    int Quantity);

public record GelatoFile(string Type, string Url);

public record GelatoAddress(
    string FirstName,
    string LastName,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string PostCode,
    string Country,
    string? Email,
    string? Phone);

public record GelatoOrderResponse(
    string Id,
    string OrderReferenceId,
    string OrderType,
    string? FulfillmentStatus,
    string Currency,
    List<GelatoOrderItemResponse>? Items);

public record GelatoOrderItemResponse(
    string Id,
    string ItemReferenceId,
    string ProductUid,
    string? FulfillmentStatus);

/// <summary>
/// Payload Gelato POSTs to the webhook URL configured for your store whenever an order's
/// production/shipping status changes. See GelatoWebhookController and
/// https://dashboard.gelato.com/docs/webhooks/ - PLACEHOLDER: confirm the exact field names and
/// event types against that page once you have dashboard access, this mirrors the documented
/// shape at the time of writing but Gelato may add/rename fields.
/// </summary>
public record GelatoWebhookPayload(
    string Event,
    string OrderId,
    string? OrderReferenceId,
    string? FulfillmentStatus);

public record GelatoCatalogResponse(
    string CatalogUid,
    string? Title,
    List<GelatoProductAttribute>? ProductAttributes);

public record GelatoProductAttribute(
    string ProductAttributeUid,
    string? Title,
    List<GelatoProductAttributeValue> Values);

public record GelatoProductAttributeValue(
    string ProductAttributeValueUid,
    string Title);
