using System;

namespace Ocuda.Utility.Abstract
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}