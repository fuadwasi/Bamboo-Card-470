using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Misc.BambooCard.Core.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var pluginOutputDir = BambooCardDefaults.PluginOutputDirName;
            if (context.AreaName == "Admin")
                viewLocations = new[] {
                        $"/Plugins/{pluginOutputDir}/Areas/Admin/Views/{{1}}/{{0}}.cshtml",
                        $"/Plugins/{pluginOutputDir}/Areas/Admin/Views/Shared/{{0}}.cshtml",
                        $"/Plugins/{pluginOutputDir}/Areas/Admin/Views/Shared/EditorTemplates/{{0}}.cshtml",
                }.Concat(viewLocations);
            else
            {
                viewLocations = new[] {
                    $"/Plugins/{pluginOutputDir}/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/{pluginOutputDir}/Views/Shared/{{0}}.cshtml"
                }.Concat(viewLocations);

                if (context.Values.TryGetValue(THEME_KEY, out var theme))
                    viewLocations = new[] {
                        $"/Plugins/{pluginOutputDir}/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                        $"/Plugins/{pluginOutputDir}/Themes/{theme}/Views/Shared/{{0}}.cshtml"
                    }.Concat(viewLocations);
            }

            return viewLocations;
        }
    }
}
