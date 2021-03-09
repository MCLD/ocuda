namespace Ocuda.Utility.Keys
{
    public struct Cache
    {
        /// <summary>
        /// Status of a digital display, {0} is the digital display id
        /// </summary>
        public static readonly string OpsDigitalDisplayStatus = "dd.{0}";

        /// <summary>
        /// Date and time of last digital display status, {0} is the digital display id
        /// </summary>
        public static readonly string OpsDigitalDisplayStatusAt = "dd.at.{0}";

        /// <summary>
        /// Cached email setup element, {0} is the id of the element, {1} is the language id
        /// </summary>
        public static readonly string OpsEmailSetup = "emailsetup.{0}.{1}";

        /// <summary>
        /// Cached email template element, {0} is the id of the element, {1} is the language id
        /// </summary>
        public static readonly string OpsEmailTemplate = "emailtemplate.{0}.{1}";

        /// <summary>
        /// A user's groups as provided by the authentication system. Replace {0} with the user's
        /// identifier and {1} with a number starting with 1. Once the return is empty you have
        /// enumerated all of the available groups.
        /// </summary>
        public static readonly string OpsGroup = "auth.{0}.group{1}";

        /// <summary>
        /// Instance that runs jobs and how often it runs them
        /// </summary>
        public static readonly string OpsJobRunner = "jobrunner";

        /// <summary>
        /// Send message to stop the job runner, replace {0} with the instance name
        /// </summary>
        public static readonly string OpsJobStop = "jobstop.{0}";

        /// <summary>
        /// Left navigation JSON
        /// </summary>
        public static readonly string OpsLeftNav = "leftnav";

        /// <summary>
        /// The return URL once the user is authenticated, replace {0} with the user's identifier.
        /// </summary>
        ///
        public static readonly string OpsReturn = "auth.{0}.return";

        /// <summary>
        /// Cached site settings, {0} is the site setting key
        /// </summary>
        public static readonly string OpsSiteSetting = "sitesetting.{0}";

        /// <summary>
        /// The user's username, replace {0} with the user's identitifer.
        /// </summary>
        public static readonly string OpsUsername = "auth.{0}.username";

        /// <summary>
        /// Cached carousel, {0} is the carousel id
        /// </summary>
        public static readonly string PromCarousel = "carousel.{0}";

        /// <summary>
        /// Cached carousel button label text, {0} is the language id, {1} is the label id
        /// </summary>
        public static readonly string PromCarouselButtonLabelText = "carouselbuttonlabeltext.{0}.{1}";

        /// <summary>
        /// Cached carousel item for page header, {0} is the carousel item id, {1} is the
        /// page layout id
        /// </summary>
        public static readonly string PromCarouselItemForPageLayout = "carousel.{0}.{1}";

        /// <summary>
        /// Cached carousel item text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromCarouselItemText = "carouselitemtext.{0}.{1}";

        /// <summary>
        /// Cached carousel text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromCarouselText = "carouseltext.{0}.{1}";

        /// <summary>
        /// Cached emedia categories
        /// </summary>
        public static readonly string PromCategories = "categories";

        /// <summary>
        /// Cached emedia text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromCategoryText = "categorytext.{0}.{1}";

        /// <summary>
        /// Default language ID
        /// </summary>
        public static readonly string PromDefaultLanguageId = "langid-default";

        /// <summary>
        /// Cached emedia categories
        /// </summary>
        public static readonly string PromEmediaCategories = "emediacategories";

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
        /// Cached external resource list
        /// </summary>
        public static readonly string PromExternalResources = "externalresources";

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
        /// Cached current page layout id from header, {0} is the header id
        /// </summary>
        public static readonly string PromPageCurrentLayoutId = "currentpagelayoutid.{0}";

        /// <summary>
        /// Cached page header info, {0} is the stub, {1} is the page type
        /// </summary>
        public static readonly string PromPageHeader = "pageheader.{0}.{1}";

        /// <summary>
        /// Cached page layout, {0} is the layout id
        /// </summary>
        public static readonly string PromPageLayout = "pagelayout.{0}";

        /// <summary>
        /// Cached page layout text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromPageLayoutText = "pagelayouttext.{0}.{1}";

        /// <summary>
        /// Cached podcast info, {0} is the podcast stub, {1} is whether to include blocked items
        /// </summary>
        public static readonly string PromPodcast = "podcast.{0}.{1}";

        /// <summary>
        /// Cached podcast items, {0} is the podcast id, {1} is whether to include blocked items
        /// </summary>
        public static readonly string PromPodcastItems = "podcastitems.{0}.{1}";

        /// <summary>
        /// Cached redirect path, {0} is the requested path
        /// </summary>
        public static readonly string PromRedirectPath = "redir.{0}";

        /// <summary>
        /// Cached schedule subjects
        /// </summary>
        public static readonly string PromScheduleSubjects = "schedulesubjects";

        /// <summary>
        /// Cached segment, {0} is the segment id
        /// </summary>
        public static readonly string PromSegment = "segment.{0}";

        /// <summary>
        /// Cached segment text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromSegmentText = "segmenttext.{0}.{1}";

        /// <summary>
        /// Cached site settings, {0} is the site setting key
        /// </summary>
        public static readonly string PromSiteSetting = "sitesetting.{0}";

        /// <summary>
        /// Cached social card, {0} is the card id
        /// </summary>
        public static readonly string PromSocialCard = "socialcard.{0}";
    }
}