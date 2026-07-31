namespace AspireNext.Server;

public static class CartCookie
{
    private const string CookieName = "cartId";
    private static readonly TimeSpan CookieLifetime = TimeSpan.FromDays(30);

    public static string GetOrCreateCartId(HttpContext context)
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
