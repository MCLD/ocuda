using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Utility.Providers
{
    public class CurrentDateTimeProvider : Abstract.IDateTimeProvider
    {
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}
