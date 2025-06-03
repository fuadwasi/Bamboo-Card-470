using FluentValidation;
using Nop.Plugin.DiscountRules.DiscountOnOrderCount.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.DiscountRules.DiscountOnOrderCount.Validators;

/// <summary>
/// Represents an <see cref="RequirementModel"/> validator.
/// </summary>
public class RequirementModelValidator : BaseNopValidator<RequirementModel>
{
    public RequirementModelValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.DiscountId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.DiscountRules.DiscountOnOrderCount.Fields.DiscountId.Required"));
        RuleFor(model => model.OrderCount).GreaterThanOrEqualTo(1)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.DiscountRules.DiscountOnOrderCount.Fields.OrderCount.Invalid"));
        RuleFor(model => model.OrderStatusIds)
            .Must(statusIds => CheckOrderStatusIdshaveValue(statusIds))
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.DiscountRules.DiscountOnOrderCount.Fields.OrderStatusIds.Invalid"));

    }

    private bool CheckOrderStatusIdshaveValue(IEnumerable<int> statusIds)
    {
        // Implement your custom condition logic here
        return statusIds != null && statusIds.Any(); // Example condition
    }
}