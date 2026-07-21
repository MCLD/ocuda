using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Ocuda.Utility.TagHelpers.Extensions;

namespace Ocuda.Utility.TagHelpers
{
    [HtmlTargetElement("textarea", Attributes = AttributeName)]
    public class MarkdownEditorTagHelper : TagHelper
    {
        private const string ActiveClass = "active";
        private const string AttributeName = "markdown-editor";

        private const string ButtonRowClass = "md-button-row";
        private const string EditPanelClass = "bg-light border md-edit-panel mt-2 pt-1";
        private const string InputClass = "md-input";
        private const string NavItemClass = "nav-item";
        private const string NavLinkClass = "nav-link py-1";
        private const string NavTabsClass = "nav nav-pills";
        private const string PanelClass = "tab-pane";
        private const string PreviewClass = "border md-preview mt-2 p-3 rounded";
        private const string PreviewPanelClass = "md-preview-panel";
        private const string ShowClass = "show";
        private const string TabContentClass = "md-editor tab-content";
        private const string TabDataToggle = "tab";
        private const string TabPanelRole = "tabpanel";
        private const string TabRole = "tab";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(output);

            string id;
            if (output.Attributes.TryGetAttribute("id", out var idAttribute))
            {
                id = idAttribute.Value.ToString();
            }
            else
            {
                return;
            }

            var inputId = $"{id}_input";
            var previewId = $"{id}_preview";

            var editTab = new TagBuilder("a");
            editTab.AddCssClass(NavLinkClass);
            editTab.AddCssClass(ActiveClass);
            editTab.Attributes.Add("href", $"#{inputId}");
            editTab.Attributes.Add("data-bs-toggle", TabDataToggle);
            editTab.Attributes.Add("role", TabRole);
            editTab.InnerHtml.Append("Edit");

            var editNavItem = new TagBuilder("li");
            editNavItem.AddCssClass(NavItemClass);
            editNavItem.InnerHtml.AppendHtml(editTab);

            var previewTab = new TagBuilder("a");
            previewTab.AddCssClass(NavLinkClass);
            previewTab.Attributes.Add("href", $"#{previewId}");
            previewTab.Attributes.Add("data-bs-toggle", TabDataToggle);
            previewTab.Attributes.Add("role", TabRole);
            previewTab.InnerHtml.Append("Preview");

            var previewNavItem = new TagBuilder("li");
            previewNavItem.AddCssClass(NavItemClass);
            previewNavItem.InnerHtml.AppendHtml(previewTab);

            var navTabs = new TagBuilder("ul");
            navTabs.AddCssClass(NavTabsClass);
            navTabs.InnerHtml.AppendHtml(editNavItem);
            navTabs.InnerHtml.AppendHtml(previewNavItem);

            var buttonRow = new TagBuilder("div");
            buttonRow.AddCssClass(ButtonRowClass);

            var attributeList = new TagHelperAttributeList(output.Attributes);
            attributeList.AddCssClass(InputClass);
            attributeList.Remove(new TagHelperAttribute(AttributeName));
            var input = new TagHelperOutput(
                tagName: "textarea",
                attributes: attributeList,
                getChildContentAsync: (_, __) =>
                {
                    return Task.Factory.StartNew<TagHelperContent>(
                            () => new DefaultTagHelperContent());
                });
            input.Content.SetHtmlContent(output.Content.GetContent());

            var editPanel = new TagBuilder("div");
            editPanel.Attributes.Add("id", inputId);
            editPanel.AddCssClass(PanelClass);
            editPanel.AddCssClass(EditPanelClass);
            editPanel.AddCssClass(ActiveClass);
            editPanel.AddCssClass(ShowClass);
            editPanel.Attributes.Add("role", TabPanelRole);
            editPanel.InnerHtml.AppendHtml(buttonRow);
            editPanel.InnerHtml.AppendHtml(input);

            var preview = new TagBuilder("div");
            preview.AddCssClass(PreviewClass);

            var previewPanel = new TagBuilder("div");
            previewPanel.Attributes.Add("id", previewId);
            previewPanel.AddCssClass(PanelClass);
            previewPanel.AddCssClass(PreviewPanelClass);
            previewPanel.Attributes.Add("role", TabPanelRole);
            previewPanel.InnerHtml.AppendHtml(preview);

            var tabContent = new TagBuilder("div");
            tabContent.AddCssClass(TabContentClass);
            tabContent.InnerHtml.AppendHtml(editPanel);
            tabContent.InnerHtml.AppendHtml(previewPanel);

            output.Reinitialize(string.Empty, TagMode.StartTagAndEndTag);
            output.Content.SetHtmlContent(navTabs);
            output.Content.AppendHtml(tabContent);
        }
    }
}
