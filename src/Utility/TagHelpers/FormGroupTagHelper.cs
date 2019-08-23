using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Ocuda.Utility.TagHelpers.Extensions;

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
        private const string dateTimePickerAttribute = "datetime-picker";

        private const string defaultWrapperDivClass = "row form-group";
        private const string defaultLabelLayoutClass = "col-md-3";
        private const string defaultLabelClass = "col-form-label text-md-right";
        private const string defaultInputClass = "form-control";
        private const string defaultInnerDivClass = "form-group-inner col-md-9";
        private const string defaultValidationMessageClass = "validation-message text-danger";
        private const string defaultFieldLayoutClass = "col-md-9";
        private const string validationIgnoreClass = "validation-ignore";

        private const string dateTimeGroupClass = "input-group date datetimepicker";
        private const string dateTimeGroupAppendClass = "input-group-append";
        private const string dateTimeGroupTextClass = "input-group-text";
        private const string dateTimeIconCalendarClass = "far fa-calendar-alt";
        private const string dateTimeIconClockClass = "far fa-clock";
        private const string dateTimeInputClass = "datetimepicker-input";
        private const string dateTimeInputType = "text";
        private const string dateTimeTargetInput = "nearest";
        private const string dateTimeToggle = "datetimepicker";

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

        [HtmlAttributeName(dateTimePickerAttribute)]
        public DateTimePickerType? DateTimePicker { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Manually create each child asp form tag helper element
            TagHelperOutput labelElement = await CreateLabelElement(context);
            TagHelperOutput inputElement = await CreateInputElement(output);
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

            TagHelperOutput labelOutput = CreateTagHelperOutput("label", null);

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

            if (DateTimePicker.HasValue && !string.IsNullOrWhiteSpace(inputId))
            {
                var pickerId = $"{inputId}_datetimepicker";

                var icon = new TagBuilder("span");
                if (DateTimePicker.Value == DateTimePickerType.DateTime)
                {
                    icon.AddCssClass(dateTimeIconCalendarClass);
                }
                else
                {
                    icon.AddCssClass(dateTimeIconClockClass);
                }

                var inputGroupText = new TagBuilder("div");
                inputGroupText.AddCssClass(dateTimeGroupTextClass);
                inputGroupText.InnerHtml.SetHtmlContent(icon);

                var inputGroupAppend = new TagBuilder("div");
                inputGroupAppend.AddCssClass(dateTimeGroupAppendClass);
                inputGroupAppend.Attributes.Add("data-target", $"#{pickerId}");
                inputGroupAppend.Attributes.Add("data-toggle", dateTimeToggle);
                inputGroupAppend.InnerHtml.SetHtmlContent(inputGroupText);

                inputOutput.Attributes.RemoveAll("type");
                inputOutput.Attributes.Add("type", dateTimeInputType);
                inputOutput.Attributes.AddCssClass(dateTimeInputClass);
                inputOutput.Attributes.Add("data-target", $"#{pickerId}");
                inputOutput.Attributes.Add("data-toggle", dateTimeToggle);
                inputOutput.PostElement.SetHtmlContent(inputGroupAppend);

                var pickerGroup = new TagBuilder("div");
                pickerGroup.Attributes.Add("id", pickerId);
                pickerGroup.AddCssClass(dateTimeGroupClass);
                pickerGroup.Attributes.Add("data-target-input", dateTimeTargetInput);
                inputOutput.PreElement.AppendHtml(pickerGroup.RenderStartTag());
                inputOutput.PostElement.AppendHtml(pickerGroup.RenderEndTag());
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
    }

    public enum DateTimePickerType
    {
        DateTime,
        TimeOnly
    }
}
