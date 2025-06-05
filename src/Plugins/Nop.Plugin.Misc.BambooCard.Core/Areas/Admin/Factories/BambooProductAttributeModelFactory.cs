using Nop.Plugin.Misc.BambooCard.Core.Areas.Admin.Models;
using Nop.Plugin.Misc.BambooCard.Core.Services;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.BambooCard.Core.Areas.Admin.Factories;
public class BambooProductAttributeModelFactory : IBambooProductAttributeModelFactory
{
    private readonly IBambooProductAttributeService _bambooProductAttributeService;

    #region Fields

    #endregion

    #region Ctor

    public BambooProductAttributeModelFactory(IBambooProductAttributeService bambooProductAttributeService)
    {
        _bambooProductAttributeService = bambooProductAttributeService;
    }

    #endregion

    #region Methods

    public virtual Task<BambooProductAttributeSearchModel> PrepareProductAttributeSearchModelAsync(BambooProductAttributeSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    public virtual async Task<ProductAttributeListModel> PrepareProductAttributeListModelAsync(BambooProductAttributeSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var productAttributes = await _bambooProductAttributeService
            .GetAllProductAttributesAsync(keyword: searchModel.SearchKeyword, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = new ProductAttributeListModel().PrepareToGrid(searchModel, productAttributes, () =>
        {
            return productAttributes.Select(attribute => attribute.ToModel<ProductAttributeModel>());
        });

        return model;
    }

    #endregion
}