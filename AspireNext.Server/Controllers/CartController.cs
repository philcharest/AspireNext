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
            return Ok(await cartService.AddItemAsync(CartCookie.ResolveCartKey(HttpContext), request.ProductId, request.SizeId, request.Quantity));
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

    [HttpPut("items/{productId:int}/{sizeId:int}")]
    public async Task<IActionResult> UpdateItem(int productId, int sizeId, UpdateCartItemRequest request) =>
        Ok(await cartService.UpdateItemAsync(CartCookie.ResolveCartKey(HttpContext), productId, sizeId, request.Quantity));

    [HttpDelete("items/{productId:int}/{sizeId:int}")]
    public async Task<IActionResult> RemoveItem(int productId, int sizeId) =>
        Ok(await cartService.RemoveItemAsync(CartCookie.ResolveCartKey(HttpContext), productId, sizeId));

    [HttpPut("currency")]
    public async Task<IActionResult> SetCurrency(UpdateCartCurrencyRequest request)
    {
        try
        {
            return Ok(await cartService.SetCurrencyAsync(CartCookie.ResolveCartKey(HttpContext), request.Currency));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("merge")]
    [Authorize]
    public async Task<IActionResult> MergeCarts() =>
        Ok(await cartService.MergeCartsAsync(CartCookie.GetAnonymousCartKey(HttpContext), CartCookie.ResolveCartKey(HttpContext)));
}
