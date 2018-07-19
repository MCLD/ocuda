using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ocuda.Ops.Controllers.Validators;

namespace Ocuda.Ops.Controllers.RouteConstraints
{
    public class SectionRouteConstraint : IRouteConstraint
    {
        private readonly ISectionPathValidator _sectionPathValidator;
        public SectionRouteConstraint(ISectionPathValidator sectionPathValidator)
        {
            _sectionPathValidator = sectionPathValidator
                ?? throw new ArgumentNullException(nameof(sectionPathValidator));
        }

        public bool Match(HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            var sectionPath = (string)values[routeKey];
            if (!string.IsNullOrWhiteSpace(sectionPath))
            {
                return _sectionPathValidator.IsValid(sectionPath);
            }
            return false;
        }
    }
}
