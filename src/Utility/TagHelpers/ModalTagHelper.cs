using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Ocuda.Utility.TagHelpers
{
    public enum ModalTypes
    {
        Add,
        Edit,
        Delete
    }

    [HtmlTargetElement("div", Attributes = attributeName)]
    public class ModalTagHelper : TagHelper
    {
        private const string attributeName = "modal";
        private const string bodyClass = "modal-body";
        private const string bodyDeleteTextClass = "modal-text";
        private const string buttonSpinnerClass = "fas fa-spinner fa-lg fa-pulse fa-fw ml-1 d-none";
        private const string cancelButtonClass = "btn btn-outline-secondary";
        private const string confirmButtonClass = "modal-btn-confirm";
        private const string containerClass = "row";
        private const string contentClass = "modal-content";
        private const string deleteButtonClass = "btn btn-outline-danger btn-spinner";
        private const string dialogClass = "modal-dialog";
        private const string footerClass = "modal-footer";
        private const string footerDeleteIconClass = "fas fa-minus-circle mr-1";
        private const string headerAttibuteName = "modal-header";
        private const string headerButtonClass = "close";
        private const string headerClass = "modal-header";
        private const string headerTitleClass = "modal-title";
        private const string idAttributeName = "id";
        private const string isLargeAttributeName = "isLarge";
        private const string isNonSubmitAttributeName = "isNonSubmit";
        private const string modalClass = "modal fade";
        private const string modalLargeClass = "modal-lg oc-modal-xl";
        private const string nameAttributeName = "name";
        private const string saveButtonClass = "btn btn-outline-success btn-spinner";
        private const string saveIconClass = "far fa-save mr-1";
        private const string typeAttributeName = "type";

        [HtmlAttributeName(idAttributeName)]
        public string Id { get; set; }

        [HtmlAttributeName(isLargeAttributeName)]
        public bool IsLarge { get; set; }

        [HtmlAttributeName(isNonSubmitAttributeName)]
        public bool IsNonSubmit { get; set; }

        [HtmlAttributeName(headerAttibuteName)]
        public string ModalHeader { get; set; }

        [HtmlAttributeName(nameAttributeName)]
        public string Name { get; set; }

        [HtmlAttributeName(typeAttributeName)]
        public ModalTypes? Type { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public async Task<TagBuilder> CreateBody(TagHelperOutput output)
        {
            if (output == null) { throw new ArgumentNullException(nameof(output)); }

            var body = new TagBuilder("div");
            body.AddCssClass(bodyClass);
            body.InnerHtml.AppendHtml(await output.GetChildContentAsync());

            if (Type == ModalTypes.Delete)
            {
                var text = new TagBuilder("span");
                text.AddCssClass(bodyDeleteTextClass);
                body.InnerHtml.AppendHtml(text);
            }

            return body;
        }

        public TagBuilder CreateFooter()
        {
            var footer = new TagBuilder("div");
            footer.AddCssClass(footerClass);

            var cancelButton = new TagBuilder("button");
            cancelButton.AddCssClass(cancelButtonClass);
            cancelButton.Attributes.Add("type", "button");
            cancelButton.Attributes.Add("data-dismiss", "modal");

            if (!Type.HasValue)
            {
                cancelButton.InnerHtml.Append("Close");
                footer.InnerHtml.AppendHtml(cancelButton);
            }
            else
            {
                cancelButton.InnerHtml.Append("Cancel");
                footer.InnerHtml.AppendHtml(cancelButton);
                var confirmButton = new TagBuilder("button");
                confirmButton.AddCssClass(confirmButtonClass);

                if (IsNonSubmit)
                {
                    confirmButton.Attributes.Add("type", "button");
                }
                else
                {
                    confirmButton.Attributes.Add("type", "submit");
                }

                if (Type == ModalTypes.Delete)
                {
                    var icon = new TagBuilder("span");
                    icon.AddCssClass(footerDeleteIconClass);

                    confirmButton.InnerHtml.AppendHtml(icon);
                    confirmButton.AddCssClass(deleteButtonClass);
                    confirmButton.InnerHtml.Append("Delete");
                }
                else
                {
                    confirmButton.AddCssClass(saveButtonClass);
                    var icon = new TagBuilder("span");
                    icon.AddCssClass(saveIconClass);
                    confirmButton.InnerHtml.AppendHtml(icon);
                    confirmButton.InnerHtml.Append("Save");
                }

                var spinner = new TagBuilder("span");
                spinner.AddCssClass(buttonSpinnerClass);
                confirmButton.InnerHtml.AppendHtml(spinner);

                footer.InnerHtml.AppendHtml(confirmButton);
            }

            return footer;
        }

        public TagBuilder CreateHeader()
        {
            var header = new TagBuilder("div");
            header.AddCssClass(headerClass);

            var title = new TagBuilder("h5");
            title.AddCssClass("fs-5");
            title.AddCssClass(headerTitleClass);
            title.Attributes.Add("id", $"{Id}Label");

            var titleStrings = new StringBuilder(ModalHeader);

            if (titleStrings.Length == 0)
            {
                titleStrings.Append(Type == ModalTypes.Edit ? "Update" : Type)
                    .Append(' ')
                    .Append(Name);
            }

            title.InnerHtml.Append(titleStrings.ToString().Trim());
            header.InnerHtml.AppendHtml(title);

            var button = new TagBuilder("button");
            button.AddCssClass(headerButtonClass);
            button.Attributes.Add("type", "button");
            button.Attributes.Add("data-dismiss", "modal");
            button.Attributes.Add("aria-label", "Close dialog.");
            button.InnerHtml.AppendHtml("&times;");

            header.InnerHtml.AppendHtml(button);
            return header;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null) { throw new ArgumentNullException(nameof(context)); }
            if (output == null) { throw new ArgumentNullException(nameof(output)); }

            var content = new TagBuilder("div");
            content.AddCssClass(contentClass);
            content.InnerHtml.AppendHtml(CreateHeader())
                .AppendHtml(await CreateBody(output))
                .AppendHtml(CreateFooter());

            var dialog = new TagBuilder("div");
            var appenededDialogClass = new StringBuilder(dialogClass);
            if (IsLarge)
            {
                appenededDialogClass.Append(' ').Append(modalLargeClass);
            }
            dialog.AddCssClass(appenededDialogClass.ToString());
            dialog.Attributes.Add("role", "document");
            dialog.InnerHtml.AppendHtml(content);

            var modal = new TagBuilder("div");

            var appendedModalClass = new StringBuilder(modalClass);
            var tagClass = output.Attributes.FirstOrDefault(_ => _.Name == "class");
            if (tagClass != null)
            {
                appendedModalClass.Append(' ').Append(tagClass.Value);
            }
            modal.AddCssClass(appendedModalClass.ToString());

            modal.Attributes.Add("id", Id);
            modal.Attributes.Add("tabindex", "-1");
            modal.Attributes.Add("role", "dialog");
            modal.InnerHtml.AppendHtml(dialog);

            foreach (var attribute in output
                .Attributes
                .Where(_ => _.Name.StartsWith("data-"))
                .ToList())
            {
                modal.Attributes.Add(attribute.Name, attribute.Value.ToString());
                output.Attributes.Remove(attribute);
            }

            output.Content.SetHtmlContent(modal);
            output.Attributes.SetAttribute("class", containerClass);
            output.Attributes.Remove(new TagHelperAttribute(attributeName));
        }
    }
}
