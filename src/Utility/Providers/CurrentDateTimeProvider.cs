using System;

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
