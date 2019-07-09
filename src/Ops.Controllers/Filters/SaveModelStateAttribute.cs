using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ocuda.Utility.Helpers;

namespace Ocuda.Ops.Controllers.Filters
{
    public class SaveModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //Only export when ModelState is not valid
            if (!context.ModelState.IsValid
                && (context.Result is RedirectResult
                    || context.Result is RedirectToRouteResult
                    || context.Result is RedirectToActionResult))
            {
                using (var controller = context.Controller as Controller)
                {
                    if (controller != null && context.ModelState != null)
                    {
                        var key = ModelStateHelper.GetModelStateKey(context.RouteData.Values);
                        controller.TempData[key] = ModelStateHelper
                            .SerializeModelState(context.ModelState);
                    }
                }
            }

            base.OnActionExecuted(context);
        }
    }
}
