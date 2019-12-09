using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Ocuda.Utility.TagHelpers.Extensions;

namespace Ocuda.Utility.TagHelpers
{
    [HtmlTargetElement("textarea", Attributes = attributeName)]
    public class MarkdownEditorTagHelper : TagHelper
    {
        private const string attributeName = "markdown-editor";

        private const string navTabsClass = "nav nav-tabs";
        private const string navItemClass = "nav-item";
        private const string navLinkClass = "nav-link";
        private const string tabDataToggle = "tab";
        private const string tabRole = "tab";

        private const string tabContentClass = "tab-content md-editor";
        private const string panelClass = "tab-pane";
        private const string editPanelClass = "md-edit-panel";
        private const string previewPanelClass = "md-preview-panel";
        private const string tabPanelRole = "tabpanel";
        private const string buttonRowClass = "md-button-row";
        private const string inputClass = "md-input";
        private const string previewClass = "md-preview";

        private const string activeClass = "active";
        private const string showClass = "show";


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
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
            editTab.AddCssClass(navLinkClass);
            editTab.AddCssClass(activeClass);
            editTab.Attributes.Add("href", $"#{inputId}");
            editTab.Attributes.Add("data-toggle", tabDataToggle);
            editTab.Attributes.Add("role", tabRole);
            editTab.InnerHtml.Append("Edit");

            var editNavItem = new TagBuilder("li");
            editNavItem.AddCssClass(navItemClass);
            editNavItem.InnerHtml.AppendHtml(editTab);

            var previewTab = new TagBuilder("a");
            previewTab.AddCssClass(navLinkClass);
            previewTab.Attributes.Add("href", $"#{previewId}");
            previewTab.Attributes.Add("data-toggle", tabDataToggle);
            previewTab.Attributes.Add("role", tabRole);
            previewTab.InnerHtml.Append("Preview");

            var previewNavItem = new TagBuilder("li");
            previewNavItem.AddCssClass(navItemClass);
            previewNavItem.InnerHtml.AppendHtml(previewTab);

            var navTabs = new TagBuilder("ul");
            navTabs.AddCssClass(navTabsClass);
            navTabs.InnerHtml.AppendHtml(editNavItem);
            navTabs.InnerHtml.AppendHtml(previewNavItem);

            var buttonRow = new TagBuilder("div");
            buttonRow.AddCssClass(buttonRowClass);

            var attributeList = new TagHelperAttributeList(output.Attributes);
            attributeList.AddCssClass(inputClass);
            attributeList.Remove(new TagHelperAttribute(attributeName));
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
            editPanel.AddCssClass(panelClass);
            editPanel.AddCssClass(editPanelClass);
            editPanel.AddCssClass(activeClass);
            editPanel.AddCssClass(showClass);
            editPanel.Attributes.Add("role", tabPanelRole);
            editPanel.InnerHtml.AppendHtml(buttonRow);
            editPanel.InnerHtml.AppendHtml(input);

            var preview = new TagBuilder("div");
            preview.AddCssClass(previewClass);

            var previewPanel = new TagBuilder("div");
            previewPanel.Attributes.Add("id", previewId);
            previewPanel.AddCssClass(panelClass);
            previewPanel.AddCssClass(previewPanelClass);
            previewPanel.Attributes.Add("role", tabPanelRole);
            previewPanel.InnerHtml.AppendHtml(preview);

            var tabContent = new TagBuilder("div");
            tabContent.AddCssClass(tabContentClass);
            tabContent.InnerHtml.AppendHtml(editPanel);
            tabContent.InnerHtml.AppendHtml(previewPanel);

            output.Reinitialize("", TagMode.StartTagAndEndTag);
            output.Content.SetHtmlContent(navTabs);
            output.Content.AppendHtml(tabContent);
        }
    }
}
