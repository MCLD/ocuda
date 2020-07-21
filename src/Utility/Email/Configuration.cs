namespace Ocuda.Utility.Email
{
    public class Configuration
    {
        public string BccAddress { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string OutgoingHost { get; set; }
        public string OutgoingLogin { get; set; }
        public string OutgoingPassword { get; set; }
        public int OutgoingPort { get; set; }
        public string OverrideToAddress { get; set; }
        public string RestrictToDomain { get; set; }
    }
}
