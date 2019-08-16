using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Ocuda.Utility.TagHelpers
{
    [HtmlTargetElement(targetElement, Attributes = attributeName)]
    public class ButtonSpinnerTagHelper : TagHelper
    {
        private const string targetElement = "button";
        private const string attributeName = "buttonSpinner";
        private const string ignoreValidationAttributeName = "ignoreValidation";
        private const string classAttribute = "class";
        private const string spinnerClass = "btn-spinner";
        private const string ignoreValidationClass = "spinner-ignore-validation";
        private const string spanTag = "span";
        private const string spanClass = "fa fa-spinner fa-pulse fa-fw d-none";

        [HtmlAttributeName(ignoreValidationAttributeName)]
        public bool IgnoreValidation { get; set; }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var existingClasses = output.Attributes.FirstOrDefault(f => f.Name == classAttribute);
            var buttonClasses = string.Empty;
            if (existingClasses != null)
            {
                buttonClasses = existingClasses.Value.ToString();
                output.Attributes.Remove(existingClasses);
            }
            buttonClasses += $" {spinnerClass}";
            if (IgnoreValidation)
            {
                buttonClasses += $" {ignoreValidationClass}";
            }

            var buttonClassAttribute = new TagHelperAttribute(classAttribute, buttonClasses);
            output.Attributes.Add(buttonClassAttribute);

            var spinnerSpan = new TagBuilder(spanTag)
            {
                TagRenderMode = TagRenderMode.Normal
            };
            spinnerSpan.AddCssClass(spanClass);

            output.PostContent.SetHtmlContent(spinnerSpan);
        }
    }
}
