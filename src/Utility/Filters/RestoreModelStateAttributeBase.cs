using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Helpers;

namespace Ocuda.Utility.Filters
{
    /// <summary>
    /// Load model state items from temp data and merge them into the model state when redirected to
    /// after a model state error. Key can be set manually or generated from route values.
    /// </summary>
    public abstract class RestoreModelStateAttributeBase : ActionFilterAttribute
    {
        public string Key { get; set; }
        public int ModelStateTimeOut { get; set; }

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

                if (TimeSpan.FromSeconds(timeDifference).Minutes < ModelStateTimeOut
                    || ModelStateTimeOut < 1)
                {
                    //Only Import if we are viewing
                    if (resultContext.Result is ViewResult)
                    {
                        context.ModelState.Merge(modelState);
                    }
                }
                else
                {
                    var _logger = (ILogger<RestoreModelStateAttributeBase>)context.HttpContext
                        .RequestServices.GetService(typeof(ILogger<RestoreModelStateAttributeBase>));
                    _logger.LogError("ModelState timed out for key {ModelStateKey}",
                        modelStateKey);
                }
            }
        }
    }
}
