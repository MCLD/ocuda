using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ocuda.Utility.Helpers;

namespace Ocuda.Ops.Controllers.Filter
{
    public class SaveModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //Only export when ModelState is not valid
            if (!context.ModelState.IsValid)
            {
                //Export if we are redirecting
                if (context.Result is RedirectResult
                    || context.Result is RedirectToRouteResult
                    || context.Result is RedirectToActionResult)
                {
                    var controller = context.Controller as Controller;
                    if (controller != null && context.ModelState != null)
                    {
                        var key = ModelStateHelpers.GetModelStateKey(context.RouteData.Values);
                        controller.TempData[key] = ModelStateHelpers
                            .SerializeModelState(context.ModelState);
                    }
                }
            }

            base.OnActionExecuted(context);
        }
    }
}
