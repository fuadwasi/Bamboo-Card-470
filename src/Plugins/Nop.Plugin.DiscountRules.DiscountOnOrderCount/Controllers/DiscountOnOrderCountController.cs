using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.DiscountOnOrderCount.Models;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.DiscountRules.DiscountOnOrderCount.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class DiscountOnOrderCountController : BasePluginController
{
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly IDiscountService _discountService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IPermissionService _permissionService;
    protected readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public DiscountOnOrderCountController(
        IBaseAdminModelFactory baseAdminModelFactory,
        ICustomerService customerService,
        IDiscountService discountService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ISettingService settingService)
    {
        _baseAdminModelFactory = baseAdminModelFactory;
        _customerService = customerService;
        _discountService = discountService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _settingService = settingService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Get errors message from model state
    /// </summary>
    /// <param name="modelState">Model state</param>
    /// <returns>Errors message</returns>
    protected IEnumerable<string> GetErrorsFromModelState(ModelStateDictionary modelState)
    {
        return ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
    }

    #endregion



    #region Methods

    public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return Content("Access denied");

        //load the discount
        var discount = await _discountService.GetDiscountByIdAsync(discountId)
                       ?? throw new ArgumentException("Discount could not be loaded");

        //check whether the discount requirement exists
        if (discountRequirementId.HasValue && await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value) is null)
            return Content("Failed to load requirement.");

        //try to get previously saved restricted customer role identifier
        var orderCountValue = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountOnOrderCountDefaults.OrderCountSettingsKey, discountRequirementId ?? 0));
        var orderStatusIds = await _settingService.GetSettingByKeyAsync<List<int>>(string.Format(DiscountOnOrderCountDefaults.OrderStatusIdsSettingsKey, discountRequirementId ?? 0));

        var model = new RequirementModel
        {
            RequirementId = discountRequirementId ?? 0,
            DiscountId = discountId,
            OrderCount = orderCountValue,
            OrderStatusIds = orderStatusIds ?? new List<int>(),
        };
        await _baseAdminModelFactory.PrepareOrderStatusesAsync(model.AvailableOrderStatuses, withSpecialDefaultItem: false);

        if (model.AvailableOrderStatuses.Any())
        {
            if (model.OrderStatusIds?.Any() ?? false)
            {
                var ids = model.OrderStatusIds.Select(id => id.ToString());
                var statusItems = model.AvailableOrderStatuses.Where(statusItem => ids.Contains(statusItem.Value)).ToList();
                foreach (var statusItem in statusItems)
                {
                    statusItem.Selected = true;
                }
            }
        }


        //set the HTML field prefix
        ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountOnOrderCountDefaults.HtmlFieldPrefix, discountRequirementId ?? 0);

        return View("~/Plugins/DiscountRules.DiscountOnOrderCount/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(RequirementModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return Content("Access denied");

        if (ModelState.IsValid)
        {
            //load the discount
            var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
            if (discount == null)
                return NotFound(new { Errors = new[] { "Discount could not be loaded" } });

            //get the discount requirement
            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId);

            //the discount requirement does not exist, so create a new one
            if (discountRequirement == null)
            {
                discountRequirement = new DiscountRequirement
                {
                    DiscountId = discount.Id,
                    DiscountRequirementRuleSystemName = DiscountOnOrderCountDefaults.SystemName
                };

                await _discountService.InsertDiscountRequirementAsync(discountRequirement);
            }

            //save restricted customer role identifier
            await _settingService.SetSettingAsync(string.Format(DiscountOnOrderCountDefaults.OrderCountSettingsKey, discountRequirement.Id), model.OrderCount);
            await _settingService.SetSettingAsync(string.Format(DiscountOnOrderCountDefaults.OrderStatusIdsSettingsKey, discountRequirement.Id), model.OrderStatusIds.ToList());

            return Ok(new { NewRequirementId = discountRequirement.Id });
        }

        return Ok(new { Errors = GetErrorsFromModelState(ModelState) });
    }

    #endregion
}
