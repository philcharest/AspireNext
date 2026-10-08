namespace AspireNext.Server.Models;

/// <summary>
/// Recipient address for an order, captured from Stripe Checkout's shipping address collection
/// and sent on to Gelato as the print fulfillment destination. Stored as an EF Core owned type on
/// <see cref="Order"/> rather than a separate table, since it only ever belongs to one order.
/// </summary>
public class ShippingAddress
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public string? State { get; set; }
    public required string PostCode { get; set; }
    public required string Country { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
