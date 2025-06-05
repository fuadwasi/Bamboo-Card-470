using Nop.Plugin.Misc.BambooCard.Core.Areas.Admin.Models;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.Misc.BambooCard.Core.Areas.Admin.Factories;

public interface IBambooProductAttributeModelFactory
{
    Task<BambooProductAttributeSearchModel> PrepareProductAttributeSearchModelAsync(BambooProductAttributeSearchModel searchModel);

    Task<ProductAttributeListModel> PrepareProductAttributeListModelAsync(BambooProductAttributeSearchModel searchModel);
}