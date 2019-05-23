using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Helpers;

namespace Ocuda.Ops.Controllers.Filters
{
    public class RestoreModelStateAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context,
                                         ActionExecutionDelegate next)
        {
            var resultContext = await next();

            var controller = context.Controller as Controller;
            var key = ModelStateHelper.GetModelStateKey(context.RouteData.Values);

            var modelStateStorage = controller?.TempData[key] as string;
            if (modelStateStorage != null)
            {
                var storage = ModelStateHelper.DeserializeModelState(modelStateStorage);
                var timeDifference = DateTimeOffset.Now.ToUnixTimeSeconds() - storage.time;

                var _siteSettingService = (ISiteSettingService)context.HttpContext.RequestServices
                    .GetService(typeof(ISiteSettingService));
                var modelstateTimeOut = await _siteSettingService
                    .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ModelStateTimeoutMinutes);
                if (TimeSpan.FromSeconds(timeDifference).Minutes < modelstateTimeOut 
                    || modelstateTimeOut < 1)
                {
                    //Only Import if we are viewing
                    if (resultContext.Result is ViewResult)
                    {
                        context.ModelState.Merge(storage.modelState);
                    }
                }
                else
                {
                    var _logger = (ILogger<RestoreModelStateAttribute>)context.HttpContext
                        .RequestServices.GetService(typeof(ILogger<RestoreModelStateAttribute>));
                    _logger.LogError($"ModelState timed out for key {key}.");
                }
            }
        }
    }
}
