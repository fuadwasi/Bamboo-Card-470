using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;

namespace Nop.Plugin.Misc.BambooCard.Core.Services;
public class BambooProductAttributeService : IBambooProductAttributeService
{
    private readonly IRepository<ProductAttribute> _productAttributeRepository;

    #region Fields

    #endregion

    #region Ctor

    public BambooProductAttributeService(IRepository<ProductAttribute> productAttributeRepository)
    {
        _productAttributeRepository = productAttributeRepository;
    }

    #endregion

    #region Utilites

    #endregion

    #region Methods

    public virtual async Task<IPagedList<ProductAttribute>> GetAllProductAttributesAsync(string keyword = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var productAttributes = await _productAttributeRepository.GetAllPagedAsync(query =>
        {
            query = from pa in query
                   orderby pa.Name
                   select pa;

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(pa => pa.Name.Contains(keyword) || pa.Description.Contains(keyword));

            return query;
        }, pageIndex, pageSize);

        return productAttributes;
    }

    #endregion
}