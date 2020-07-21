using System.ComponentModel.DataAnnotations;

namespace Ocuda.Utility.Email
{
    public class Record
    {
        public Record() { }

        public Record(Record incoming)
        {
            if (incoming != null)
            {
                BccEmailAddress = incoming.BccEmailAddress;
                BodyHtml = incoming.BodyHtml;
                BodyText = incoming.BodyText;
                FromEmailAddress = incoming.FromEmailAddress;
                FromName = incoming.FromName;
                OverrideEmailToAddress = incoming.OverrideEmailToAddress;
                RestrictToDomain = incoming.RestrictToDomain;
                SentResponse = incoming.SentResponse;
                Subject = incoming.Subject;
                ToEmailAddress = incoming.ToEmailAddress;
                ToName = incoming.ToName;
            }
        }

        [Required]
        public string Subject { get; set; }
        public string OverrideEmailToAddress { get; set; }
        public string RestrictToDomain { get; set; }
        public string BccEmailAddress { get; set; }

        [Required]
        public string FromEmailAddress { get; set; }

        [Required]
        public string FromName { get; set; }
        [Required]
        public string ToEmailAddress { get; set; }
        [Required]
        public string ToName { get; set; }

        [Required]
        public string BodyText { get; set; }

        [Required]
        public string BodyHtml { get; set; }

        public string SentResponse { get; set; }
    }
}
