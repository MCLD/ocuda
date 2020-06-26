using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Utility.Email
{
    public class Details : Record
    {
        public string Password { get; set; }
        public int? Port { get; set; }

        [Required]
        public string Server { get; set; }
        public string Username { get; set; }

        public IDictionary<string, string> Tags { get; set; }
        public string TemplateHtml { get; set; }
        public string TemplateText { get; set; }
        public string Preview { get; set; }
        public string UrlParameters { get; set; }
    }
}
