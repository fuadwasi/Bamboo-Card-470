using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Orders;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;

public interface IOrderModelApiFactory
{
    Task<OrderSearchApiModel> PrepareCustomerOrderSearchModelAsync(OrderSearchApiModel searchModel);
    Task<OrderListModel> PrepareCustomerOrderListModelAsync(OrderSearchApiModel searchModel);
}