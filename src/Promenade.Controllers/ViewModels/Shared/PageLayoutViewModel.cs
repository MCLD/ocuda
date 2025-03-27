using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Shared
{
    public class PageLayoutViewModel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1056:URI-like properties should not be strings",
            Justification = "URI-property is a string for database storage")]
        public string CanonicalUrl { get; set; }

        public bool HasCarousels { get; set; }
        public ImageFeatureTemplate PageFeatureTemplate { get; set; }
        public string PageHeaderClasses { get; set; }
        public PageLayout PageLayout { get; set; }
        public string Stub { get; set; }

        public static string CardTitleClasses(CardDetail cardDetail)
        {
            var sb = new StringBuilder("card-title h5 ");
            if (string.IsNullOrEmpty(cardDetail?.Text))
            {
                sb.Append("mb-0");
            }
            return sb.ToString()?.Trim();
        }

        public static string DataButtonFields(IEnumerable<CarouselButton> buttons)
        {
            if (buttons?.Any() != true)
            {
                return string.Empty;
            }
            var output = new StringBuilder();
            foreach (var button in buttons)
            {
                output.AppendFormat(CultureInfo.InvariantCulture, "data-button-{0}='{1}' ",
                    button.Order,
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Sort = button.Order,
                        Button = button.LabelText.Text,
                        Link = button.Url
                    }));
            }
            return output.ToString().Trim();
        }
    }
}