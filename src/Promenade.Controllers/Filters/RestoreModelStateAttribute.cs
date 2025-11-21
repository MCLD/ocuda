using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Helpers;

namespace Ocuda.Promenade.Controllers.Filters
{
    public class RestoreModelStateAttribute : ActionFilterAttribute
    {
        public string Key { get; set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context,
                                         ActionExecutionDelegate next)
        {
            var resultContext = await next();

            var controller = context.Controller as Controller;

            string modelStateKey;
            if (!string.IsNullOrWhiteSpace(Key))
            {
                modelStateKey = ModelStateHelper.GetModelStateKey(Key);
            }
            else
            {
                modelStateKey = ModelStateHelper.GetModelStateKey(context.RouteData.Values);
            }

            if (controller?.TempData[modelStateKey] is string modelStateStorage)
            {
                var (modelState, time) = ModelStateHelper.DeserializeModelState(modelStateStorage);
                var timeDifference = DateTimeOffset.Now.ToUnixTimeSeconds() - time;

                var modelstateTimeOut = 2;
                if (TimeSpan.FromSeconds(timeDifference).Minutes < modelstateTimeOut
                    || modelstateTimeOut < 1)
                {
                    //Only Import if we are viewing
                    if (resultContext.Result is ViewResult)
                    {
                        context.ModelState.Merge(modelState);
                    }
                }
                else
                {
                    var _logger = (ILogger<RestoreModelStateAttribute>)context.HttpContext
                        .RequestServices.GetService(typeof(ILogger<RestoreModelStateAttribute>));
                    _logger.LogError("ModelState timed out for key {ModelStateKey}",
                        modelStateKey);
                }
            }
        }
    }
}
