using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Ocuda.Utility.TagHelpers
{
    [HtmlTargetElement("input", Attributes = attributeName)]
    [HtmlTargetElement("select", Attributes = attributeName)]
    [HtmlTargetElement("textarea", Attributes = attributeName)]
    public class FormGroupTagHelper : TagHelper
    {
        private const string attributeName = "formgroup";
        private const string forAttributeName = "asp-for";
        private const string ignoreValidationAttributeName = "ignore-validation";
        private const string infoTooltipAttributeName = "info-tooltip";
        private const string onBlurJs = "on-blur-js";
        private const string labelClassAttribute = "label-class";
        private const string fieldClassAttribute = "field-class";

        private const string defaultWrapperDivClass = "row form-group";
        private const string defaultLabelLayoutClass = "col-md-3";
        private const string defaultLabelClass = "col-form-label text-md-right";
        private const string defaultInputClass = "form-control";
        private const string defaultFieldLayoutClass = "col-md-9";
        private const string defaultValidationMessageClass = "text-danger";
        private const string validationIgnoreClass = "validation-ignore";

        private readonly IHtmlGenerator _htmlGenerator;
        public FormGroupTagHelper(IHtmlGenerator htmlGenerator)
        {
            _htmlGenerator = htmlGenerator
                ?? throw new ArgumentNullException(nameof(htmlGenerator));
        }

        [HtmlAttributeName(forAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(ignoreValidationAttributeName)]
        public bool IgnoreValidation { get; set; }

        [HtmlAttributeName(infoTooltipAttributeName)]
        public string InfoTooltip { get; set; }

        [HtmlAttributeName(onBlurJs)]
        public string OnBlurJs { get; set; }

        [HtmlAttributeName(labelClassAttribute)]
        public string LabelClass { get; set; }

        [HtmlAttributeName(fieldClassAttribute)]
        public string FieldClass { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Manually create each child asp form tag helper element
            TagHelperOutput labelElement = await CreateLabelElement(context);
            TagHelperOutput inputElement = await CreateInputElement(context, output);
            TagHelperOutput validationMessageElement
                = await CreateValidationMessageElement(context);

            // Wrap input and validation with column div
            IHtmlContent innerDiv = WrapElementsWithDiv(
                    new List<IHtmlContent>()
                    {
                        inputElement,
                        validationMessageElement
                    },
                    string.IsNullOrEmpty(FieldClass) ? defaultFieldLayoutClass : FieldClass
                );

            // Wrap all elements with a form group div
            IHtmlContent formGroupDiv = WrapElementsWithDiv(
                    new List<IHtmlContent>()
                    {
                        labelElement,
                        innerDiv
                    },
                    defaultWrapperDivClass
                );

            // Reinitialize the parent tag helper into an empty tag
            output.Reinitialize("", TagMode.StartTagAndEndTag);

            // Put everything into the innerHtml of this tag helper
            output.Content.SetHtmlContent(formGroupDiv);
        }

        private async Task<TagHelperOutput> CreateLabelElement(TagHelperContext context)
        {
            var labelTagHelper =
                new LabelTagHelper(_htmlGenerator)
                {
                    For = For,
                    ViewContext = ViewContext
                };

            TagHelperOutput labelOutput = CreateTagHelperOutput("label");

            await labelTagHelper.ProcessAsync(context, labelOutput);

            string combinedLabelClass = string.IsNullOrEmpty(LabelClass)
                ? string.Join(' ', defaultLabelClass, defaultLabelLayoutClass)
                : string.Join(' ', defaultLabelClass, LabelClass);

            labelOutput.Attributes.Add(
                new TagHelperAttribute("class", combinedLabelClass));

            if (!string.IsNullOrEmpty(InfoTooltip))
            {
                var tooltip = new TagBuilder("span");
                tooltip.AddCssClass("fas fa-info-circle");
                tooltip.Attributes.Add("data-toggle", "tooltip");
                tooltip.Attributes.Add("href", "#");
                tooltip.Attributes.Add("title", InfoTooltip);
                labelOutput.Content.AppendHtml("&nbsp;");
                labelOutput.Content.AppendHtml(tooltip.RenderSelfClosingTag());
            }

            return labelOutput;
        }

        private async Task<TagHelperOutput> CreateInputElement(TagHelperContext context,
            TagHelperOutput output)
        {
            var inputOutput = CreateTagHelperOutput(output.TagName);

            var attributeList = output.Attributes;
            attributeList.Add(new TagHelperAttribute("class", defaultInputClass));
            foreach (var attribute in output.Attributes)
            {
                if (attribute.Name != attributeName)
                {
                    var attributes = inputOutput.Attributes
                        .FirstOrDefault(a => a.Name == attribute.Name)?.Value;
                    inputOutput.Attributes
                        .SetAttribute(attribute.Name, $"{attributes} {attribute.Value}".Trim());
                }
            }
            inputOutput.Content.AppendHtml(output.PreContent.GetContent());
            inputOutput.Content.AppendHtml(output.Content.GetContent());
            inputOutput.Content.AppendHtml(await output.GetChildContentAsync());
            inputOutput.Content.AppendHtml(output.PostContent.GetContent());

            if (!string.IsNullOrEmpty(OnBlurJs))
            {
                inputOutput.Attributes.Add("onblur", $"{OnBlurJs}(this)");
            }

            return inputOutput;
        }

        private async Task<TagHelperOutput> CreateValidationMessageElement(TagHelperContext context)
        {
            var validationMessageTagHelper =
                new ValidationMessageTagHelper(_htmlGenerator)
                {
                    For = For,
                    ViewContext = ViewContext
                };

            TagHelperOutput validationMessageOutput = CreateTagHelperOutput("span");

            var validatorClass = defaultValidationMessageClass;

            if (IgnoreValidation)
            {
                validatorClass += $" {validationIgnoreClass}";
            }

            validationMessageOutput.Attributes.Add(
                new TagHelperAttribute("class", validatorClass));

            await validationMessageTagHelper.ProcessAsync(context, validationMessageOutput);

            return validationMessageOutput;
        }

        private IHtmlContent WrapElementsWithDiv(List<IHtmlContent> elements, string classValue)
        {
            var div = new TagBuilder("div");
            div.AddCssClass(classValue);
            foreach (IHtmlContent element in elements)
            {
                div.InnerHtml.AppendHtml(element);
            }

            return div;
        }

        private TagHelperOutput CreateTagHelperOutput(string tagName)
        {
            return new TagHelperOutput(
                tagName: tagName,
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (_, __) =>
                {
                    return Task.Factory.StartNew<TagHelperContent>(
                            () => new DefaultTagHelperContent());
                }
            );
        }
    }
}
