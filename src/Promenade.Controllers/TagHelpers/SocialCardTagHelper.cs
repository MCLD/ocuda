using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.TagHelpers
{
    [HtmlTargetElement("socialcard", Attributes = attributeName)]
    public class SocialCardTagHelper : TagHelper
    {
        private const string attributeName = "card";
        private const string ogDescription = "og:Description";
        private const string ogImage = "og:Image";
        private const string ogImageAlt = "og:ImageAlt";
        private const string ogTitle = "og:Title";
        private const string ogType = "og:Type";
        private const string ogTypeValue = "website";
        private const string ogUrl = "og:Url";
        private const string twitterCard = "twitter:card";
        private const string twitterCardValue = "summary_large_image";
        private const string twitterSite = "twitter:site";

        [HtmlAttributeName(attributeName)]
        public SocialCard Card { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (output == null)
            {
                throw new System.ArgumentNullException(nameof(output));
            }

            output.TagName = string.Empty;

            if (Card != null)
            {
                output.Content.AppendHtml(MetaProperty(ogType, ogTypeValue));
                output.Content.AppendHtml(MetaProperty(ogTitle, Card.Title));
                output.Content.AppendHtml(MetaProperty(ogDescription, Card.Description));
                output.Content.AppendHtml(MetaProperty(ogUrl, Card.Url));
                output.Content.AppendHtml(MetaProperty(ogImage, Card.Image));

                if (!string.IsNullOrWhiteSpace(Card.ImageAlt))
                {
                    output.Content.AppendHtml(MetaProperty(ogImageAlt, Card.ImageAlt));
                }

                output.Content.AppendHtml(MetaName(twitterCard, twitterCardValue));

                if (!string.IsNullOrWhiteSpace(Card.TwitterSite))
                {
                    output.Content.AppendHtml(MetaName(twitterSite, Card.TwitterSite));
                }
            }
        }

        private TagBuilder MetaName(string name, string content)
        {
            var metaTag = new TagBuilder("meta")
            {
                TagRenderMode = TagRenderMode.SelfClosing
            };
            metaTag.Attributes.Add("name", name);
            metaTag.Attributes.Add("content", content);
            return metaTag;
        }

        private TagBuilder MetaProperty(string property, string content)
        {
            var metaTag = new TagBuilder("meta")
            {
                TagRenderMode = TagRenderMode.SelfClosing
            };
            metaTag.Attributes.Add("property", property);
            metaTag.Attributes.Add("content", content);
            return metaTag;
        }
    }
}
