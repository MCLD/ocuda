namespace Ocuda.Ops.Controllers.ViewModels.AddressVerification
{
    public class IndexViewModel
    {
        public bool AddressLookupConfigured { get; set; }
        public bool BarcodeLookupConfigured { get; set; }
        public string Link { get; set; }
        public string LinkText { get; set; }
    }
}