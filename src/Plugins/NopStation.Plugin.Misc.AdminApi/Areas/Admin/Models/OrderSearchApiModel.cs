using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models;
public record OrderSearchApiModel : BaseSearchModel
{
    public string CustomerEmail { get; set; }
}
