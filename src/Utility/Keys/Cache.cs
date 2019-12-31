namespace Ocuda.Utility.Keys
{
    public struct Cache
    {
        /// <summary>
        /// The user's username, replace {0} with the user's identitifer.
        /// </summary>
        public static readonly string OpsUsername = "auth.{0}.username";

        /// <summary>
        /// The return URL once the user is authenticated, replace {0} with the user's identifier.
        /// </summary>
        /// 
        public static readonly string OpsReturn = "auth.{0}.return";

        /// <summary>
        /// A user's groups as provided by the authentication system. Replace {0} with the user's
        /// identifier and {1} with a number starting with 1. Once the return is empty you have
        /// enumerated all of the available groups.
        /// </summary>
        public static readonly string OpsGroup = "auth.{0}.group{1}";

        /// <summary>
        /// Cached site settings, {0} is the site setting key
        /// </summary>
        public static readonly string OpsSiteSetting = "sitesetting.{0}";
    }
}
