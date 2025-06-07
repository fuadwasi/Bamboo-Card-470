using System.Collections.Generic;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Models.Customers;

public record AdminLoginModel : LoginModel
{
    public AdminLoginModel()
    {
        StringResources = [];
    }

    public LanguageSelectorModel LanguageNavSelector { get; set; }

    public AppConfigurationModel AppConfigurationModel { get; set; }

    public List<KeyValuePair<string, string>> StringResources { get; set; }

    public bool Rtl { get; set; }
}
