namespace Ocuda.Utility.Exceptions
{
    public class OcudaEmailException : OcudaException
    {
        public OcudaEmailException(string message) : base(message)
        {
        }

        public OcudaEmailException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}