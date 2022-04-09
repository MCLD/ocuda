using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Ocuda.Utility.TagHelpers
{
    /// <summary>
    /// Tee auto-active tag helper will add an "active" css class to the current tag if all of the
    /// following are true:
    /// - The asp-action property matches the current route's action
    /// - The asp-area property is not defined or matches the current route's area
    /// - The asp-controller is not defined or  matches the current route's controller
    /// </summary>
    [HtmlTargetElement(Attributes = "auto-active")]
    public class AutoActiveTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public AutoActiveTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory
                ?? throw new ArgumentNullException(nameof(urlHelperFactory));
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContextData { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null) { throw new ArgumentNullException(nameof(context)); }
            if (output == null) { throw new ArgumentNullException(nameof(output)); }

            var routeAction = ViewContextData.RouteData.Values["action"]?.ToString();
            var routeArea = ViewContextData.RouteData.Values["area"]?.ToString();
            var routeController = ViewContextData.RouteData.Values["controller"]?.ToString();

            var linkAction = context.AllAttributes["asp-action"]?.Value.ToString();
            var linkArea = context.AllAttributes["asp-area"]?.Value.ToString();
            var linkController = context.AllAttributes["asp-controller"]?.Value.ToString();

            var areaCompare = string.IsNullOrEmpty(linkArea) || linkArea == routeArea;

            var controllerCompare = string.IsNullOrEmpty(linkController) 
                || linkController == routeController;

            if (linkAction == routeAction && areaCompare && controllerCompare)
            {
                var cssClass = new StringBuilder("active");
                if(output.Attributes.TryGetAttribute("class", out var classAttribute))
                {
                    cssClass.Append(' ').Append(classAttribute.Value.ToString().Trim());
                    output.Attributes.Remove(classAttribute);
                }
                output.Attributes.Add(new TagHelperAttribute("class", cssClass.ToString()));
            }

            output.Attributes.Remove(new TagHelperAttribute("auto-active"));
        }
    }
}
