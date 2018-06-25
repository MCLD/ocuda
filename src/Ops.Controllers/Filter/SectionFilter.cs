using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service;

namespace Ocuda.Ops.Controllers.Filter
{
    public class SectionFilter : Attribute, IAsyncResourceFilter
    {
        private readonly ILogger<SectionFilter> _logger;
        private readonly SectionService _sectionService;

        public SectionFilter(ILogger<SectionFilter> logger, SectionService sectionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            httpContext.Items[ItemKey.Sections] = await _sectionService.GetNavigationAsync();

            await next();
        }
    }
}
