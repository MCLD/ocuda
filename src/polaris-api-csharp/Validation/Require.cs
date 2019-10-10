using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api.Validation
{
    /// <summary>
    /// Allows us to require properties of parameter objects
    /// </summary>
    public class Require
    {
        /// <summary>
        /// Verify argument is provided
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void Argument(string name, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
