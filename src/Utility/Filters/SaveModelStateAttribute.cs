using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ocuda.Utility.Helpers;

namespace Ocuda.Utility.Filters
{
    /// <summary>
    /// Save model state items to temp data when redirecting after a model state error. Key can be
    /// set manually or generated from route values.
    /// </summary>
    public class SaveModelStateAttribute : ActionFilterAttribute
    {
        public string Key { get; set; }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //Only export when ModelState is not valid
            if (!context.ModelState.IsValid
                && (context.Result is RedirectResult
                    || context.Result is RedirectToRouteResult
                    || context.Result is RedirectToActionResult))
            {
                using var controller = context.Controller as Controller;
                if (controller != null && context.ModelState != null)
                {
                    string modelStateKey;
                    if (!string.IsNullOrWhiteSpace(Key))
                    {
                        modelStateKey = ModelStateHelper.GetModelStateKey(Key);
                    }
                    else
                    {
                        modelStateKey = ModelStateHelper.GetModelStateKey(context.RouteData.Values);
                    }

                    controller.TempData[modelStateKey] = ModelStateHelper
                        .SerializeModelState(context.ModelState);
                }
            }

            base.OnActionExecuted(context);
        }
    }
}
