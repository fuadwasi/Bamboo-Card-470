using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.BambooCard.Core.Areas.Admin.Models;
public record BambooProductAttributeSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Admin.Plugin.Misc.BambooCard.Core.ProductAttributeSearch.Fields.SearchKeyword")]
    public string SearchKeyword { get; set; }
}
