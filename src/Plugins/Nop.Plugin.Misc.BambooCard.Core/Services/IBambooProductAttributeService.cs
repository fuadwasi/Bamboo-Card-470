using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.BambooCard.Core.Services;

public interface IBambooProductAttributeService
{
    Task<IPagedList<ProductAttribute>> GetAllProductAttributesAsync(string keyword = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);
}