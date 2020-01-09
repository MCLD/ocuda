using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Web.StartupHelper
{
    public class DataContext
    {
        private readonly IApplicationBuilder _app;
        private ILogger _logger;

        public DataContext(IApplicationBuilder applicationBuilder)
        {
            _app = applicationBuilder
                ?? throw new ArgumentNullException(nameof(applicationBuilder));
        }

        public void InitialSetup()
        {
            using var scope = _app.ApplicationServices.CreateScope();
            // migration
            _logger = scope.ServiceProvider.GetRequiredService<ILogger<DataContext>>();
            using (var context = scope.ServiceProvider.GetRequiredService<OpsContext>())
            {
                MigrateContext(context, "OpsContext");
            }

            using (var context = scope.ServiceProvider.GetRequiredService<PromenadeContext>())
            {
                MigrateContext(context, "PromenadeContext");
            }

            // verify initial setup data is accurate
            var initialSetup = scope.ServiceProvider.GetRequiredService<IInitialSetupService>();
            Task.Run(() => initialSetup.VerifyInitialSetupAsync()).Wait();
        }

        private void MigrateContext(IMigratableContext context, string contextName)
        {
            bool hasMigrations;
            try
            {
                var pending = context.GetPendingMigrationList();
                hasMigrations = pending != null && pending.Any();
                if (hasMigrations)
                {
                    _logger.LogWarning("Applying {MigrationsCount} db migrations for {ContextName}, last is: {LastMigration}",
                        pending.Count(),
                        contextName,
                        pending.Last());
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,
                    "Error looking up migrations for {ContextName}: {Message}",
                    contextName,
                    ex.Message);
                throw;
            }
            try
            {
                if (hasMigrations)
                {
                    context.Migrate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,
                    "Error performing migrations for {ContextName}: {Message}",
                    contextName,
                    ex.Message);
                throw;
            }
        }
    }

    public static class DataContextExtensions
    {
        public static IApplicationBuilder InitialSetup(this IApplicationBuilder builder)
        {
            new DataContext(builder).InitialSetup();
            return builder;
        }
    }
}
