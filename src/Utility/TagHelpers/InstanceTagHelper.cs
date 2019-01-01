using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace Ocuda.Utility.TagHelpers
{
    public class InstanceTagHelper : TagHelper
    {
        private readonly IConfiguration _config;
        public InstanceTagHelper(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        [HtmlAttributeName("name")]
        public string name { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            // Always strip the outer tag name as we never want <environment> to render
            output.TagName = null;

            if (string.IsNullOrEmpty(name) || _config["instance"] == null)
            {
                // No current instance name, do nothing
                output.SuppressOutput();
                return;
            }

            if (!name.Equals(_config["instance"], StringComparison.OrdinalIgnoreCase))
            {
                output.SuppressOutput();
            }
        }
    }
}
