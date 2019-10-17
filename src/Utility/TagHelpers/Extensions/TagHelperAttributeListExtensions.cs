using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Ocuda.Utility.TagHelpers.Extensions
{
    public static class TagHelperAttributeListExtensions
    {
        public static void AddCssClass(this TagHelperAttributeList attributeList,
            string cssClass)
        {
            var existingCssClassValue = attributeList
                .FirstOrDefault(x => x.Name == "class")?.Value.ToString();

            if (string.IsNullOrEmpty(existingCssClassValue))
            {
                attributeList.SetAttribute("class", cssClass);
            }
            else
            {
                attributeList.SetAttribute("class", $"{ cssClass } { existingCssClassValue }");
            }
        }
    }
}
