using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Filters;

namespace Ocuda.Ops.Controllers.Filters
{
    public sealed class RestoreModelStateAttribute : RestoreModelStateAttributeBase
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context,
                                         ActionExecutionDelegate next)
        {
            var _siteSettingService = (ISiteSettingService)context.HttpContext.RequestServices
                   .GetService(typeof(ISiteSettingService));
            ModelStateTimeOut = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ModelStateTimeoutMinutes);
            
            await base.OnActionExecutionAsync(context, next);
        }
    }
}
