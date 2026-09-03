using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspireNext.Server.Controllers;

/// <summary>
/// Validates the antiforgery token pair for unsafe HTTP methods. Safe methods (GET/HEAD) pass
/// through untouched, since minimal APIs with JSON bodies don't get antiforgery metadata
/// attached automatically the way form-bound endpoints do - this fills that gap explicitly.
/// </summary>
public class AntiforgeryActionFilter(IAntiforgery antiforgery) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method))
        {
            await next();
            return;
        }

        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new BadRequestObjectResult("Missing or invalid CSRF token.");
            return;
        }

        await next();
    }
}
