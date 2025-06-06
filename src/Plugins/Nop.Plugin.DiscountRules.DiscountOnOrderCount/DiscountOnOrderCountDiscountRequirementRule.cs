using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;

namespace Nop.Plugin.DiscountRules.DiscountOnOrderCount;

public class DiscountOnOrderCountDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
{
    #region Fields

    protected readonly IActionContextAccessor _actionContextAccessor;
    protected readonly ICustomerService _customerService;
    protected readonly IDiscountService _discountService;
    protected readonly ILocalizationService _localizationService;
    private readonly IOrderService _orderService;
    protected readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    protected readonly IUrlHelperFactory _urlHelperFactory;
    protected readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public DiscountOnOrderCountDiscountRequirementRule(IActionContextAccessor actionContextAccessor,
        IDiscountService discountService,
        ICustomerService customerService,
        ILocalizationService localizationService,
        IOrderService orderService,
        ISettingService settingService,
        IStoreContext storeContext,
        IUrlHelperFactory urlHelperFactory,
        IWebHelper webHelper,
        IWorkContext workContext)
    {
        _actionContextAccessor = actionContextAccessor;
        _customerService = customerService;
        _discountService = discountService;
        _localizationService = localizationService;
        _orderService = orderService;
        _settingService = settingService;
        _storeContext = storeContext;
        _urlHelperFactory = urlHelperFactory;
        _webHelper = webHelper;
        _workContext = workContext;
    }

    #endregion

    #region Utilites

    private Dictionary<string, string> GetLanguageResources()
    {
        var languageResources = new Dictionary<string, string>()
        {
            ["Plugin.DiscountRules.DiscountOnOrderCount.Fields.OrderCount"] = "Order Count",
            ["Plugin.DiscountRules.DiscountOnOrderCount.Fields.OrderCount.Hint"] = "Enter the minimum number of orders required to apply the discount.",

            ["Plugin.DiscountRules.DiscountOnOrderCount.Fields.OrderStatus"] = "Order Status",
            ["Plugin.DiscountRules.DiscountOnOrderCount.Fields.OrderStatus.Hint"] = "Select the order statuses that are eligible for the discount.",

            ["Plugin.DiscountRules.DiscountOnOrderCount.Fields.DiscountId.Required"] = "A discount must be selected.",
            ["Plugin.DiscountRules.DiscountOnOrderCount.Fields.OrderCount.Invalid"] = "Order count must be at least 1.",
            ["Plugin.DiscountRules.DiscountOnOrderCount.Fields.OrderStatusIds.Invalid"] = "At least one order status must be selected."
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

    private async Task InstallDiscountAsync()
    {
        var discount = new Discount
        {
            Name = "Discount on Order Count",
            DiscountTypeId = (int)DiscountType.AssignedToOrderTotal,
            DiscountPercentage = 10,
            UsePercentage = true,
            RequiresCouponCode = false,
            IsActive = true,
            AdminComment = "This discount applies when a customer has placed a certain number of orders.",
            DiscountAmount = 10,
            IsCumulative = true
        };
        await _discountService.InsertDiscountAsync(discount);

        var discountRequirement = new DiscountRequirement
        {
            DiscountId = discount.Id,
            DiscountRequirementRuleSystemName = DiscountOnOrderCountDefaults.SystemName
        };

        await _discountService.InsertDiscountRequirementAsync(discountRequirement);

        var orderStatusIds = new List<int>()
        {
            (int) OrderStatus.Complete,
            (int) OrderStatus.Pending,
            (int) OrderStatus.Processing,
        };

        //save restricted customer role identifier
        await _settingService.SetSettingAsync(string.Format(DiscountOnOrderCountDefaults.OrderCountSettingsKey, discountRequirement.Id), DiscountOnOrderCountDefaults.DefultNumberOfOrderToPlaceCount);
        await _settingService.SetSettingAsync(string.Format(DiscountOnOrderCountDefaults.OrderStatusIdsSettingsKey, discountRequirement.Id), orderStatusIds);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Check discount requirement
    /// </summary>
    /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Customer);

        //invalid by default
        var result = new DiscountRequirementValidationResult();

        if (request.Customer == null)
            return result;

        //try to get saved restricted customer role identifier
        var discountOnOrderCount = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountOnOrderCountDefaults.OrderCountSettingsKey, request.DiscountRequirementId));
        var orderStatusIds = await _settingService.GetSettingByKeyAsync<List<int>>(string.Format(DiscountOnOrderCountDefaults.OrderStatusIdsSettingsKey, request.DiscountRequirementId));
        if (discountOnOrderCount == 0 || orderStatusIds == null || orderStatusIds.Count() == 0)
            return result;


        var store = await _storeContext.GetCurrentStoreAsync();
        var orders = (await _orderService.SearchOrdersAsync(storeId: store.Id,
            customerId: request.Customer.Id)).Where(o => orderStatusIds.Contains(o.OrderStatusId));

        //result is valid if the customer belongs to the restricted role
        result.IsValid = orders.Count() >= discountOnOrderCount;

        return result;
    }

    /// <summary>
    /// Get URL for rule configuration
    /// </summary>
    /// <param name="discountId">Discount identifier</param>
    /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
    /// <returns>URL</returns>
    public string GetConfigurationUrl(int discountId, int? discountRequirementId)
    {
        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

        return urlHelper.Action("Configure", "DiscountOnOrderCount",
            new { discountId, discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //locales
        await InstallLanguageResourcesAsync();

        await InstallDiscountAsync();

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //discount requirements
        var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
            .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountOnOrderCountDefaults.SystemName);
        foreach (var discountRequirement in discountRequirements)
            await _discountService.DeleteDiscountRequirementAsync(discountRequirement, false);

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugin.DiscountRules.DiscountOnOrderCount.");

        await base.UninstallAsync();
    }

    #endregion
}