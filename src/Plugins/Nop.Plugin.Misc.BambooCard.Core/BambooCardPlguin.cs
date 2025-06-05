using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace Nop.Plugin.Misc.BambooCard.Core;
public class BambooCardPlguin : BasePlugin, IMiscPlugin, INopStationPlugin
{
    #region Fields

    private readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
    private readonly ILocalizationService _localizationService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public BambooCardPlguin(IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        ILocalizationService localizationService,
        IWorkContext workContext)
    {
        _checkoutAttributeService = checkoutAttributeService;
        _localizationService = localizationService;
        _workContext = workContext;
    }

    #endregion

    #region Utilites

    private async Task InstallLanguageResourcesAsync()
    {
        var workingLanguage = await _workContext.GetWorkingLanguageAsync();
        var resources = PluginResouces();
        foreach (var resource in resources)
        {
            var existingResource = await _localizationService.GetResourceAsync(resource.Key, workingLanguage.Id, returnEmptyIfNotFound: true);
            if (string.IsNullOrEmpty(existingResource))
                await _localizationService.AddOrUpdateLocaleResourceAsync(resource.Key, resource.Value);
        }
    }

    private async Task InstallGiftMessageCheckoutAttributeAsync()
    {
        var giftMessageCheckoutAttribute = (await _checkoutAttributeService.GetAllAttributesAsync()).FirstOrDefault(ca => ca.Name.Equals(BambooCardDefaults.GiftMessageCheckoutAttributeName, StringComparison.InvariantCultureIgnoreCase));
        if (giftMessageCheckoutAttribute == null)
        {
            await _checkoutAttributeService.InsertAttributeAsync(new CheckoutAttribute
            {
                Name = "Gift Message",
                IsRequired = true,
                AttributeControlType = AttributeControlType.TextBox,
                DefaultValue = string.Empty,
                DisplayOrder = 1,
                TextPrompt = "Enter your gift message here"
            });
        }
    }


    #endregion

    #region Methods

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Admin.Plugin.Misc.BambooCard.Core.ProductAttributeSearch.Fields.SearchKeyword", "Search Keyword"),
            new KeyValuePair<string, string>("Admin.Plugin.Misc.BambooCard.Core.ProductAttributeSearch.Fields.SearchKeyword.Hint", "Search Keyword to search on product attribute name"),
        };
        return list;
    }

    public override async Task InstallAsync()
    {
        await InstallGiftMessageCheckoutAttributeAsync();
        await this.InstallPluginAsync();
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _localizationService.DeleteLocaleResourcesAsync("Admin.Plugin.Misc.BambooCard.Core.");
        await _localizationService.DeleteLocaleResourcesAsync("Plugin.Misc.BambooCard.Core.");

        await this.UninstallPluginAsync();
        await base.UninstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        await InstallLanguageResourcesAsync();

        await base.UpdateAsync(currentVersion, targetVersion);
    }

    #endregion
}
