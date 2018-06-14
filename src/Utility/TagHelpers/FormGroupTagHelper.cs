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
        private const string defaultWraperDivClass = "form-group row";
        private const string defaultRowDivClass = "row";
        private const string defaultLabelClass = "col-md-3 col-form-label text-md-right";
        private const string defaultInputClass = "form-control";
        private const string defaultInnerDivClass = "col-md-9";
        private const string defaultValidationMessageClass = "text-danger";

        private readonly IHtmlGenerator _htmlGenerator;
        public FormGroupTagHelper(IHtmlGenerator htmlGenerator)
        {
            _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        }

        [HtmlAttributeName(forAttributeName)]
        public ModelExpression For { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Manually create each child asp form tag helper element
            TagHelperOutput labelElement = await CreateLabelElement(context);
            TagHelperOutput inputElement = await CreateInputElement(context, output);
            TagHelperOutput validationMessageElement = await CreateValidationMessageElement(context);

            // Wrap input and validation with column div
            IHtmlContent innerDiv = WrapElementsWithDiv(
                    new List<IHtmlContent>()
                    {
                        inputElement,
                        validationMessageElement
                    },
                    defaultInnerDivClass
                );

            // Wrap all elements with a form group div
            IHtmlContent formGroupDiv = WrapElementsWithDiv(
                    new List<IHtmlContent>()
                    {
                        labelElement,
                        innerDiv
                    },
                    defaultWraperDivClass
                );

            // Reinitialize the parent tag helper into an empty tag
            output.Reinitialize("", TagMode.StartTagAndEndTag);

            // Put everything into the innerHtml of this tag helper
            output.Content.SetHtmlContent(formGroupDiv);
        }

        private async Task<TagHelperOutput> CreateLabelElement(TagHelperContext context)
        {
            LabelTagHelper labelTagHelper =
                new LabelTagHelper(_htmlGenerator)
                {
                    For = this.For,
                    ViewContext = this.ViewContext
                };

            TagHelperOutput labelOutput = CreateTagHelperOutput("label");

            await labelTagHelper.ProcessAsync(context, labelOutput);

            labelOutput.Attributes.Add(
                new TagHelperAttribute("class", defaultLabelClass));

            return labelOutput;
        }

        private async Task<TagHelperOutput> CreateInputElement(TagHelperContext context, TagHelperOutput output)
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

            return inputOutput;
        }

        private async Task<TagHelperOutput> CreateValidationMessageElement(TagHelperContext context)
        {
            ValidationMessageTagHelper validationMessageTagHelper =
                new ValidationMessageTagHelper(_htmlGenerator)
                {
                    For = this.For,
                    ViewContext = this.ViewContext
                };

            TagHelperOutput validationMessageOutput = CreateTagHelperOutput("span");

            validationMessageOutput.Attributes.Add(
                new TagHelperAttribute("class", defaultValidationMessageClass));

            await validationMessageTagHelper.ProcessAsync(context, validationMessageOutput);        

            return validationMessageOutput;
        }

        private IHtmlContent WrapElementsWithDiv(List<IHtmlContent> elements, string classValue)
        {
            TagBuilder div = new TagBuilder("div");
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
                getChildContentAsync: (s, t) =>
                {
                    return Task.Factory.StartNew<TagHelperContent>(
                            () => new DefaultTagHelperContent());
                }
            );
        }
    }
}
