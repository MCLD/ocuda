using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using Ocuda.Utility.TagHelpers.Extensions;

namespace Ocuda.Utility.TagHelpers
{
    [HtmlTargetElement("input", Attributes = attributeName)]
    [HtmlTargetElement("select", Attributes = attributeName)]
    [HtmlTargetElement("textarea", Attributes = attributeName)]
    public class FormGroupTagHelper : TagHelper
    {
        private const string attributeName = "formgroup";
        private const string defaultInnerDivClass = "form-group-inner col-md-9";
        private const string defaultInputClass = "form-control";
        private const string defaultLabelClass = "col-form-label text-md-right";
        private const string defaultLabelLayoutClass = "col-md-3";
        private const string defaultValidationMessageClass = "validation-message text-danger";
        private const string defaultWrapperDivClass = "row form-group";
        private const string forAttributeName = "asp-for";
        private const string hideLabelAttributeName = "hide-label";
        private const string hideLabelInnerDivClass = "form-group-inner col-12";
        private const string ignoreValidationAttributeName = "ignore-validation";
        private const string infoTooltipAttributeName = "info-tooltip";
        private const string labelClassAttribute = "label-class";
        private const string labelNameAttribute = "label-name";
        private const string onBlurJs = "on-blur-js";
        private const string requiredFieldClass = "fas fa-asterisk fa-xs d-inline-block ml-2 text-danger oc-required-field-marker";
        private const string validationIgnoreClass = "validation-ignore";
        private readonly IHtmlGenerator _htmlGenerator;
        private readonly IStringLocalizer<i18n.Resources.Shared> _localizer;
        public FormGroupTagHelper(IHtmlGenerator htmlGenerator,
            IStringLocalizer<i18n.Resources.Shared> localizer)
        {
            _htmlGenerator = htmlGenerator
                ?? throw new ArgumentNullException(nameof(htmlGenerator));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        [HtmlAttributeName(forAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(hideLabelAttributeName)]
        public bool HideLabel { get; set; }

        [HtmlAttributeName(ignoreValidationAttributeName)]
        public bool IgnoreValidation { get; set; }

        [HtmlAttributeName(infoTooltipAttributeName)]
        public string InfoTooltip { get; set; }

        [HtmlAttributeName(labelClassAttribute)]
        public string LabelClass { get; set; }

        [HtmlAttributeName(labelNameAttribute)]
        public string LabelName { get; set; }

        [HtmlAttributeName(onBlurJs)]
        public string OnBlurJs { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Manually create each child asp form tag helper element
            TagHelperOutput labelElement = null;
            if (!HideLabel)
            {
                labelElement = await CreateLabelElement(context, output);
            }
            TagHelperOutput inputElement = await CreateInputElement(output);
            TagHelperOutput validationMessageElement
                = await CreateValidationMessageElement(context);

            // Wrap input and validation with column div
            IHtmlContent innerDiv = WrapElementsWithDiv(
                    new List<IHtmlContent>
                    {
                        inputElement,
                        validationMessageElement
                    },
                    HideLabel ? hideLabelInnerDivClass : defaultInnerDivClass
                );

            // Wrap all elements with a form group div
            IHtmlContent formGroupDiv = WrapElementsWithDiv(
                    new List<IHtmlContent>
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

        private async Task<TagHelperOutput> CreateInputElement(TagHelperOutput output)
        {
            var attributes = new TagHelperAttributeList(output.Attributes);

            string inputId = null;
            if (output.Attributes.TryGetAttribute("id", out var idAttribute))
            {
                inputId = idAttribute.Value.ToString();
            }

            attributes.AddCssClass(defaultInputClass);
            attributes.RemoveAll(attributeName);
            var inputOutput = CreateTagHelperOutput(output.TagName, attributes);

            inputOutput.Content.AppendHtml(output.PreContent.GetContent());
            inputOutput.Content.AppendHtml(output.Content.GetContent());
            inputOutput.Content.AppendHtml(await output.GetChildContentAsync());
            inputOutput.Content.AppendHtml(output.PostContent.GetContent());

            if (!string.IsNullOrEmpty(OnBlurJs))
            {
                inputOutput.Attributes.Add("onblur", $"{OnBlurJs}(this)");
            }

            if (inputOutput.Attributes.ContainsName("formgroup-val-required"))
            {
                var tagValue = inputOutput.Attributes["formgroup-val-required"];
                inputOutput.Attributes.Add("data-val-required", tagValue.Value);
                inputOutput.Attributes.Remove(tagValue);
            }

            return inputOutput;
        }

        private async Task<TagHelperOutput> CreateLabelElement(TagHelperContext context,
                    TagHelperOutput output)
        {
            var labelTagHelper =
                new LabelTagHelper(_htmlGenerator)
                {
                    For = For,
                    ViewContext = ViewContext
                };

            TagHelperOutput labelOutput = CreateTagHelperOutput("label", null);

            if (!string.IsNullOrWhiteSpace(LabelName))
            {
                labelOutput.Content.SetContent(LabelName);
            }

            await labelTagHelper.ProcessAsync(context, labelOutput);

            string combinedLabelClass = string.IsNullOrEmpty(LabelClass)
                ? string.Join(' ', defaultLabelClass, defaultLabelLayoutClass)
                : string.Join(' ', defaultLabelClass, LabelClass);

            labelOutput.Attributes.Add(
                new TagHelperAttribute("class", combinedLabelClass));

            if (!string.IsNullOrEmpty(InfoTooltip))
            {
                string popupTooltip = InfoTooltip.Replace("'",
                    "\\'",
                    StringComparison.InvariantCultureIgnoreCase);

                var tooltip = new TagBuilder("span");
                tooltip.AddCssClass("fas fa-info-circle");
                tooltip.Attributes.Add("data-toggle", "tooltip");
                tooltip.Attributes.Add("href", "#");
                tooltip.Attributes.Add("title", _localizer[InfoTooltip]);
                tooltip.Attributes.Add("onclick", $"alert('{popupTooltip}');");
                labelOutput.Content.AppendHtml("&nbsp;");
                labelOutput.Content.AppendHtml(tooltip.RenderSelfClosingTag());
            }

            if (output.Attributes.ContainsName("data-val-required")
                || output.Attributes.ContainsName("formgroup-val-required"))
            {
                var icon = new TagBuilder("span");
                icon.AddCssClass(requiredFieldClass);

                labelOutput.Content.AppendHtml(icon.RenderSelfClosingTag());
            }
            return labelOutput;
        }

        private TagHelperOutput CreateTagHelperOutput(string tagName,
            TagHelperAttributeList attributes)
        {
            return new TagHelperOutput(
                tagName: tagName,
                attributes: attributes ?? new TagHelperAttributeList(),
                getChildContentAsync: (_, __) =>
                {
                    return Task.Factory.StartNew<TagHelperContent>(
                            () => new DefaultTagHelperContent());
                }
            );
        }

        private async Task<TagHelperOutput> CreateValidationMessageElement(TagHelperContext context)
        {
            var validationMessageTagHelper =
                new ValidationMessageTagHelper(_htmlGenerator)
                {
                    For = For,
                    ViewContext = ViewContext
                };

            TagHelperOutput validationMessageOutput = CreateTagHelperOutput("span", null);

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
    }
}