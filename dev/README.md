# Ocuda Developer Documentation

## Framework

Ocuda targets the ASP.NET Core 2.2 framework. For that to work currently you must:

- Install the [.NET Core 2.2 SDK](https://dotnet.microsoft.com/download/dotnet-core/2.2)
- [Visual Studio 2019](https://visualstudio.microsoft.com/vs/)

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

To run you must create database migrations for the two databases. Currently the only available data provider is Microsoft SQL Server ("SqlServer").

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

1. `cd src/Ops.DataProvider.<provider>.Ops && dotnet ef migrations add -s ../Ops.Web/Ops.Web.Web.csproj --context Ocuda.Ops.DataProvider.<provider>.Ops.Context develop`
2. `cd ../Ops.DataProvider.<provider>.Promenade && dotnet ef migrations add -s ../Ops.Web/Ops.Web.csproj --context Ocuda.Ops.DataProvider.<provider>.Promenade.Context develop`

## Application settings
_Eventually this should be moved to the user documentation._

### Ops

#### Required
##### Connection strings
- `Ops`
- `Promenade`

##### Settings
- `Ops.DatabaseProvider` - which database provider to use, currently the only supported option is: `SqlServer`

#### Optional
##### Connection strings
- `SerilogSoftwareLogs` - SQL Server connection string for writing application logs out, if unset logs are not written to SQL Server

##### Settings
- `Ocuda.FileShared` - defaults to "shared" - a file location shared among any instances of the site which are running for handling site assets
- `Ocuda.UrlSharedContent` - defaults to "content" - a mapping to the public shared file directory (which is "public" under the configured `Ocuda.FileShared`)

- `OpsAuthBlankRequestRedirect` - *used only by Ops.Web.WindowsAuth* - if the authentication site is loaded with no id or directive then redirect to this URL
- `Ops.AuthRedirect` - URL to the deployed Ops.Web.WindowsAuth site - if not specified authentication will not function
- `Ops.AuthTimeoutMinutes` - defaults to 2 minutes - timeout for authentication bits (cookie and distributed cache elements)
- `Ops.Culture` - defaults to "en-US", the culture to use for displaying things like dates and times - for valid options see the language tags listed in the [Microsoft National Language Support (NLS) API Reference](http://go.microsoft.com/fwlink/?LinkId=200048)
- `Ops.DistributedCache` - when unset, defaults to memory-based distributed cache - a cache strategy to use: currently either unset or 'Redis' are valid
- `Ops.DistributedCacheInstanceDiscriminator` - if set, appends the string to distributed cache keys in order to isolate them from other instances (e.g. if multiple developers are using the same distributed cache)
- `Ops.DistributedCache.RedisConfiguration` - *also used by Ops.Web.WindowsAuth* - if *Ops.DistributedCache* is set to 'Redis' this must be set with Redis configuration information, see the [RedisCacheOptions.Configuration property](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.redis.rediscacheoptions.configuration)
- `Ops.DomainName` - an Active Directory domain name to remove from the beginning of authenticated users (do not include the slash)
- `Ops.HttpErrorFileTag` - if *Ops.RollingLogLocation* is set, this will write out http error logs in the same location but with the value of this setting in the filename
- `Ops.Instance` - configure an instance name for more specific logging
- `Ops.LDAPDN` - Distinguished name of an LDAP user for performing lookups, similar to "CN=Users Real Name,OU=Organizational Unit,DC=domain,DC=tld"
- `Ops.LDAPPassword` - Password for the LDAPDN user
- `Ops.LDAPPort` - Port to connect to for LDAP, defaults to 389
- `Ops.LDAPSearchBase` - Search base for querying usernames, similar to "OU=Users,OU=Organizational Unit,DC=domain,DC=tld"
- `Ops.LDAPServer` - LDAP server to query for information based on the user's username
- `Ops.RollingLogLocation` - path of where to write log files which rotate daily, if unset no rolling log is written
- `Ops.SessionTimeoutMinutes` - defaults to 2 hours - amount of time in minutes for sessions to last
- `Ops.SiteManagerGroup` - if specified, this authentication group (currently ADGroup) will be granted site manager access
- `Ops.SiteSettingCacheMinutes` - defaults to 60 minutes - how long to cache site setting values

### Promenade

#### Required
- `Promenade.DatabaseProvider` - which database provider to use, must currently be set to: `SqlServer`

#### Optional
- `Ocuda.FileShared` - defaults to "shared" - a file location shared among any instances of the site which are running for handling site assets
- `Ocuda.UrlSharedContent` - defaults to "content" - a mapping to the public shared file directory (which is "public" under the configured `Ocuda.FileShared`)

- `Promenade.Culture` - defaults to "en-US", the culture to use for displaying things like dates and times - for valid options see the language tags listed in the [Microsoft National Language Support (NLS) API Reference](http://go.microsoft.com/fwlink/?LinkId=200048)
