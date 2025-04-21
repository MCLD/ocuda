using System;
using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Ocuda.Utility.Models;

namespace Ocuda.Utility.TagHelpers
{
    [HtmlTargetElement("nav", Attributes = "paginate")]
    public class NavPaginateTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public NavPaginateTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory ??
                throw new ArgumentNullException(nameof(urlHelperFactory));
        }

        [HtmlAttributeName("asButtons")]
        public bool AsButtons { get; set; }

        [HtmlAttributeName("paginate")]
        public PaginateModel PaginateModel { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContextData { get; set; }

        public static TagBuilder PaginatorInput(PaginateModel model)
        {
            if (model == null) { throw new ArgumentNullException(nameof(model)); }

            var liTag = new TagBuilder("li");
            liTag.MergeAttribute("class", "page-item");

            var formTag = new TagBuilder("form");
            formTag.MergeAttribute("role", "form");
            formTag.MergeAttribute("method", "get");
            formTag.MergeAttribute("aria-hidden", "true");
            formTag.TagRenderMode = TagRenderMode.Normal;

            var inputTag = new TagBuilder("input");
            inputTag.MergeAttribute("name", "page");
            inputTag.MergeAttribute("type", "number");
            inputTag.MergeAttribute("min", "1");
            inputTag.MergeAttribute("max", model.MaxPage.ToString(CultureInfo.InvariantCulture));
            inputTag.MergeAttribute("class", "page-link page-input");
            inputTag.MergeAttribute("value",
                model.CurrentPage.ToString(CultureInfo.InvariantCulture));
            inputTag.MergeAttribute("aria-label",
                $"Current page, Page {model.CurrentPage} of {model.MaxPage}");
            inputTag.TagRenderMode = TagRenderMode.Normal;

            formTag.InnerHtml.AppendHtml(inputTag);

            liTag.InnerHtml.SetHtmlContent(formTag);

            return liTag;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null) { throw new ArgumentNullException(nameof(context)); }
            if (output == null) { throw new ArgumentNullException(nameof(output)); }

            if (PaginateModel == null || PaginateModel.MaxPage < 2)
            {
                output.SuppressOutput();
                return;
            }

            IUrlHelper url = _urlHelperFactory.GetUrlHelper(ViewContextData);
            var ulTag = new TagBuilder("ul");
            ulTag.MergeAttribute("class", "pagination");

            string firstPage = PaginateModel.FirstPage == null
                               ? null
                               : QueryBuilder(url, PaginateModel.FirstPage, AsButtons);
            ulTag.InnerHtml.AppendHtml(PaginatorLi(firstPage, "fast-backward", AsButtons));

            string previousPage = PaginateModel.PreviousPage == null
                                  ? null
                                  : QueryBuilder(url, PaginateModel.PreviousPage, AsButtons);
            ulTag.InnerHtml.AppendHtml(PaginatorLi(previousPage, "backward", AsButtons));

            ulTag.InnerHtml.AppendHtml(PaginatorInput(PaginateModel));

            ulTag.InnerHtml.AppendHtml(PaginatorLi($"of {PaginateModel.MaxPage}", AsButtons));

            string nextPage = PaginateModel.NextPage == null
                              ? null
                              : QueryBuilder(url, PaginateModel.NextPage, AsButtons);

            ulTag.InnerHtml.AppendHtml(PaginatorLi(nextPage, "forward", AsButtons));

            string lastPage = PaginateModel.LastPage == null
                              ? null
                              : QueryBuilder(url, PaginateModel.LastPage, AsButtons);

            ulTag.InnerHtml.AppendHtml(PaginatorLi(lastPage, "fast-forward", AsButtons));

            output.TagName = "nav";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetHtmlContent(ulTag);
        }

        private static TagBuilder PaginatorLi(string text, bool asButtons)
        {
            var liTag = new TagBuilder("li")
            {
                TagRenderMode = TagRenderMode.Normal
            };
            liTag.MergeAttribute("class", "page-item disabled");

            if (asButtons)
            {
                var buttonTag = new TagBuilder("button")
                {
                    TagRenderMode = TagRenderMode.Normal
                };
                buttonTag.InnerHtml.SetHtmlContent(text);
                buttonTag.MergeAttribute("class", "page-link disabled");
                buttonTag.MergeAttribute("type", "button");
                liTag.InnerHtml.SetHtmlContent(buttonTag);
            }
            else
            {
                var aTag = new TagBuilder("a");
                aTag.MergeAttribute("href", "#");
                aTag.MergeAttribute("class", "page-link");
                aTag.MergeAttribute("onclick", "return false;");
                aTag.MergeAttribute("aria-hidden", "true");
                aTag.MergeAttribute("tabindex", "-1");
                aTag.InnerHtml.SetHtmlContent(text);
                aTag.TagRenderMode = TagRenderMode.Normal;
                liTag.InnerHtml.SetHtmlContent(aTag);
            }

            return liTag;
        }

        private static TagBuilder PaginatorLi(string pageUrl, string glyph, bool asButtons)
        {
            var liTag = new TagBuilder("li")
            {
                TagRenderMode = TagRenderMode.Normal
            };
            var spanTag = new TagBuilder("span")
            {
                TagRenderMode = TagRenderMode.Normal
            };
            spanTag.MergeAttribute("class", $"fa-solid fa-{glyph}");
            if (asButtons)
            {
                var buttonTag = new TagBuilder("button")
                {
                    TagRenderMode = TagRenderMode.Normal
                };
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
                var aTag = new TagBuilder("a")
                {
                    TagRenderMode = TagRenderMode.Normal
                };
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

                if (glyph.Equals("fast-backward", StringComparison.OrdinalIgnoreCase))
                {
                    aTag.MergeAttribute("aria-label", "Go to first page.");
                }
                else if (glyph.Equals("backward", StringComparison.OrdinalIgnoreCase))
                {
                    aTag.MergeAttribute("aria-label", "Go to previous page.");
                }
                else if (glyph.Equals("forward", StringComparison.OrdinalIgnoreCase))
                {
                    aTag.MergeAttribute("aria-label", "Go to next page.");
                }
                else if (glyph.Equals("fast-forward", StringComparison.OrdinalIgnoreCase))
                {
                    aTag.MergeAttribute("aria-label", "Go to last page.");
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
                return page?.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                var routeValues = new RouteValueDictionary();
                foreach (var query in url.ActionContext.HttpContext.Request.Query)
                {
                    if (!string.Equals(query.Key, "page", StringComparison.OrdinalIgnoreCase))
                    {
                        routeValues.Add(query.Key, query.Value);
                    }
                }
                routeValues.Add("page", page);
                return url.RouteUrl(routeValues);
            }
        }
    }
}
