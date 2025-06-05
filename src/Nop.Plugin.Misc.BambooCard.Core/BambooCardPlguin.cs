using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.BambooCard.Core;
public class BambooCardPlguin : BasePlugin, IMiscPlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public BambooCardPlguin(ILocalizationService localizationService,
        IWorkContext workContext)
    {
        _localizationService = localizationService;
        _workContext = workContext;
    }

    #endregion

    #region Utilites


    private Dictionary<string, string> GetLanguageResources()
    {
        var languageResources = new Dictionary<string, string>()
        {
            //Public resource string
            ["Plugin.Misc.CommerceX360.ManufacturerType.Misc"] = "Misc",
        };

        return languageResources;
    }

    private async Task InstallLanguageResourcesAsync()
    {
        var workingLanguage = await _workContext.GetWorkingLanguageAsync();
        var resources = await GetLanguageResources().ToListAsync();
        foreach (var resource in resources)
        {
            var existingResource = await _localizationService.GetResourceAsync(resource.Key, workingLanguage.Id, returnEmptyIfNotFound: true);
            if (string.IsNullOrEmpty(existingResource))
                await _localizationService.AddOrUpdateLocaleResourceAsync(resource.Key, resource.Value);
        }
    }


    #endregion

    #region Methods

    public override async Task InstallAsync()
    {
        await InstallLanguageResourcesAsync();
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await base.UninstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        await InstallLanguageResourcesAsync();

        await base.UpdateAsync(currentVersion, targetVersion);
    }


    #endregion
}
