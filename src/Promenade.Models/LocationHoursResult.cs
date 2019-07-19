using System;

namespace Ocuda.Promenade.Models
{
    public class LocationHoursResult
    {
        public bool Open;
        public DateTime? OpenTime;
        public DateTime? CloseTime;

        public bool IsCurrentlyOpen;
        public string StatusMessage;
    }
}
