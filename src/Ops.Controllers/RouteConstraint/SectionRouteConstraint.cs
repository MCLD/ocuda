using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Ocuda.Ops.Controllers.RouteConstraint
{
    public class SectionRouteConstraint : IRouteConstraint
    {
        public SectionRouteConstraint()
        {
        }

        public bool Match(HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            var site = (string)values[routeKey];
            if (site == "HumanResources" || site == "Communications")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
