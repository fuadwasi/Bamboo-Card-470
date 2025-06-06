namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models.Api
{
    public class AppConfigurationModel
    {
        public string AndroidVersion { get; set; }
        public bool AndriodForceUpdate { get; set; }
        public string PlayStoreUrl { get; set; }
        public string IOSVersion { get; set; }
        public bool IOSForceUpdate { get; set; }
        public string AppStoreUrl { get; set; }
        public string LogoUrl { get; set; }
    }
}