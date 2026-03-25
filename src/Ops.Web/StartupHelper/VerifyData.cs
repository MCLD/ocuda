using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;

namespace Ocuda.Ops.Web.StartupHelper
{
    public static class VerifyDataExtensions
    {
        public static IApplicationBuilder EnsureRequiredDataIsPresent(this IApplicationBuilder builder)
        {
            new VerifyData(builder).EnsureRequiredDataIsPresent();
            return builder;
        }
    }

    public class VerifyData(IApplicationBuilder applicationBuilder)
    {
        private readonly IApplicationBuilder _app = applicationBuilder
            ?? throw new ArgumentNullException(nameof(applicationBuilder));

        public void EnsureRequiredDataIsPresent()
        {
            using var scope = _app.ApplicationServices.CreateScope();
            var emediaService = scope.ServiceProvider.GetRequiredService<IEmediaService>();
            Task.Run(emediaService.EnsureSlugsAsync).Wait();
        }
    }
}