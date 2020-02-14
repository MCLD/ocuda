namespace Ocuda.Utility.Keys
{
    public struct Configuration
    {
        public static readonly string OcudaErrorControllerName = "Ocuda.ErrorControllerName";
        public static readonly string OcudaFileShared = "Ocuda.FileShared";
        public static readonly string OcudaInstance = "Ocuda.Instance";
        public static readonly string OcudaLoggingDatabase = "Ocuda.LoggingDatabase";
        public static readonly string OcudaLoggingRollingFile = "Ocuda.LoggingRollingFile";
        public static readonly string OcudaLoggingRollingHttpFile = "Ocuda.LoggingRollingHttpFile";
        public static readonly string OcudaSeqEndpoint = "Ocuda.SeqEndpoint";
        public static readonly string OcudaSeqAPI = "Ocuda.API.Seq";
        public static readonly string OcudaUrlSharedContent = "Ocuda.UrlSharedContent";

        public static readonly string OcudaRuntimeRedisCacheConfiguration
            = "Ocuda.Runtime.RedisCacheConfiguration";

        public static readonly string OcudaRuntimeRedisCacheInstance
            = "Ocuda.Runtime.RedisCacheInstance";

        public static readonly string OcudaRuntimeSessionTimeout = "Ocuda.Runtime.SessionTimeout";

        public static readonly string OpsAPIGoogleMaps = "Ops.API.GoogleMaps";
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
        public static readonly string OpsHttpErrorFileTag = "Ops.HttpErrorFileTag";
        public static readonly string OpsLdapDn = "Ops.LDAPDN";
        public static readonly string OpsLdapPassword = "Ops.LDAPPassword";
        public static readonly string OpsLdapPort = "Ops.LDAPPort";
        public static readonly string OpsLdapSearchBase = "Ops.LDAPSearchBase";
        public static readonly string OpsLdapServer = "Ops.LDAPServer";
        public static readonly string OpsNavColumn = "Ops.NavColumn";
        public static readonly string OpsSessionTimeoutMinutes = "Ops.SessionTimeoutMinutes";
        public static readonly string OpsSiteManagerGroup = "Ops.SiteManagerGroup";
        public static readonly string OpsSiteSettingCacheMinutes = "Ops.SiteSettingCacheMinutes";

        public static readonly string PromenadeAPIGoogleMaps = "Promenade.API.GoogleMaps";
        public static readonly string PromenadeDatabaseProvider = "Promenade.DatabaseProvider";
        public static readonly string PromenadeDistributedCache = "Promenade.DistributedCache";

        public static readonly string PromenadeDistributedCacheInstanceDiscriminator
            = "Promenade.DistributedCacheInstanceDiscriminator";

        public static readonly string PromenadeDistributedCacheRedisConfiguration
            = "Promenade.DistributedCache.RedisConfiguration";

        public static readonly string PromenadeRequestLogging = "Promenade.RequestLogging";
    }
}
