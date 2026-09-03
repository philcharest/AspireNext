using AspireNext.Server.Data;
using AspireNext.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireNext.Server.Controllers;

[ApiController]
[Route("api/admin/returns")]
[Authorize(Roles = "Admin")]
[TypeFilter(typeof(AntiforgeryActionFilter))]
public class AdminReturnsController(ReturnService returnService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllReturns() =>
        Ok(await returnService.GetAllReturnsAsync());

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            return Ok(await returnService.ApproveReturnAsync(id));
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

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, RejectReturnRequest request)
    {
        try
        {
            return Ok(await returnService.RejectReturnAsync(id, request.Note));
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
