using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Utility.Email
{
    public class Details : Record
    {
        public Details()
        {
            Cc = new Dictionary<string, string>();
            Tags = new Dictionary<string, string>();
        }

        public Details(IDictionary<string, string> tags)
        {
            Cc = new Dictionary<string, string>();
            if (tags != null)
            {
                Tags = tags;
            }
            else
            {
                Tags = new Dictionary<string, string>();
            }
        }

        public IDictionary<string, string> Cc { get; }
        public string Password { get; set; }
        public int? Port { get; set; }

        public string Preview { get; set; }

        [Required]
        public string Server { get; set; }

        public IDictionary<string, string> Tags { get; }
        public string TemplateHtml { get; set; }
        public string TemplateText { get; set; }
        public string UrlParameters { get; set; }
        public string Username { get; set; }
    }
}