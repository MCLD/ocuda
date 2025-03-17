using System;
using BooksByMail.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace Ocuda.Utility.TagHelpers
{
    [HtmlTargetElement("paginate", Attributes = "paginateModel")]
    public class PaginateTagHelper : TagHelper
    {
        private IUrlHelperFactory _urlHelperFactory;

        public PaginateTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory ??
                throw new ArgumentNullException(nameof(urlHelperFactory));
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContextData { get; set; }

        [HtmlAttributeName("paginateModel")]
        public PaginateModel paginateModel { get; set; }

        [HtmlAttributeName("asButtons")]
        public bool asButtons { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper url = _urlHelperFactory.GetUrlHelper(ViewContextData);
            TagBuilder ulTag = new TagBuilder("ul");
            ulTag.TagRenderMode = TagRenderMode.Normal;
            ulTag.MergeAttribute("class", "pagination");

            string firstPage = paginateModel.FirstPage == null
                               ? null
                               : QueryBuilder(url, paginateModel.FirstPage, asButtons);
            ulTag.InnerHtml.AppendHtml(PaginatorLi(firstPage, "fast-backward", asButtons));

            string previousPage = paginateModel.PreviousPage == null
                                  ? null
                                  : QueryBuilder(url, paginateModel.PreviousPage, asButtons);
            ulTag.InnerHtml.AppendHtml(PaginatorLi(previousPage, "backward", asButtons));

            ulTag.InnerHtml.AppendHtml(PaginatorInput(paginateModel, GetNonPageRouteValues(url)));

            ulTag.InnerHtml.AppendHtml(
                PaginatorLi($"of {paginateModel.MaxPage.ToString()}", asButtons));

            string nextPage = paginateModel.NextPage == null
                              ? null
                              : QueryBuilder(url, paginateModel.NextPage, asButtons);

            ulTag.InnerHtml.AppendHtml(PaginatorLi(nextPage, "forward", asButtons));

            string lastPage = paginateModel.LastPage == null
                              ? null
                              : QueryBuilder(url, paginateModel.LastPage, asButtons);

            ulTag.InnerHtml.AppendHtml(PaginatorLi(lastPage, "fast-forward", asButtons));

            TagBuilder navTag = new TagBuilder("nav");
            navTag.TagRenderMode = TagRenderMode.Normal;
            navTag.InnerHtml.SetHtmlContent(ulTag);
            output.Content.SetHtmlContent(navTag);
        }

        public static TagBuilder PaginatorInput(PaginateModel model, RouteValueDictionary routeValues)
        {
            TagBuilder liTag = new TagBuilder("li");
            liTag.TagRenderMode = TagRenderMode.Normal;
            liTag.MergeAttribute("class", "page-item");

            TagBuilder formTag = new TagBuilder("form");
            formTag.MergeAttribute("class", "page-form");
            formTag.MergeAttribute("role", "form");
            formTag.MergeAttribute("method", "get");
            formTag.TagRenderMode = TagRenderMode.Normal;

            TagBuilder inputTag = new TagBuilder("input");
            inputTag.MergeAttribute("name", "page");
            inputTag.MergeAttribute("type", "number");
            inputTag.MergeAttribute("min", "1");
            inputTag.MergeAttribute("max", model.MaxPage.ToString());
            inputTag.MergeAttribute("class", "page-link page-input");
            inputTag.MergeAttribute("value", model.CurrentPage.ToString());
            inputTag.TagRenderMode = TagRenderMode.Normal;

            formTag.InnerHtml.AppendHtml(inputTag);

            foreach (var routeValue in routeValues)
            {
                TagBuilder routeInputTag = new TagBuilder("input");
                routeInputTag.MergeAttribute("type", "hidden");
                routeInputTag.MergeAttribute("name", routeValue.Key);
                routeInputTag.MergeAttribute("value", routeValue.Value.ToString());
                routeInputTag.TagRenderMode = TagRenderMode.Normal;

                formTag.InnerHtml.AppendHtml(routeInputTag);
            }

            liTag.InnerHtml.SetHtmlContent(formTag);

            return liTag;
        }

        private static TagBuilder PaginatorLi(string text, bool asButtons)
        {
            TagBuilder liTag = new TagBuilder("li");
            liTag.TagRenderMode = TagRenderMode.Normal;
            liTag.MergeAttribute("class", "page-item disabled");

            if (asButtons)
            {
                TagBuilder buttonTag = new TagBuilder("button");
                buttonTag.TagRenderMode = TagRenderMode.Normal;
                buttonTag.InnerHtml.SetHtmlContent(text);
                buttonTag.MergeAttribute("class", "page-link disabled");
                buttonTag.MergeAttribute("type", "button");
                liTag.InnerHtml.SetHtmlContent(buttonTag);
            }
            else
            {
                TagBuilder aTag = new TagBuilder("a");
                aTag.MergeAttribute("href", "#");
                aTag.MergeAttribute("class", "page-link");
                aTag.MergeAttribute("onclick", "return false;");
                aTag.InnerHtml.SetHtmlContent(text);
                aTag.TagRenderMode = TagRenderMode.Normal;
                liTag.InnerHtml.SetHtmlContent(aTag);
            }

            return liTag;
        }

        private static TagBuilder PaginatorLi(string pageUrl, string glyph, bool asButtons)
        {
            TagBuilder liTag = new TagBuilder("li");
            liTag.TagRenderMode = TagRenderMode.Normal;
            TagBuilder spanTag = new TagBuilder("span");
            spanTag.TagRenderMode = TagRenderMode.Normal;
            spanTag.MergeAttribute("class", string.Format("fa fa-{0}", glyph));
            if (asButtons)
            {
                TagBuilder buttonTag = new TagBuilder("button");
                buttonTag.TagRenderMode = TagRenderMode.Normal;
                buttonTag.MergeAttribute("class", "page-link");
                buttonTag.MergeAttribute("type", "button");
                if (pageUrl == null)
                {
                    buttonTag.AddCssClass("disabled");
                    liTag.MergeAttribute("class", "page-item disabled");
                }
                else
                {
                    buttonTag.MergeAttribute("data-page", pageUrl);
                }
                buttonTag.InnerHtml.SetHtmlContent(spanTag);
                liTag.InnerHtml.SetHtmlContent(buttonTag);
            }
            else
            {
                TagBuilder aTag = new TagBuilder("a");
                aTag.TagRenderMode = TagRenderMode.Normal;
                if (pageUrl == null)
                {
                    liTag.MergeAttribute("class", "page-item disabled");
                    aTag.MergeAttribute("href", "#");
                    aTag.MergeAttribute("class", "page-link");
                    aTag.MergeAttribute("onclick", "return false;");
                }
                else
                {
                    liTag.MergeAttribute("class", "page-item");
                    aTag.MergeAttribute("class", "page-link");
                    aTag.MergeAttribute("href", pageUrl);
                }

                aTag.InnerHtml.SetHtmlContent(spanTag);
                liTag.InnerHtml.SetHtmlContent(aTag);
            }
            return liTag;
        }

        private static string QueryBuilder(IUrlHelper url, int? page, bool asButtons)
        {
            if (asButtons)
            {
                if (page.HasValue)
                {
                    return page.ToString();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var routeValues = GetNonPageRouteValues(url);
                routeValues.Add("page", page);
                return url.RouteUrl(routeValues);
            }
        }

        private static RouteValueDictionary GetNonPageRouteValues(IUrlHelper url)
        {
            var routeValues = new RouteValueDictionary();
            foreach (var query in url.ActionContext.HttpContext.Request.Query)
            {
                if (!(String.Equals(query.Key, "page", StringComparison.OrdinalIgnoreCase)))
                {
                    routeValues.Add(query.Key, query.Value);
                }
            }
            return routeValues;
        }
    }
}
