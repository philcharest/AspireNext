using Microsoft.AspNetCore.Antiforgery;

namespace AspireNext.Server;

/// <summary>
/// Validates the antiforgery token pair for unsafe HTTP methods. Safe methods (GET/HEAD) pass
/// through untouched, since minimal APIs with JSON bodies don't get antiforgery metadata
/// attached automatically the way form-bound endpoints do - this fills that gap explicitly.
/// </summary>
public static class AntiforgeryFilter
{
    public static async ValueTask<object?> ValidateAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method))
        {
            return await next(context);
        }

        var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest("Missing or invalid CSRF token.");
        }

        return await next(context);
    }
}
