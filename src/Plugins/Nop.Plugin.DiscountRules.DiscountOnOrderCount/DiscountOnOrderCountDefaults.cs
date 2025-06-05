namespace Nop.Plugin.DiscountRules.DiscountOnOrderCount;
public static class DiscountOnOrderCountDefaults
{

    public static string SystemName => "DiscountRules.DiscountOnOrderCount";
    public static string OrderCountSettingsKey => "DiscountRequirement.DiscountOnOrderCount.OrderCount-{0}";
    public static string OrderStatusIdsSettingsKey => "DiscountRequirement.DiscountOnOrderCount.OrderStatusIds-{0}";

    public static string HtmlFieldPrefix => "DiscountOnOrderCount{0}";
}
