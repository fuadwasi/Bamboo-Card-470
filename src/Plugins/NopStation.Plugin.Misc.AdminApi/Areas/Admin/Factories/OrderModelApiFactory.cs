using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;
public class OrderModelApiFactory : IOrderModelApiFactory
{

    #region Fields

    protected readonly AddressSettings _addressSettings;
    protected readonly CatalogSettings _catalogSettings;
    protected readonly CurrencySettings _currencySettings;
    protected readonly IActionContextAccessor _actionContextAccessor;
    protected readonly IAddressModelFactory _addressModelFactory;
    protected readonly IAddressService _addressService;
    protected readonly IAffiliateService _affiliateService;
    protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
    protected readonly ICountryService _countryService;
    protected readonly ICurrencyService _currencyService;
    protected readonly ICustomerService _customerService;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IDiscountService _discountService;
    protected readonly IDownloadService _downloadService;
    protected readonly IEncryptionService _encryptionService;
    protected readonly IGiftCardService _giftCardService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IMeasureService _measureService;
    protected readonly IOrderProcessingService _orderProcessingService;
    protected readonly IOrderReportService _orderReportService;
    protected readonly IOrderService _orderService;
    protected readonly IPaymentPluginManager _paymentPluginManager;
    protected readonly IPaymentService _paymentService;
    protected readonly IPictureService _pictureService;
    protected readonly IPriceCalculationService _priceCalculationService;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly IProductAttributeService _productAttributeService;
    protected readonly IProductService _productService;
    protected readonly IReturnRequestService _returnRequestService;
    protected readonly IRewardPointService _rewardPointService;
    protected readonly ISettingService _settingService;
    protected readonly IShipmentService _shipmentService;
    protected readonly IShippingService _shippingService;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly IStoreService _storeService;
    protected readonly ITaxService _taxService;
    protected readonly IUrlHelperFactory _urlHelperFactory;
    protected readonly IVendorService _vendorService;
    protected readonly IWorkContext _workContext;
    protected readonly MeasureSettings _measureSettings;
    protected readonly NopHttpClient _nopHttpClient;
    protected readonly OrderSettings _orderSettings;
    protected readonly ShippingSettings _shippingSettings;
    protected readonly IUrlRecordService _urlRecordService;
    protected readonly TaxSettings _taxSettings;
    private static readonly char[] _separator = [','];

    #endregion

    #region Ctor

    public OrderModelApiFactory(AddressSettings addressSettings,
        CatalogSettings catalogSettings,
        CurrencySettings currencySettings,
        IActionContextAccessor actionContextAccessor,
        IAddressModelFactory addressModelFactory,
        IAddressService addressService,
        IAffiliateService affiliateService,
        IBaseAdminModelFactory baseAdminModelFactory,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IDiscountService discountService,
        IDownloadService downloadService,
        IEncryptionService encryptionService,
        IGiftCardService giftCardService,
        ILocalizationService localizationService,
        IMeasureService measureService,
        IOrderProcessingService orderProcessingService,
        IOrderReportService orderReportService,
        IOrderService orderService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        IPictureService pictureService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IReturnRequestService returnRequestService,
        IRewardPointService rewardPointService,
        ISettingService settingService,
        IShipmentService shipmentService,
        IShippingService shippingService,
        IStateProvinceService stateProvinceService,
        IStoreService storeService,
        ITaxService taxService,
        IUrlHelperFactory urlHelperFactory,
        IVendorService vendorService,
        IWorkContext workContext,
        MeasureSettings measureSettings,
        NopHttpClient nopHttpClient,
        OrderSettings orderSettings,
        ShippingSettings shippingSettings,
        IUrlRecordService urlRecordService,
        TaxSettings taxSettings)
    {
        _addressSettings = addressSettings;
        _catalogSettings = catalogSettings;
        _currencySettings = currencySettings;
        _actionContextAccessor = actionContextAccessor;
        _addressModelFactory = addressModelFactory;
        _addressService = addressService;
        _affiliateService = affiliateService;
        _baseAdminModelFactory = baseAdminModelFactory;
        _countryService = countryService;
        _currencyService = currencyService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _discountService = discountService;
        _downloadService = downloadService;
        _encryptionService = encryptionService;
        _giftCardService = giftCardService;
        _localizationService = localizationService;
        _measureService = measureService;
        _orderProcessingService = orderProcessingService;
        _orderReportService = orderReportService;
        _orderService = orderService;
        _paymentPluginManager = paymentPluginManager;
        _paymentService = paymentService;
        _pictureService = pictureService;
        _priceCalculationService = priceCalculationService;
        _priceFormatter = priceFormatter;
        _productAttributeService = productAttributeService;
        _productService = productService;
        _returnRequestService = returnRequestService;
        _rewardPointService = rewardPointService;
        _settingService = settingService;
        _shipmentService = shipmentService;
        _shippingService = shippingService;
        _stateProvinceService = stateProvinceService;
        _storeService = storeService;
        _taxService = taxService;
        _urlHelperFactory = urlHelperFactory;
        _vendorService = vendorService;
        _workContext = workContext;
        _measureSettings = measureSettings;
        _nopHttpClient = nopHttpClient;
        _orderSettings = orderSettings;
        _shippingSettings = shippingSettings;
        _urlRecordService = urlRecordService;
        _taxSettings = taxSettings;
    }

    #endregion

    #region Utilites

    #endregion

    #region Methods

    public virtual async Task<OrderSearchApiModel> PrepareCustomerOrderSearchModelAsync(OrderSearchApiModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare grid
        searchModel.SetGridPageSize();

        return await Task.FromResult(searchModel);
    }

    public virtual async Task<OrderListModel> PrepareCustomerOrderListModelAsync(OrderSearchApiModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        var customerId = 0;
        if (!string.IsNullOrEmpty(searchModel.CustomerEmail))
        {
            var customer = await _customerService.GetCustomerByEmailAsync(searchModel.CustomerEmail);
            customerId = customer?.Id ?? 0;
        }

        //get orders
        var orders = await _orderService.SearchOrdersAsync(customerId: customerId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new OrderListModel().PrepareToGridAsync(searchModel, orders, () =>
        {
            //fill in model values from the entity
            return orders.SelectAwait(async order =>
            {
                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                //fill in model values from the entity
                var orderModel = new OrderModel
                {
                    Id = order.Id,
                    OrderStatusId = order.OrderStatusId,
                    PaymentStatusId = order.PaymentStatusId,
                    ShippingStatusId = order.ShippingStatusId,
                    CustomerEmail = billingAddress.Email,
                    CustomerFullName = $"{billingAddress.FirstName} {billingAddress.LastName}",
                    CustomerId = order.CustomerId,
                    CustomOrderNumber = order.CustomOrderNumber
                };

                //convert dates to the user time
                orderModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc);

                //fill in additional values (not existing in the entity)
                orderModel.StoreName = (await _storeService.GetStoreByIdAsync(order.StoreId))?.Name ?? "Deleted";
                orderModel.OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus);
                orderModel.PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
                orderModel.ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus);
                orderModel.OrderTotal = await _priceFormatter.FormatPriceAsync(order.OrderTotal, true, false);

                return orderModel;
            });
        });

        return model;
    }

    #endregion
}