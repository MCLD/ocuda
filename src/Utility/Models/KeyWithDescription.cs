namespace Ocuda.Utility.Models
{
    public class KeyWithDescription
    {
        public KeyWithDescription(string key, string description)
        {
            Key = key;
            Description = description;
        }

        public string Description { get; set; }
        public string Key { get; set; }
    }
}