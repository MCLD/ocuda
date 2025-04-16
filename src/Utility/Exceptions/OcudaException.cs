using System;

namespace Ocuda.Utility.Exceptions
{
    public class OcudaException : Exception
    {
        public OcudaException()
        { }

        public OcudaException(string message) : base(message)
        {
        }

        public OcudaException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}