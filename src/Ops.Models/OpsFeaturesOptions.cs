namespace Ocuda.Ops.Models
{
    public class OpsFeaturesOptions
    {
        public bool AddressLookupConfigured
        {
            get
            {
                return AddressLookupTrestleConfigured || AddressLookupMaricopaCountyConfigured;
            }
        }

        public bool AddressLookupMaricopaCountyConfigured { get; set; }
        public bool AddressLookupTrestleConfigured { get; set; }
        public bool PolarisHelperConfigured { get; set; }
    }
}