using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

namespace Ocuda.Promenade.Web
{
    public class DbLoggingInterceptor : DbCommandInterceptor
    {
        private ILogger _logger;

        public DbLoggingInterceptor()
        {
            _logger = Log.Logger.ForContext(GetType());
        }

        public override Task<InterceptionResult<DbDataReader>>
            ReaderExecutingAsync(DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result,
                CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(command?.CommandText))
            {
                _logger.Debug(command.CommandText);
            }
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        //public override Task<DbDataReader> ReaderExecutedAsync(DbCommand command,
        //    CommandExecutedEventData eventData,
        //    DbDataReader result,
        //    CancellationToken cancellationToken = default)
        //{
        //    return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        //}
    }
}
