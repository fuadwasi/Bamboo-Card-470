using Nop.Core.Domain.Discounts;
using Nop.Core.Events;
using Nop.Services.Configuration;
using Nop.Services.Events;

namespace Nop.Plugin.DiscountRules.DiscountOnOrderCount.Events;
public class DiscountRequirementEventConsumer : IConsumer<EntityDeletedEvent<DiscountRequirement>>
{
    #region Fields

    protected readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public DiscountRequirementEventConsumer(ISettingService settingService)
    {
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public async Task HandleEventAsync(EntityDeletedEvent<DiscountRequirement> eventMessage)
    {
        var discountRequirement = eventMessage?.Entity;
        if (discountRequirement == null)
            return;

        var setting = await _settingService.GetSettingAsync(string.Format(DiscountOnOrderCountDefaults.OrderCountSettingsKey, discountRequirement.Id));
        if (setting != null)
            await _settingService.DeleteSettingAsync(setting);
    }

    #endregion
}
