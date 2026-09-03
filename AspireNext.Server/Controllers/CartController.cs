using AspireNext.Server.Data;
using AspireNext.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireNext.Server.Controllers;

[ApiController]
[Route("api/cart")]
[TypeFilter(typeof(AntiforgeryActionFilter))]
public class CartController(CartService cartService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCart() =>
        Ok(await cartService.GetCartAsync(CartCookie.ResolveCartKey(HttpContext)));

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest request)
    {
        try
        {
            return Ok(await cartService.AddItemAsync(CartCookie.ResolveCartKey(HttpContext), request.ProductId, request.Quantity));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> UpdateItem(int productId, UpdateCartItemRequest request) =>
        Ok(await cartService.UpdateItemAsync(CartCookie.ResolveCartKey(HttpContext), productId, request.Quantity));

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId) =>
        Ok(await cartService.RemoveItemAsync(CartCookie.ResolveCartKey(HttpContext), productId));

    [HttpPost("merge")]
    [Authorize]
    public async Task<IActionResult> MergeCarts() =>
        Ok(await cartService.MergeCartsAsync(CartCookie.GetAnonymousCartKey(HttpContext), CartCookie.ResolveCartKey(HttpContext)));
}
