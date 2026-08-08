using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;

namespace Ocuda.Ops.Web.StartupHelper
{
    public class VerifyData(IApplicationBuilder applicationBuilder)
    {
        private readonly IApplicationBuilder app = applicationBuilder
            ?? throw new ArgumentNullException(nameof(applicationBuilder));

        public void EnsureRequiredDataIsPresent()
        {
            using var scope = app.ApplicationServices.CreateScope();

            var emediaService = scope.ServiceProvider.GetRequiredService<IEmediaService>();
            Task.Run(emediaService.EnsureSlugsAsync).Wait();

            var userManagementService
                = scope.ServiceProvider.GetRequiredService<IUserManagementService>();
            var adminUser = Task.Run(userManagementService.EnsureSysadminUserAsync).Result;

            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
            Task.Run(() => jobService.EnsureJobConfigurationsAsync(adminUser.Id)).Wait();
        }
    }

    public static class VerifyDataExtensions
    {
        public static IApplicationBuilder EnsureRequiredDataIsPresent(this IApplicationBuilder builder)
        {
            new VerifyData(builder).EnsureRequiredDataIsPresent();
            return builder;
        }
    }
}
