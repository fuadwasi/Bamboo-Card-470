using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Actions.Infrastructure;

namespace Nop.Plugin.Misc.BambooCard.Core.Infrastructure;
public class MiddlewareNopStartup : INopStartup
{
    public int Order => new ErrorHandlerStartup().Order + 1;

    public void Configure(IApplicationBuilder application)
    {
        application.UseMiddleware<RedirectRouteMiddleware>();

    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}
