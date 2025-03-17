using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksByMail.Keys
{
    public struct Cache
    {
        /// <summary>
        /// The user's username, replace {0} with the user's identitifer.
        /// </summary>
        public static readonly string Username = "auth.{0}.username";
        /// <summary>
        /// The return URL once the user is authenticated, replace {0} with the user's identifier.
        /// </summary>
        public static readonly string Return = "auth.{0}.return";
    }
}
