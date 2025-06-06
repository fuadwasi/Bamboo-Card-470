using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;

public class AdminApiModelFactory : IAdminApiModelFactory
{
    #region Fields

    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IPictureService _pictureService;

    #endregion

    #region Ctor

    public AdminApiModelFactory(ISettingService settingService,
        IStoreContext storeContext,
        IPictureService pictureService)
    {
        _settingService = settingService;
        _storeContext = storeContext;
        _pictureService = pictureService;
    }

    #endregion

    #region Methods

    public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var webApiSettings = await _settingService.LoadSettingAsync<AdminApiSettings>(storeScope);

        var model = webApiSettings.ToSettingsModel<ConfigurationModel>();
        model.ActiveStoreScopeConfiguration = storeScope;

        if (storeScope == 0)
            return model;

        model.CheckIat_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.CheckIat, storeScope);
        model.EnableJwtSecurity_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.EnableJwtSecurity, storeScope);
        model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.SecretKey, storeScope);
        model.TokenKey_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.TokenKey, storeScope);
        model.TokenSecondsValid_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.TokenSecondsValid, storeScope);
        model.TokenSecret_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.TokenSecret, storeScope);
        model.AndroidVersion_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.AndroidVersion, storeScope);
        model.AndriodForceUpdate_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.AndriodForceUpdate, storeScope);
        model.PlayStoreUrl_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.PlayStoreUrl, storeScope);
        model.IOSVersion_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.IOSVersion, storeScope);
        model.IOSForceUpdate_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.IOSForceUpdate, storeScope);
        model.AppStoreUrl_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.AppStoreUrl, storeScope);
        model.LogoId_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.LogoId, storeScope);
        model.LogoSize_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.LogoSize, storeScope);
        model.ShowChangeBaseUrlPanel_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.ShowChangeBaseUrlPanel, storeScope);

        return model;
    }

    public async Task<AppConfigurationModel> PrepareAppConfigurationModelAsync()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var adminApiSettings = await _settingService.LoadSettingAsync<AdminApiSettings>(storeScope);

        var appConfigurationModel = new AppConfigurationModel
        {
            AndriodForceUpdate = adminApiSettings.AndriodForceUpdate,
            AndroidVersion = adminApiSettings.AndroidVersion,
            PlayStoreUrl = adminApiSettings.PlayStoreUrl,
            IOSForceUpdate = adminApiSettings.IOSForceUpdate,
            IOSVersion = adminApiSettings.IOSVersion,
            AppStoreUrl = adminApiSettings.AppStoreUrl,
            LogoUrl = await _pictureService.GetPictureUrlAsync(adminApiSettings.LogoId, adminApiSettings.LogoSize)
        };

        return appConfigurationModel;
    }

    #endregion
}
