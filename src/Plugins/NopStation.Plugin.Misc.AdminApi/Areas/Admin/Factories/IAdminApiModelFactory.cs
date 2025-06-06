using System.Threading.Tasks;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;

public interface IAdminApiModelFactory
{
    Task<ConfigurationModel> PrepareConfigurationModelAsync();
    Task<AppConfigurationModel> PrepareAppConfigurationModelAsync();
}