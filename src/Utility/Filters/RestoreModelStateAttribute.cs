using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ocuda.Utility.Helpers;

namespace Ocuda.Utility.Filters
{
    public class RestoreModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var controller = context.Controller as Controller;

            var key = ModelStateHelpers.GetModelStateKey(context.RouteData.Values);

            var modelStateStorage = controller?.TempData[key] as string;

            if (modelStateStorage != null)
            {
                var storage = ModelStateHelpers.DeserializeModelState(modelStateStorage);
                var timeDifference = DateTimeOffset.Now.ToUnixTimeSeconds() - storage.time;
                if (TimeSpan.FromSeconds(timeDifference).Minutes < 2)
                {
                    //Only Import if we are viewing
                    if (context.Result is ViewResult)
                    {
                        context.ModelState.Merge(storage.modelState);
                    }
                }
            }

            base.OnActionExecuted(context);
        }
    }
}
