using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Filters
{
    public class ExternalResourceFilterAttribute : Attribute, IAsyncResourceFilter
    {
        private readonly ILogger<ExternalResourceFilterAttribute> _logger;
        private readonly IExternalResourceService _externalResourceService;

        public ExternalResourceFilterAttribute(ILogger<ExternalResourceFilterAttribute> logger,
            IExternalResourceService externalResourceService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _externalResourceService = externalResourceService
                ?? throw new ArgumentNullException(nameof(externalResourceService));
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            var css = await _externalResourceService.GetAllAsync(ExternalResourceType.CSS);
            var js = await _externalResourceService.GetAllAsync(ExternalResourceType.JS);

            context.HttpContext.Items[ItemKey.ExternalCSS] = css.Select(_ => _.Url).ToList();
            context.HttpContext.Items[ItemKey.ExternalJS] = js.Select(_ => _.Url).ToList();

            await next();
        }
    }
}