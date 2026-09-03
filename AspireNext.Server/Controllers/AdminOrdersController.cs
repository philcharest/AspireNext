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
}
