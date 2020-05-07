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

        /// <summary>
        /// Left navigation JSON
        /// </summary>
        public static readonly string OpsLeftNav = "leftnav";

        /// <summary>
        /// Default language ID
        /// </summary>
        public static readonly string PromDefaultLanguageId = "langid-default";

        /// <summary>
        /// Language id, {0} is the culture
        /// </summary>
        public static readonly string PromLanguageId = "langid.{0}";

        /// <summary>
        /// Cached navigation element, {0} is the id of the element, {1} is the language id
        /// </summary>
        public static readonly string PromNavLang = "nav.{0}.{1}";

        /// <summary>
        /// Cached page, {0} is the language id, {1} is the type, {2} is the stub
        /// </summary>
        public static readonly string PromPage = "page.{0}.{1}.{2}";

        /// <summary>
        /// Cached redirect path, {0} is the requested path
        /// </summary>
        public static readonly string PromRedirectPath = "redir.{0}";

        /// <summary>
        /// Cached site settings, {0} is the site setting key
        /// </summary>
        public static readonly string PromSiteSetting = "sitesetting.{0}";

        /// <summary>
        /// Cached external resource list
        /// </summary>
        public static readonly string PromExternalResources = "externalresources";

        /// <summary>
        /// Cached social card, {0} is the card id
        /// </summary>
        public static readonly string PromSocialCard = "socialcard.{0}";

        /// <summary>
        /// Cached segment, {0} is the segment id
        /// </summary>
        public static readonly string PromSegment = "segment.{0}";

        /// <summary>
        /// Cached segment text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromSegmentText = "segmenttext.{0}.{1}";

        /// <summary>
        /// Cached emedia groups
        /// </summary>
        public static readonly string PromEmediaGroups = "emediagroups";

        /// <summary>
        /// Cached emedia groups
        /// </summary>
        public static readonly string PromEmedias = "emedias";

        /// <summary>
        /// Cached emedia text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromEmediaText = "emediatext.{0}.{1}";

        /// <summary>
        /// Cached emedia categories
        /// </summary>
        public static readonly string PromCategories = "categories";

        /// <summary>
        /// Cached emedia text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromCategoryText = "categorytext.{0}.{1}";

        /// <summary>
        /// Cached emedia categories
        /// </summary>
        public static readonly string PromEmediaCategories = "emediacategories";

        /// <summary>
        /// Cached schedule subjects
        /// </summary>
        public static readonly string PromScheduleSubjects = "schedulesubjects";
    }
}
