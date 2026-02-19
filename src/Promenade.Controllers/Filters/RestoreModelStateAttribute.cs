using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Ocuda.Utility.Filters;

namespace Ocuda.Promenade.Controllers.Filters
{
    public sealed class RestoreModelStateAttribute : RestoreModelStateAttributeBase
    {
        public const int TimeOutMinutes = 2;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context,
                                         ActionExecutionDelegate next)
        {
            ModelStateTimeOut = TimeOutMinutes;
            await base.OnActionExecutionAsync(context, next);
        }
    }
}
