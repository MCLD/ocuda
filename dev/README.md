# Ocuda Developer Documentation

## Framework

Ocuda targets the ASP.NET Core 2.1 framework. For that to work currently you must:

- Install the [.NET Core 2.1 SDK](https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.300)
- Ensure you're using [Visual Studio Update 7](https://visualstudio.com/vs)

For more details see the ASP.NET Blog post about [ASP.NET Core 2.1.0](https://blogs.msdn.microsoft.com/webdev/2018/05/30/asp-net-core-2-1-0-now-available/).

## Project layout

- `Ocuda.Utility` - utility classes which are useful to both projects

Both projects have the some common layout items:

- `*.Web` - serves the application via the Web
- `*.Controllers` - ASP.NET MVC controllers and related items (ViewModels)
- `*.Models` - pass data around the application
- `*.Service` - operations performed by the controllers, contains logic
- `*.Data` - handles data storage and retrieval
- `*.DataProviders.*` - Entity Framework data providers to connect to persistence back-ends

### Ops

This is the administration portion of the application which also can function as an intranet.

### Promenade

This is the public-facing portion of the application.

## Initial build and run

To run you must create database migrations for the two databases. First, choose a data provider - currently either SQLite ("SQLite") or Microsoft SQL Server ("SqlServer").

### Visual Studio Package Manager Console

1. Select the "Default Project" of `Ops.DataProvider.<provider>.Ops` for your chosen data provider.
2. Enter `add-migration -Context Ocuda.Ops.DataProvider.<provider>.Ops.Context develop`.
3. Select the "Default Project" of `Ops.DataProvider.<provider>.Promenade` for your chosen data provider.
4. Enter `add-migration -Context Ocuda.Ops.DataProvider.<provider>.Promenade.Context develop`.

### Using dotnet command line tool

Use the following scripts:

- Add a migration with: `dev/add-migration.sh <provider> <name>`.
- Remove the latest migration with: `dev/remove-migration.sh <provider>`.

Or perform the steps manually, such as:

1. `Web.csproj --context Ocuda.Ops.DataProvider.<provider>.Ops.Context develop`
2. `cd ../Ops.DataProvider.<provider>.Promenade && dotnet ef migrations add -s ../Ops.Web/Ops.Web.csproj --context Ocuda.Ops.DataProvider.<provider>.Promenade.Context develop`

## Application settings
_Eventually this should be moved to the user documentation._

### Ops

#### Required
##### Connection strings
- `Ops`
- `Promenade`

##### Settings
- `Ops.DatabaseProvider` - which database provider to use, must be one of: `SQLite`, `SqlServer`

#### Optional
##### Connection strings
- `SerilogSoftwareLogs` - SQL Server connection string for writing application logs out, if unset logs are not written to SQL Server

##### Settings
- `OpsAuthBlankRequestRedirect` - *used only by Ops.Web.WindowsAuth* - if the authentication site is loaded with no id or directive then redirect to this URL
- `Ops.AuthRedirect` - URL to the deployed Ops.Web.WindowsAuth site - if not specified authentication will not function
- `Ops.AuthTimeoutMinutes` - defaults to 2 minutes - timeout for authentication bits (cookie and distributed cache elements)
- `Ops.Culture` - defaults to "en-US", the culture to use for displaying things like dates and times - for valid options see the language tags listed in the [Microsoft National Language Support (NLS) API Reference](http://go.microsoft.com/fwlink/?LinkId=200048)
- `Ops.DistributedCache` - when unset, defaults to memory-based distributed cache - a cache strategy to use: currently either unset or 'Redis' are valid
- `Ops.DistributedCache.RedisConfiguration` - *also used by Ops.Web.WindowsAuth* - if *Ops.DistributedCache* is set to 'Redis' this must be set with Redis configuration information, see the [RedisCacheOptions.Configuration property](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.redis.rediscacheoptions.configuration)
- `Ops.DomainName` - an Active Directory domain name to remove from the beginning of authenticated users (do not include the slash)
- `Ops.HttpErrorFileTag` - if *Ops.RollingLogLocation* is set, this will write out http error logs in the same location but with the value of this setting in the filename
- `Ops.Instance` - configure an instance name for more specific logging
- `Ops.RollingLogLocation` - path of where to write log files which rotate daily, if unset no rolling log is written
- `Ops.SessionTimeoutMinutes` - defaults to 2 hours - amount of time in minutes for sessions to last
- `Ops.SiteManagerGroup` - if specified, this authentication group (currently ADGroup) will be granted site manager access

### Promenade

#### Required
- `Promenade.DatabaseProvider` - which database provider to use, must be one of: `SQLite`, `SqlServer`

#### Optional
- `Promenade.Culture` - defaults to "en-US", the culture to use for displaying things like dates and times - for valid options see the language tags listed in the [Microsoft National Language Support (NLS) API Reference](http://go.microsoft.com/fwlink/?LinkId=200048)
