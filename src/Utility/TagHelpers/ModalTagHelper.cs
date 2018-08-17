﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Ocuda.Utility.TagHelpers
{
    [HtmlTargetElement("div", Attributes = attributeName)]
    public class ModalTagHelper : TagHelper
    {
        private const string attributeName = "modal";
        private const string idAttributeName = "id";
        private const string nameAttributeName = "name";
        private const string typeAttributeName = "type";
        private const string isNonSubmitAttributeName = "isNonSubmit";
        private const string containerClass = "row";
        private const string modalClass = "modal fade";
        private const string dialogClass = "modal-dialog";
        private const string contentClass = "modal-content";
        private const string headerClass = "modal-header";
        private const string headerTitleClass = "modal-title";
        private const string headerButtonClass = "close";
        private const string headerIconClass = "fa fa-times";
        private const string bodyClass = "modal-body";
        private const string bodyDeleteIconClass = "fa fa-exclamation-triangle";
        private const string bodyDeleteTextClass = "modal-text";
        private const string footerClass = "modal-footer";
        private const string cancelButtonClass = "btn btn-outline-secondary";
        private const string confirmButtonClass = "modal-btn-confirm";
        private const string deleteButtonClass = "btn btn-danger btn-spinner";
        private const string saveButtonClass = "btn btn-success btn-spinner";
        private const string footerDeleteIconClass = "fa fa-times";
        private const string footerSaveIconClass = "fa fa-save";
        private const string buttonSpinnerClass = "fa fa-spinner fa-lg fa-pulse fa-fw d-none";

        private readonly IHtmlGenerator _htmlGenerator;
        public ModalTagHelper(IHtmlGenerator htmlGenerator)
        {
            _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        }

        [HtmlAttributeName(idAttributeName)]
        public string Id { get; set; }

        [HtmlAttributeName(nameAttributeName)]
        public string Name { get; set; }

        [HtmlAttributeName(typeAttributeName)]
        public ModalTypes? Type { get; set; }

        [HtmlAttributeName(isNonSubmitAttributeName)]
        public bool IsNonSubmit { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = new TagBuilder("div");
            content.AddCssClass(contentClass);
            content.InnerHtml.AppendHtml(CreateHeader()).AppendHtml(await CreateBody(output));
            if (Type.HasValue)
            {
                content.InnerHtml.AppendHtml(CreateFooter());
            }

            var dialog = new TagBuilder("div");
            dialog.AddCssClass(dialogClass);
            dialog.Attributes.Add("role", "document");
            dialog.InnerHtml.AppendHtml(content);

            var modal = new TagBuilder("div");
            modal.AddCssClass(modalClass);
            modal.Attributes.Add("id", Id);
            modal.Attributes.Add("tabindex", "-1");
            modal.Attributes.Add("role", "dialog");
            modal.InnerHtml.AppendHtml(dialog);

            output.Content.SetHtmlContent(modal);
            output.Attributes.SetAttribute("class", containerClass);
            output.Attributes.Remove(new TagHelperAttribute(attributeName));
        }

        public TagBuilder CreateHeader()
        {
            var header = new TagBuilder("div");
            header.AddCssClass(headerClass);

            var title = new TagBuilder("h5");
            title.AddCssClass(headerTitleClass);
            title.Attributes.Add("id", $"{Id}Label");
            var titleStrings = new List<string>();
            if (Type.HasValue)
            {
                titleStrings.Add(Type.ToString());
            }
            if (!string.IsNullOrWhiteSpace(Name))
            {
                titleStrings.Add(Name);
            }
            if (titleStrings.Count > 0)
            {
                title.InnerHtml.Append(string.Join(" ", titleStrings));
            }
            header.InnerHtml.AppendHtml(title);

            var button = new TagBuilder("button");
            button.AddCssClass(headerButtonClass);
            button.Attributes.Add("type", "button");
            button.Attributes.Add("data-dismiss", "modal");

            var icon = new TagBuilder("span");
            icon.AddCssClass(headerIconClass);
            button.InnerHtml.AppendHtml(icon);

            header.InnerHtml.AppendHtml(button);

            return header;
        }

        public async Task<TagBuilder> CreateBody(TagHelperOutput output)
        {
            var body = new TagBuilder("div");
            body.AddCssClass(bodyClass);
            if (Type == ModalTypes.Delete)
            {
                var icon = new TagBuilder("span");
                icon.AddCssClass(bodyDeleteIconClass);
                body.InnerHtml.AppendHtml(icon);

                var text = new TagBuilder("span");
                text.AddCssClass(bodyDeleteTextClass);
                body.InnerHtml.AppendHtml(text);
            }
            else
            {
                body.InnerHtml.AppendHtml(await output.GetChildContentAsync());
            }

            return body;
        }

        public TagBuilder CreateFooter()
        {
            var cancelButton = new TagBuilder("button");
            cancelButton.AddCssClass(cancelButtonClass);
            cancelButton.Attributes.Add("type", "button");
            cancelButton.Attributes.Add("data-dismiss", "modal");
            cancelButton.InnerHtml.Append("Cancel");
            var confirmButton = new TagBuilder("button");
            confirmButton.AddCssClass(confirmButtonClass);

            if(IsNonSubmit)
            {
                confirmButton.Attributes.Add("type", "button");
                confirmButton.Attributes.Add("data-dismiss", "modal");
            }
            else
            {
                confirmButton.Attributes.Add("type", "submit");
            }
            
            var icon = new TagBuilder("span");
            confirmButton.InnerHtml.AppendHtml(icon);

            if (Type == ModalTypes.Delete)
            {
                confirmButton.AddCssClass(deleteButtonClass);
                icon.AddCssClass(footerDeleteIconClass);
                confirmButton.InnerHtml.Append(" Delete ");
            }
            else
            {
                confirmButton.AddCssClass(saveButtonClass);
                icon.AddCssClass(footerSaveIconClass);
                confirmButton.InnerHtml.Append(" Save ");
            }

            var spinner = new TagBuilder("span");
            spinner.AddCssClass(buttonSpinnerClass);
            confirmButton.InnerHtml.AppendHtml(spinner);

            var footer = new TagBuilder("div");
            footer.AddCssClass(footerClass);
            footer.InnerHtml.AppendHtml(cancelButton).AppendHtml(confirmButton);

            return footer;
        }
    }

    public enum ModalTypes
    {
        Add,
        Edit,
        Delete
    }
}
