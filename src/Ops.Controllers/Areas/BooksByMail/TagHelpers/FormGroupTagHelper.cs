using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BooksByMail.TagHelpers
{
    [HtmlTargetElement("input", Attributes = attributeName)]
    [HtmlTargetElement("select", Attributes = attributeName)]
    [HtmlTargetElement("textarea", Attributes = attributeName)]
    public class FormGroupTagHelper : TagHelper
    {
        public const string textareaTagName = "textarea";

        // tag attribute names
        private const string attributeName = "formgroup";
        private const string forAttributeName = "asp-for";
        private const string ignoreValidationAttributeName = "ignoreValidation";
        private const string isSingleFieldAttributeName = "isSingleField";

        // classes
        private const string defaultWraperDivClass = "form-group row";
        private const string defaultRowDivClass = "row";
        private const string defaultLabelClass = "col-md-3 col-form-label text-md-right";
        private const string defaultInputClass = "form-control";
        private const string defaultInnerDivClass = "col-md-9";
        private const string defaultValidationMessageClass = "text-danger";
        private const string validationIgnoreClass = "validation-ignore";

        // single field
        private const string singleFieldRowsAttribute = "rows";
        private const int singleFieldRowsValue = 5;
        private const string singleFieldInnerDivClass = "col-12";

        private readonly IHtmlGenerator _htmlGenerator;
        public FormGroupTagHelper(IHtmlGenerator htmlGenerator)
        {
            _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        }

        [HtmlAttributeName(forAttributeName)]
        public ModelExpression For { get; set; }
        [HtmlAttributeName(isSingleFieldAttributeName)]
        public bool IsSingleField { get; set; }
        [HtmlAttributeName(ignoreValidationAttributeName)]
        public bool IgnoreValidation { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Create each child tag helper element 
            TagHelperOutput inputElement = await CreateInputElement(context, output);
            TagHelperOutput validationMessageElement = await CreateValidationMessageElement(context);

            var innerDivClass = IsSingleField ? singleFieldInnerDivClass : defaultInnerDivClass;

            // Wrap input and validation with column div
            IHtmlContent innerDiv = WrapElementsWithDiv(
                    new List<IHtmlContent>()
                    {
                        inputElement,
                        validationMessageElement
                    },
                    innerDivClass
                );

            var formGroupElements = new List<IHtmlContent>();

            // Create and add the label if it's not a single field
            if (!IsSingleField)
            {
                formGroupElements.Add(await CreateLabelElement(context));
            }
            formGroupElements.Add(innerDiv);

            // Wrap all elements with a form group div
            IHtmlContent formGroupDiv = WrapElementsWithDiv(
                    formGroupElements,
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

            if (output.TagName == textareaTagName && IsSingleField)
            {
                attributeList.Add(singleFieldRowsAttribute, singleFieldRowsValue);
            } 

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

            var validatorClass = defaultValidationMessageClass;

            if(IgnoreValidation)
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
