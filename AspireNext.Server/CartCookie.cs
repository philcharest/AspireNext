using System.Security.Claims;

namespace AspireNext.Server;

public static class CartCookie
{
    private const string CookieName = "cartId";
    private static readonly TimeSpan CookieLifetime = TimeSpan.FromDays(30);

    /// <summary>
    /// The cart key to read/write for this request: the authenticated user's cart if
    /// signed in, otherwise the anonymous cart tied to the "cartId" cookie.
    /// </summary>
    public static string ResolveCartKey(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is not null ? $"user:{userId}" : GetAnonymousCartKey(context);
    }

    public static string GetAnonymousCartKey(HttpContext context) => $"anon:{GetOrCreateCartId(context)}";

    private static string GetOrCreateCartId(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(CookieName, out var cartId) && !string.IsNullOrEmpty(cartId))
            return cartId;

        cartId = Guid.NewGuid().ToString("n");
        context.Response.Cookies.Append(CookieName, cartId, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.Add(CookieLifetime),
        });

        return cartId;
    }
}
