using Microsoft.AspNetCore.Http;

namespace Ocuda.Utility.Helpers
{
    public class WebHelper
    {
        public string GetCurrentUrl(HttpContext context)
        {
            return $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
        }
    }
}