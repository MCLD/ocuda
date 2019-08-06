namespace Ocuda.Utility.Keys
{
    public struct Configuration
    {
        public static readonly string OcudaErrorControllerName = "Ocuda.ErrorControllerName";
        public static readonly string OcudaInstance = "Ocuda.Instance";
        public static readonly string OcudaLoggingDatabase = "Ocuda.LoggingDatabase";
        public static readonly string OcudaLoggingRollingFile = "Ocuda.LoggingRollingFile";
        public static readonly string OcudaLoggingRollingHttpFile = "Ocuda.LoggingRollingHttpFile";

        public static readonly string ThrowQueryWarningsInDev = "ThrowQueryWarningsInDev";

        public static readonly string OpsAuthBlankRequestRedirect = "Ops.AuthBlankRequestRedirect";
        public static readonly string OpsAuthRedirect = "Ops.AuthRedirect";
        public static readonly string OpsAuthTimeoutMinutes = "Ops.AuthTimeoutMinutes";
        public static readonly string OpsCulture = "Ops.Culture";
        public static readonly string OpsDatabaseProvider = "Ops.DatabaseProvider";
        public static readonly string OpsDistributedCache = "Ops.DistributedCache";

        public static readonly string OpsDistributedCacheInstanceDiscriminator
            = "Ops.DistributedCacheInstanceDiscriminator";

        public static readonly string OpsDistributedCacheRedisConfiguration
            = "Ops.DistributedCache.RedisConfiguration";

        public static readonly string OpsDomainName = "Ops.DomainName";
        public static readonly string OpsFileShared = "Ops.FileShared";
        public static readonly string OpsHttpErrorFileTag = "Ops.HttpErrorFileTag";
        public static readonly string OpsLdapDn = "Ops.LDAPDN";
        public static readonly string OpsLdapPassword = "Ops.LDAPPassword";
        public static readonly string OpsLdapPort = "Ops.LDAPPort";
        public static readonly string OpsLdapSearchBase = "Ops.LDAPSearchBase";
        public static readonly string OpsLdapServer = "Ops.LDAPServer";
        public static readonly string OpsNavColumn = "Ops.NavColumn";
        public static readonly string OpsSiteManagerGroup = "Ops.SiteManagerGroup";
        public static readonly string OpsSiteSettingCacheMinutes = "Ops.SiteSettingCacheMinutes";
        public static readonly string OpsSessionTimeoutMinutes = "Ops.SessionTimeoutMinutes";
        public static readonly string OpsUrlSharedContent = "Ops.UrlSharedContent";

        public static readonly string PromAPIGoogleMaps = "Promenade.API.GoogleMaps";
    }
}
