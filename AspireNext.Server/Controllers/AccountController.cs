using System.Security.Claims;
using AspireNext.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspireNext.Server.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController(SignInManager<ApplicationUser> signInManager) : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetCurrentUser() =>
        User.Identity?.IsAuthenticated == true
            ? Ok(new { email = User.FindFirstValue(ClaimTypes.Email), isAdmin = User.IsInRole("Admin") })
            : Unauthorized();

    [HttpPost("logout")]
    [Authorize]
    [TypeFilter(typeof(AntiforgeryActionFilter))]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok();
    }
}
