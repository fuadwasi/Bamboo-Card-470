using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.DiscountRules.DiscountOnOrderCount.Models;
public class RequirementModel
{
    public RequirementModel()
    {
        OrderStatusIds = new List<int>();
        AvailableOrderStatuses = new List<SelectListItem>();
    }

    public int DiscountId { get; set; }
    public int RequirementId { get; set; }

    [NopResourceDisplayName("Plugin.DiscountRules.DiscountOnOrderCount.Fields.OrderCount")]
    public int OrderCount { get; set; }

    [NopResourceDisplayName("Plugin.DiscountRules.DiscountOnOrderCount.Fields.OrderStatus")]
    public IList<int> OrderStatusIds { get; set; }
    public IList<SelectListItem> AvailableOrderStatuses { get; set; }
}
