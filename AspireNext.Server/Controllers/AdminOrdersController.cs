using AspireNext.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireNext.Server.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController(OrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllOrders() =>
        Ok(await orderService.GetAllOrdersAsync());

    // Retries submitting an order to Gelato - useful while FulfillmentStatus.Failed orders pile up
    // because Product.GelatoProductUid mappings or the Gelato account aren't fully set up yet.
    [HttpPost("{id:int}/resubmit-fulfillment")]
    [TypeFilter(typeof(AntiforgeryActionFilter))]
    public async Task<IActionResult> ResubmitFulfillment(int id)
    {
        try
        {
            await orderService.ResubmitFulfillmentAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
