using System;
using MaricopaCountyAssessorHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocuda.Ops.Models;
using Ocuda.PolarisHelper;
using TrestleHelper;

namespace Ocuda.Ops.Web.StartupHelper
{
    public static class VerifyFeatureExtensions
    {
        public static IApplicationBuilder VerifyFeatures(this IApplicationBuilder builder)
        {
            new VerifyFeatures(builder).VerifyFeatureConfiguration();
            return builder;
        }
    }

    public class VerifyFeatures(IApplicationBuilder builder)
    {
        private readonly IApplicationBuilder _app = builder
            ?? throw new ArgumentNullException(nameof(builder));

        public void VerifyFeatureConfiguration()
        {
            using var scope = _app.ApplicationServices.CreateScope();

            var features = scope.ServiceProvider.GetRequiredService<IOptions<OpsFeaturesOptions>>();

            var mcLookup = scope.ServiceProvider.GetRequiredService<MaricopaCountyAssessorClient>();
            features.Value.AddressLookupMaricopaCountyConfigured = mcLookup.IsConfigured;

            var polarisHelper = scope.ServiceProvider.GetRequiredService<IPolarisHelper>();
            features.Value.PolarisHelperConfigured = polarisHelper.IsConfigured;

            var trestleLookup = scope.ServiceProvider.GetRequiredService<TrestleClient>();
            features.Value.AddressLookupTrestleConfigured = trestleLookup.IsConfigured;
        }
    }
}