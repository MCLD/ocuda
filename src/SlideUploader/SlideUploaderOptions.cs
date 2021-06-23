namespace Ocuda.SlideUploader
{
    internal class SlideUploaderOptions
    {
        public const string SlideUploader = "SlideUploader";
        public string AuthBase { get; set; }
        public string Instance { get; set; }
        public string JobFile { get; set; }
        public string JobResultFile { get; set; }
        public string OpsBase { get; set; }
    }
}