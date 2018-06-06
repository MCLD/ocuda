using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ops.Service;

namespace Ocuda.Ops.Controllers.Filter
{
    public class OpsFilter : Attribute, IAsyncResourceFilter
    {
        private readonly ILogger<OpsFilter> _logger;
        private readonly SectionService _sectionService;

        public OpsFilter(ILogger<OpsFilter> logger, SectionService sectionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            httpContext.Items[ItemKey.Sections] = _sectionService.GetAll();

            await next();
        }
    }
}
