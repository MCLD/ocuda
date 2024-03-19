namespace Ocuda.Utility.Exceptions
{
    public class OcudaConfigurationException : OcudaException
    {
        public OcudaConfigurationException(string message) : base(message)
        {
        }

        public OcudaConfigurationException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        public OcudaConfigurationException()
        {
        }
    }
}