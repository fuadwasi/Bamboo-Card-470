using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.BambooCard.Core.Areas.Admin.Factories;
using Nop.Plugin.Misc.BambooCard.Core.Areas.Admin.Models;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.BambooCard.Core.Areas.Admin.Controllers;
public class BambooProductAttributeController : BaseAdminController
{

    #region Fields

    private readonly IBambooProductAttributeModelFactory _bambooProductAttributeModelFactory;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public BambooProductAttributeController(IBambooProductAttributeModelFactory bambooProductAttributeModelFactory,
        IPermissionService permissionService)
    {
        _bambooProductAttributeModelFactory = bambooProductAttributeModelFactory;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public virtual IActionResult Index()
    {
        return RedirectToAction("List");
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AccessDeniedView();

        //prepare model
        var model = await _bambooProductAttributeModelFactory.PrepareProductAttributeSearchModelAsync(new BambooProductAttributeSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(BambooProductAttributeSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _bambooProductAttributeModelFactory.PrepareProductAttributeListModelAsync(searchModel);

        return Json(model);
    }

    #endregion

}
