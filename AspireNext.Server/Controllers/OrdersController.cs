using System.Security.Claims;
using AspireNext.Server.Data;
using AspireNext.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireNext.Server.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class OrdersController(OrderService orderService, ReturnService returnService, StripeService stripeService, IConfiguration configuration) : ControllerBase
{
    [HttpPost("checkout")]
    [TypeFilter(typeof(AntiforgeryActionFilter))]
    public async Task<IActionResult> Checkout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var order = await orderService.CreatePendingOrderAsync(userId, CartCookie.ResolveCartKey(HttpContext));

            var frontendBaseUrl =
                configuration["services:frontend:http:0"] ??
                configuration["services:frontend:https:0"] ??
                throw new InvalidOperationException("Frontend base URL is not configured.");

            var session = await stripeService.CreateCheckoutSessionAsync(order, User.FindFirstValue(ClaimTypes.Email), frontendBaseUrl);
            await orderService.SetStripeSessionIdAsync(order.Id, session.Id);

            return Ok(new { checkoutUrl = session.Url });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders() =>
        Ok(await orderService.GetOrdersAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!));

    [HttpGet("orders/{id:int}")]
    public async Task<IActionResult> GetOrderById(int id) =>
        await orderService.GetOrderAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, id)
            is OrderDto order
            ? Ok(order)
            : NotFound();

    [HttpPost("orders/{id:int}/returns")]
    [TypeFilter(typeof(AntiforgeryActionFilter))]
    public async Task<IActionResult> CreateReturnRequest(int id, CreateReturnRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            return Ok(await returnService.CreateReturnRequestAsync(userId, id, request));
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
