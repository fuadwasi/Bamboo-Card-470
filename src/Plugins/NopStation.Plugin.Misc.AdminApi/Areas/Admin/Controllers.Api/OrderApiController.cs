using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/order/[action]")]
public partial class OrderApiController : BaseAdminApiController
{
    #region Fields

    private readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
    private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;
    private readonly IAddressService _addressService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ICustomerService _customerService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IEncryptionService _encryptionService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IExportManager _exportManager;
    private readonly IGiftCardService _giftCardService;
    private readonly IImportManager _importManager;
    private readonly ILocalizationService _localizationService;
    private readonly IOrderModelApiFactory _orderModelApiFactory;
    private readonly IOrderModelFactory _orderModelFactory;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IPdfService _pdfService;
    private readonly IPermissionService _permissionService;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly IProductAttributeParserApi _productAttributeParserApi;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IProductService _productService;
    private readonly IShipmentService _shipmentService;
    private readonly IShippingService _shippingService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly OrderSettings _orderSettings;

    #endregion

    #region Ctor

    public OrderApiController(
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
        IAddressService addressService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IEncryptionService encryptionService,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IGiftCardService giftCardService,
        IImportManager importManager,
        ILocalizationService localizationService,
        IOrderModelApiFactory orderModelApiFactory,
        IOrderModelFactory orderModelFactory,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IPaymentService paymentService,
        IPdfService pdfService,
        IPermissionService permissionService,
        IPriceCalculationService priceCalculationService,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeParserApi productAttributeParserApi,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IShipmentService shipmentService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        IStoreContext storeContext,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        OrderSettings orderSettings)
    {
        _addressAttributeParser = addressAttributeParser;
        _addressAttributeService = addressAttributeService;
        _addressAttributeService = addressAttributeService;
        _addressAttributeParser = addressAttributeParser;
        _addressService = addressService;
        _customerActivityService = customerActivityService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _encryptionService = encryptionService;
        _eventPublisher = eventPublisher;
        _exportManager = exportManager;
        _giftCardService = giftCardService;
        _importManager = importManager;
        _localizationService = localizationService;
        _orderModelApiFactory = orderModelApiFactory;
        _orderModelFactory = orderModelFactory;
        _orderProcessingService = orderProcessingService;
        _orderService = orderService;
        _paymentService = paymentService;
        _pdfService = pdfService;
        _permissionService = permissionService;
        _priceCalculationService = priceCalculationService;
        _productAttributeFormatter = productAttributeFormatter;
        _productAttributeParser = productAttributeParser;
        _productAttributeParserApi = productAttributeParserApi;
        _productAttributeService = productAttributeService;
        _productService = productService;
        _shipmentService = shipmentService;
        _shippingService = shippingService;
        _shoppingCartService = shoppingCartService;
        _storeContext = storeContext;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _orderSettings = orderSettings;
    }

    #endregion

    #region Utilities

    protected virtual async ValueTask<bool> HasAccessToOrderAsync(Order order)
    {
        return order != null && await HasAccessToOrderAsync(order.Id);
    }

    protected virtual async Task<bool> HasAccessToOrderAsync(int orderId)
    {
        if (orderId == 0)
            return false;

        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor == null)
            //not a vendor; has access
            return true;

        var vendorId = currentVendor.Id;
        var hasVendorProducts = (await _orderService.GetOrderItemsAsync(orderId, vendorId: vendorId)).Any();

        return hasVendorProducts;
    }

    protected virtual async ValueTask<bool> HasAccessToProductAsync(OrderItem orderItem)
    {
        if (orderItem == null || orderItem.ProductId == 0)
            return false;

        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor == null)
            //not a vendor; has access
            return true;

        var vendorId = currentVendor.Id;

        return (await _productService.GetProductByIdAsync(orderItem.ProductId))?.VendorId == vendorId;
    }

    protected virtual async ValueTask<bool> HasAccessToShipmentAsync(Shipment shipment)
    {
        ArgumentNullException.ThrowIfNull(shipment);

        if (await _workContext.GetCurrentVendorAsync() is null)
            //not a vendor; has access
            return true;

        return await HasAccessToOrderAsync(shipment.OrderId);
    }

    protected virtual async Task LogEditOrderAsync(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);

        await _customerActivityService.InsertActivityAsync("EditOrder",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
    }

    #endregion

    #region Order list

    public virtual async Task<IActionResult> List(List<int> orderStatuses = null, List<int> paymentStatuses = null, List<int> shippingStatuses = null)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _orderModelFactory.PrepareOrderSearchModelAsync(new OrderSearchModel
        {
            OrderStatusIds = orderStatuses,
            PaymentStatusIds = paymentStatuses,
            ShippingStatusIds = shippingStatuses
        });

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<OrderSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _orderModelFactory.PrepareOrderListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> CustomerOrderList()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _orderModelApiFactory.PrepareCustomerOrderSearchModelAsync(new OrderSearchApiModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CustomerOrderList([FromBody] BaseQueryModel<OrderSearchApiModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _orderModelApiFactory.PrepareCustomerOrderListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ReportAggregates([FromBody] BaseQueryModel<OrderSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _orderModelFactory.PrepareOrderAggregatorModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GoToOrderId([FromBody] BaseQueryModel<OrderSearchModel> queryModel)
    {
        var model = queryModel.Data;
        var order = await _orderService.GetOrderByCustomOrderNumberAsync(model.GoDirectlyToCustomOrderNumber);

        if (order == null)
            return await List();

        return await Edit(order.Id);
    }

    #endregion

    #region Order details

    #region Edit, delete

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null || order.Deleted)
            return NotFound("No order found with the specified id");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() is not null && !await HasAccessToOrderAsync(order))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound("No order found with the specified id");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return AdminApiAccessDenied();

        await _orderProcessingService.DeleteOrderAsync(order);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteOrder",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteOrder"), order.Id), order);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{orderId}")]
    public virtual async Task<IActionResult> PdfInvoice(int orderId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();
        //a vendor should have access only to their orders
        if (!await HasAccessToOrderAsync(orderId))
            return BadRequest("A vendor have access only to their orders");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();

        var order = await _orderService.GetOrderByIdAsync(orderId);

        byte[] bytes;
        await using var stream = new MemoryStream();

        await _pdfService.PrintOrderToPdfAsync(stream, order, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? null : await _workContext.GetWorkingLanguageAsync(), store: null, vendor: currentVendor);
        bytes = stream.ToArray();

        return File(bytes, MimeTypes.ApplicationPdf, $"order_{order.CustomOrderNumber}.pdf");
    }

    [HttpPost]
    public virtual async Task<IActionResult> PdfInvoiceAll([FromBody] BaseQueryModel<OrderSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            model.VendorId = currentVendor.Id;
        }

        var startDateValue = model.StartDate == null ? null
                        : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

        var endDateValue = model.EndDate == null ? null
                        : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        var orderStatusIds = model.OrderStatusIds != null && !model.OrderStatusIds.Contains(0)
            ? model.OrderStatusIds.ToList()
            : null;
        var paymentStatusIds = model.PaymentStatusIds != null && !model.PaymentStatusIds.Contains(0)
            ? model.PaymentStatusIds.ToList()
            : null;
        var shippingStatusIds = model.ShippingStatusIds != null && !model.ShippingStatusIds.Contains(0)
            ? model.ShippingStatusIds.ToList()
            : null;

        var filterByProductId = 0;
        var product = await _productService.GetProductByIdAsync(model.ProductId);
        if (product != null && (currentVendor == null || product.VendorId == currentVendor.Id))
            filterByProductId = model.ProductId;

        //load orders
        var orders = await _orderService.SearchOrdersAsync(storeId: model.StoreId,
            vendorId: model.VendorId,
            productId: filterByProductId,
            warehouseId: model.WarehouseId,
            paymentMethodSystemName: model.PaymentMethodSystemName,
            createdFromUtc: startDateValue,
            createdToUtc: endDateValue,
            osIds: orderStatusIds,
            psIds: paymentStatusIds,
            ssIds: shippingStatusIds,
            billingPhone: model.BillingPhone,
            billingEmail: model.BillingEmail,
            billingLastName: model.BillingLastName,
            billingCountryId: model.BillingCountryId,
            orderNotes: model.OrderNotes);

        //ensure that we at least one order selected
        if (orders.Count == 0)
        {
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Orders.NoOrders"));
        }

        try
        {
            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _pdfService.PrintOrdersToPdfAsync(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? null : await _workContext.GetWorkingLanguageAsync(), currentVendor);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationZip, "orders.zip");
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    public virtual async Task<IActionResult> PdfInvoiceSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var orders = new List<Order>();
        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return NotFound();

        if (selectedIds != null)
        {
            var ids = selectedIds
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            orders.AddRange(await _orderService.GetOrdersByIdsAsync(ids));
        }

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            orders = await orders.WhereAwait(HasAccessToOrderAsync).ToListAsync();
        }

        try
        {
            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _pdfService.PrintOrdersToPdfAsync(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? null : await _workContext.GetWorkingLanguageAsync(), currentVendor);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationZip, "orders.zip");
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    //currently we use this method on the add product to order details pages
    [HttpPost("{productId}/{validateAttributeConditions}")]
    public virtual async Task<IActionResult> ProductDetails_AttributeChange(int productId, bool validateAttributeConditions, [FromBody] BaseQueryModel<string> queryModel)
    {
        var form = queryModel.FormValues.ToNameValueCollection();
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound("No product found with the specified id");

        var errors = new List<string>();
        var attributeXml = await _productAttributeParserApi.ParseProductAttributesAsync(product, form, errors);

        //conditional attributes
        var enabledAttributeMappingIds = new List<int>();
        var disabledAttributeMappingIds = new List<int>();
        if (validateAttributeConditions)
        {
            var attributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in attributes)
            {
                var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributeXml);
                if (!conditionMet.HasValue)
                    continue;

                if (conditionMet.Value)
                    enabledAttributeMappingIds.Add(attribute.Id);
                else
                    disabledAttributeMappingIds.Add(attribute.Id);
            }
        }

        return Ok(new
        {
            EnabledAttributeMappingIds = enabledAttributeMappingIds.ToArray(),
            DisabledAttributeMappingIds = disabledAttributeMappingIds.ToArray(),
            Message = errors.Count != 0 ? errors.ToArray() : null
        }
        .ToGenericResponse<object>());
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> EditCreditCardInfo(int id, [FromBody] BaseQueryModel<OrderModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound("No order found with the specified id");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return AdminApiAccessDenied();

        if (order.AllowStoringCreditCardNumber)
        {
            var cardType = model.CardType;
            var cardName = model.CardName;
            var cardNumber = model.CardNumber;
            var cardCvv2 = model.CardCvv2;
            var cardExpirationMonth = model.CardExpirationMonth;
            var cardExpirationYear = model.CardExpirationYear;

            order.CardType = _encryptionService.EncryptText(cardType);
            order.CardName = _encryptionService.EncryptText(cardName);
            order.CardNumber = _encryptionService.EncryptText(cardNumber);
            order.MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(cardNumber));
            order.CardCvv2 = _encryptionService.EncryptText(cardCvv2);
            order.CardExpirationMonth = _encryptionService.EncryptText(cardExpirationMonth);
            order.CardExpirationYear = _encryptionService.EncryptText(cardExpirationYear);
            await _orderService.UpdateOrderAsync(order);
        }

        //add a note
        await _orderService.InsertOrderNoteAsync(new OrderNote
        {
            OrderId = order.Id,
            Note = "Credit card info has been edited",
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow
        });

        await LogEditOrderAsync(order.Id);

        //prepare model
        model = await _orderModelFactory.PrepareOrderModelAsync(model, order);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> EditOrderTotals(int id, [FromBody] BaseQueryModel<OrderModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound("No order found with the specified id");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return AdminApiAccessDenied();

        order.OrderSubtotalInclTax = model.OrderSubtotalInclTaxValue;
        order.OrderSubtotalExclTax = model.OrderSubtotalExclTaxValue;
        order.OrderSubTotalDiscountInclTax = model.OrderSubTotalDiscountInclTaxValue;
        order.OrderSubTotalDiscountExclTax = model.OrderSubTotalDiscountExclTaxValue;
        order.OrderShippingInclTax = model.OrderShippingInclTaxValue;
        order.OrderShippingExclTax = model.OrderShippingExclTaxValue;
        order.PaymentMethodAdditionalFeeInclTax = model.PaymentMethodAdditionalFeeInclTaxValue;
        order.PaymentMethodAdditionalFeeExclTax = model.PaymentMethodAdditionalFeeExclTaxValue;
        order.TaxRates = model.TaxRatesValue;
        order.OrderTax = model.TaxValue;
        order.OrderDiscount = model.OrderTotalDiscountValue;
        order.OrderTotal = model.OrderTotalValue;
        await _orderService.UpdateOrderAsync(order);

        //add a note
        await _orderService.InsertOrderNoteAsync(new OrderNote
        {
            OrderId = order.Id,
            Note = "Order totals have been edited",
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow
        });

        await LogEditOrderAsync(order.Id);

        //prepare model
        model = await _orderModelFactory.PrepareOrderModelAsync(model, order);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> EditShippingMethod(int id, [FromBody] BaseQueryModel<OrderModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound("No order found with the specified id");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return AdminApiAccessDenied();

        order.ShippingMethod = model.ShippingMethod;
        await _orderService.UpdateOrderAsync(order);

        //add a note
        await _orderService.InsertOrderNoteAsync(new OrderNote
        {
            OrderId = order.Id,
            Note = "Shipping method has been edited",
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow
        });

        await LogEditOrderAsync(order.Id);

        //prepare model
        model = await _orderModelFactory.PrepareOrderModelAsync(model, order);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> EditOrderItem(int id, [FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var form = queryModel.FormValues.ToNameValueCollection();
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound("No order found with the specified id");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return AdminApiAccessDenied();

        //get order item identifier
        var orderItemId = 0;
        foreach (string formValue in form.Keys)
            if (formValue.StartsWith("btnSaveOrderItem", StringComparison.InvariantCultureIgnoreCase))
                orderItemId = Convert.ToInt32(formValue["btnSaveOrderItem".Length..]);

        var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
            ?? throw new ArgumentException("No order item found with the specified id");

        if (!decimal.TryParse(form["pvUnitPriceInclTax" + orderItemId], out var unitPriceInclTax))
            unitPriceInclTax = orderItem.UnitPriceInclTax;
        if (!decimal.TryParse(form["pvUnitPriceExclTax" + orderItemId], out var unitPriceExclTax))
            unitPriceExclTax = orderItem.UnitPriceExclTax;
        if (!int.TryParse(form["pvQuantity" + orderItemId], out var quantity))
            quantity = orderItem.Quantity;
        if (!decimal.TryParse(form["pvDiscountInclTax" + orderItemId], out var discountInclTax))
            discountInclTax = orderItem.DiscountAmountInclTax;
        if (!decimal.TryParse(form["pvDiscountExclTax" + orderItemId], out var discountExclTax))
            discountExclTax = orderItem.DiscountAmountExclTax;
        if (!decimal.TryParse(form["pvPriceInclTax" + orderItemId], out var priceInclTax))
            priceInclTax = orderItem.PriceInclTax;
        if (!decimal.TryParse(form["pvPriceExclTax" + orderItemId], out var priceExclTax))
            priceExclTax = orderItem.PriceExclTax;

        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

        if (quantity > 0)
        {
            var qtyDifference = orderItem.Quantity - quantity;

            if (!_orderSettings.AutoUpdateOrderTotalsOnEditingOrder)
            {
                orderItem.UnitPriceInclTax = unitPriceInclTax;
                orderItem.UnitPriceExclTax = unitPriceExclTax;
                orderItem.Quantity = quantity;
                orderItem.DiscountAmountInclTax = discountInclTax;
                orderItem.DiscountAmountExclTax = discountExclTax;
                orderItem.PriceInclTax = priceInclTax;
                orderItem.PriceExclTax = priceExclTax;
                await _orderService.UpdateOrderItemAsync(orderItem);
            }

            //adjust inventory
            await _productService.AdjustInventoryAsync(product, qtyDifference, orderItem.AttributesXml,
                string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditOrder"), order.Id));
        }
        else
        {
            //adjust inventory
            await _productService.AdjustInventoryAsync(product, orderItem.Quantity, orderItem.AttributesXml,
                string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.DeleteOrderItem"), order.Id));

            //delete item
            await _orderService.DeleteOrderItemAsync(orderItem);
        }

        //update order totals
        var updateOrderParameters = new UpdateOrderParameters(order, orderItem)
        {
            PriceInclTax = unitPriceInclTax,
            PriceExclTax = unitPriceExclTax,
            DiscountAmountInclTax = discountInclTax,
            DiscountAmountExclTax = discountExclTax,
            SubTotalInclTax = priceInclTax,
            SubTotalExclTax = priceExclTax,
            Quantity = quantity
        };
        await _orderProcessingService.UpdateOrderTotalsAsync(updateOrderParameters);

        //add a note
        await _orderService.InsertOrderNoteAsync(new OrderNote
        {
            OrderId = order.Id,
            Note = "Order item has been edited",
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow
        });

        await LogEditOrderAsync(order.Id);

        //prepare model
        var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

        return OkWrap(model, errors: updateOrderParameters.Warnings);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DeleteOrderItem(int id, [FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var form = queryModel.FormValues.ToNameValueCollection();
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound("No order found with the specified id");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return AdminApiAccessDenied();

        //get order item identifier
        var orderItemId = 0;
        foreach (string formValue in form.Keys)
            if (formValue.StartsWith("btnDeleteOrderItem", StringComparison.InvariantCultureIgnoreCase))
                orderItemId = Convert.ToInt32(formValue["btnDeleteOrderItem".Length..]);

        var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
            ?? throw new ArgumentException("No order item found with the specified id");

        if ((await _giftCardService.GetGiftCardsByPurchasedWithOrderItemIdAsync(orderItem.Id)).Any())
        {
            //we cannot delete an order item with associated gift cards
            //a store owner should delete them first

            //prepare model
            var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

            return BadRequestWrap(model, errors: new List<string> { await _localizationService.GetResourceAsync("Admin.Orders.OrderItem.DeleteAssociatedGiftCardRecordError") });
        }
        else
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            //adjust inventory
            await _productService.AdjustInventoryAsync(product, orderItem.Quantity, orderItem.AttributesXml,
                string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.DeleteOrderItem"), order.Id));

            //delete item
            await _orderService.DeleteOrderItemAsync(orderItem);

            //update order totals
            var updateOrderParameters = new UpdateOrderParameters(order, orderItem);
            await _orderProcessingService.UpdateOrderTotalsAsync(updateOrderParameters);

            //add a note
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = "Order item has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            await LogEditOrderAsync(order.Id);

            //prepare model
            var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

            return OkWrap(model, errors: updateOrderParameters.Warnings);
        }
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ResetDownloadCount(int id, [FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var form = queryModel.FormValues.ToNameValueCollection();
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound("No order found with the specified id");

        //get order item identifier
        var orderItemId = 0;
        foreach (string formValue in form.Keys)
            if (formValue.StartsWith("btnResetDownloadCount", StringComparison.InvariantCultureIgnoreCase))
                orderItemId = Convert.ToInt32(formValue["btnResetDownloadCount".Length..]);

        var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);
        if (orderItem == null)
            return NotFound("No order item found with the specified id");

        //ensure a vendor has access only to his products 
        if (await _workContext.GetCurrentVendorAsync() is not null && !await HasAccessToProductAsync(orderItem))
            return AdminApiAccessDenied();

        orderItem.DownloadCount = 0;
        await _orderService.UpdateOrderItemAsync(orderItem);
        await LogEditOrderAsync(order.Id);

        //prepare model
        var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ActivateDownloadItem(int id, [FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var form = queryModel.FormValues.ToNameValueCollection();
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound("No order found with the specified id");

        //get order item identifier
        var orderItemId = 0;
        foreach (string formValue in form.Keys)
            if (formValue.StartsWith("btnPvActivateDownload", StringComparison.InvariantCultureIgnoreCase))
                orderItemId = Convert.ToInt32(formValue["btnPvActivateDownload".Length..]);

        var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);
        if (orderItem == null)
            return NotFound("No order item found with the specified id");

        //ensure a vendor has access only to his products 
        if (await _workContext.GetCurrentVendorAsync() is not null && !await HasAccessToProductAsync(orderItem))
            return AdminApiAccessDenied();

        orderItem.IsDownloadActivated = !orderItem.IsDownloadActivated;
        await _orderService.UpdateOrderItemAsync(orderItem);

        await LogEditOrderAsync(order.Id);

        //prepare model
        var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

        return OkWrap(model);
    }

    [HttpGet("{id}/{orderItemId}")]
    public virtual async Task<IActionResult> UploadLicenseFilePopup(int id, int orderItemId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound("No order found with the specified id");

        //try to get an order item with the specified id
        var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);
        if (orderItem == null)
            return NotFound("No order item found with the specified id");

        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        if (!product.IsDownload)
            return BadRequest("Product is not downloadable");

        //ensure a vendor has access only to his products 
        if (await _workContext.GetCurrentVendorAsync() is not null && !await HasAccessToProductAsync(orderItem))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _orderModelFactory.PrepareUploadLicenseModelAsync(new UploadLicenseModel(), order, orderItem);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> UploadLicenseFilePopup([FromBody] BaseQueryModel<UploadLicenseModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(model.OrderId);
        if (order == null)
            return NotFound("No order found with the specified id");

        var orderItem = await _orderService.GetOrderItemByIdAsync(model.OrderItemId);
        if (orderItem == null)
            return NotFound("No order item found with the specified id");

        //ensure a vendor has access only to his products 
        if (await _workContext.GetCurrentVendorAsync() is not null && !await HasAccessToProductAsync(orderItem))
            return AdminApiAccessDenied();

        //attach license
        if (model.LicenseDownloadId > 0)
            orderItem.LicenseDownloadId = model.LicenseDownloadId;
        else
            orderItem.LicenseDownloadId = null;

        await _orderService.UpdateOrderItemAsync(orderItem);

        await LogEditOrderAsync(order.Id);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteLicenseFilePopup([FromBody] BaseQueryModel<UploadLicenseModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(model.OrderId);
        if (order == null)
            return NotFound("No order found with the specified id");

        var orderItem = await _orderService.GetOrderItemByIdAsync(model.OrderItemId);
        if (orderItem == null)
            return NotFound("No order item found with the specified id");

        //ensure a vendor has access only to his products 
        if (await _workContext.GetCurrentVendorAsync() is not null && !await HasAccessToProductAsync(orderItem))
            return AdminApiAccessDenied();

        //attach license
        orderItem.LicenseDownloadId = null;

        await _orderService.UpdateOrderItemAsync(orderItem);

        await LogEditOrderAsync(order.Id);

        return OkWrap(model);
    }

    [HttpGet("{orderId}")]
    public virtual async Task<IActionResult> AddProductToOrder(int orderId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound("No order found with the specified id");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _orderModelFactory.PrepareAddProductToOrderSearchModelAsync(new AddProductToOrderSearchModel(), order);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AddProductToOrder([FromBody] BaseQueryModel<AddProductToOrderSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(searchModel.OrderId);
        if (order == null)
            return NotFound("No order found with the specified id");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _orderModelFactory.PrepareAddProductToOrderListModelAsync(searchModel, order);

        return OkWrap(model);
    }

    [HttpGet("{orderId}/{productId}")]
    public virtual async Task<IActionResult> AddProductToOrderDetails(int orderId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound("No order found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _orderModelFactory.PrepareAddProductToOrderModelAsync(new AddProductToOrderModel(), order, product);

        return OkWrap(model);
    }

    [HttpPost("{orderId}/{productId}")]
    public virtual async Task<IActionResult> AddProductToOrderDetails(int orderId, int productId, [FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var form = queryModel.FormValues.ToNameValueCollection();
        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return AdminApiAccessDenied();

        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound("No order found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //basic properties
        _ = decimal.TryParse(form["UnitPriceInclTax"], out var unitPriceInclTax);
        _ = decimal.TryParse(form["UnitPriceExclTax"], out var unitPriceExclTax);
        _ = int.TryParse(form["Quantity"], out var quantity);
        _ = decimal.TryParse(form["SubTotalInclTax"], out var priceInclTax);
        _ = decimal.TryParse(form["SubTotalExclTax"], out var priceExclTax);

        //warnings
        var warnings = new List<string>();

        //attributes
        var attributesXml = await _productAttributeParserApi.ParseProductAttributesAsync(product, form, warnings);

        //rental product
        _productAttributeParserApi.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);

        //warnings
        warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(customer, ShoppingCartType.ShoppingCart, product, quantity, attributesXml));
        warnings.AddRange(await _shoppingCartService.GetShoppingCartItemGiftCardWarningsAsync(ShoppingCartType.ShoppingCart, product, attributesXml));
        warnings.AddRange(await _shoppingCartService.GetRentalProductWarningsAsync(product, rentalStartDate, rentalEndDate));
        if (warnings.Count == 0)
        {
            //no errors
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            //attributes
            var attributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product, attributesXml, customer, currentStore);

            //weight
            var itemWeight = await _shippingService.GetShoppingCartItemWeightAsync(product, attributesXml);

            //save item
            var orderItem = new OrderItem
            {
                OrderItemGuid = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                UnitPriceInclTax = unitPriceInclTax,
                UnitPriceExclTax = unitPriceExclTax,
                PriceInclTax = priceInclTax,
                PriceExclTax = priceExclTax,
                OriginalProductCost = await _priceCalculationService.GetProductCostAsync(product, attributesXml),
                AttributeDescription = attributeDescription,
                AttributesXml = attributesXml,
                Quantity = quantity,
                DiscountAmountInclTax = decimal.Zero,
                DiscountAmountExclTax = decimal.Zero,
                DownloadCount = 0,
                IsDownloadActivated = false,
                LicenseDownloadId = 0,
                ItemWeight = itemWeight,
                RentalStartDateUtc = rentalStartDate,
                RentalEndDateUtc = rentalEndDate
            };

            await _orderService.InsertOrderItemAsync(orderItem);

            //adjust inventory
            await _productService.AdjustInventoryAsync(product, -orderItem.Quantity, orderItem.AttributesXml,
                string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditOrder"), order.Id));

            //update order totals
            var updateOrderParameters = new UpdateOrderParameters(order, orderItem)
            {
                PriceInclTax = unitPriceInclTax,
                PriceExclTax = unitPriceExclTax,
                SubTotalInclTax = priceInclTax,
                SubTotalExclTax = priceExclTax,
                Quantity = quantity
            };
            await _orderProcessingService.UpdateOrderTotalsAsync(updateOrderParameters);

            //add a note
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = "A new order item has been added",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            await LogEditOrderAsync(order.Id);

            //gift cards
            if (product.IsGiftCard)
            {
                _productAttributeParser.GetGiftCardAttribute(
                    attributesXml, out var recipientName, out var recipientEmail, out var senderName, out var senderEmail, out var giftCardMessage);

                for (var i = 0; i < orderItem.Quantity; i++)
                {
                    var gc = new GiftCard
                    {
                        GiftCardType = product.GiftCardType,
                        PurchasedWithOrderItemId = orderItem.Id,
                        Amount = unitPriceExclTax,
                        IsGiftCardActivated = false,
                        GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                        RecipientName = recipientName,
                        RecipientEmail = recipientEmail,
                        SenderName = senderName,
                        SenderEmail = senderEmail,
                        Message = giftCardMessage,
                        IsRecipientNotified = false,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    await _giftCardService.InsertGiftCardAsync(gc);
                }
            }

            return Ok(errors: updateOrderParameters.Warnings);
        }

        //prepare model
        var model = await _orderModelFactory.PrepareAddProductToOrderModelAsync(new AddProductToOrderModel(), order, product);
        model.Warnings.AddRange(warnings);

        return BadRequestWrap(model, errors: model.Warnings);
    }

    #endregion

    #endregion
}
