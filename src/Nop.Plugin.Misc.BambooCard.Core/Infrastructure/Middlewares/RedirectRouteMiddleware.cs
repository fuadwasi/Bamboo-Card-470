using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Misc.BambooCard.Core.Infrastructure.Middlewares;
public class RedirectRouteMiddleware
{
    private readonly RequestDelegate _next;

    public RedirectRouteMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase) &&
            context.Request.Path.Equals("/Admin/ProductAttribute/List", StringComparison.InvariantCultureIgnoreCase))
        {
            context.Response.Redirect("/Admin/BambooProductAttribute/List", true);
            return;
        }

        await _next(context);
    }
}
