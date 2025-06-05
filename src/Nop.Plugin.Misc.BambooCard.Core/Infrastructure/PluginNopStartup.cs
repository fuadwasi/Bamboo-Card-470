using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.BambooCard.Core.Areas.Admin.Factories;
using Nop.Plugin.Misc.BambooCard.Core.Services;

namespace Nop.Plugin.Misc.BambooCard.Core.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSignalR();
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            services.AddScoped<IBambooProductAttributeService, BambooProductAttributeService>();

            //Admin
            services.AddScoped<IBambooProductAttributeModelFactory, BambooProductAttributeModelFactory>();
        }

        public int Order => int.MaxValue - 500;
    }
}
