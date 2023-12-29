using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Feature
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(48)]
        [Required]
        public string Icon { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [MaxLength(80)]
        public string Stub { get; set; }

        [MaxLength(2000)]
        public string BodyText { get; set; }

        [MaxLength(5)]
        public string IconText { get; set; }

        public int? SortOrder { get; set; }

        [NotMapped]
        public bool IsNewFeature { get; set; }

        [NotMapped]
        public bool NeedsPopup { get; set; }

        [NotMapped]
        private static readonly Dictionary<string, string> LocationFeatureNames = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "ipads", "📱 iPads" },
            { "binoculars", "🐦 Binoculars" },
            { "citizen science", "🔬 Citizen Science" },
            { "telescopes", "🔭 Telescopes" },
            { "charging station", "🔌 Charging Station" },
            { "teen leadership club", "🏢 Teen Leadership Club" },
            { "friends", "😊 Friends" },
            { "reserve a room", "🗓️ Reserve A Room" },
            { "innovation hub", "🧪 Innovation Hub" },
            { "award-winning", "🏆 Award-Winning" },
            { "recorder kiosk", "Recorder Kiosk"},
            { "smart table", "SMART Table" },
            { "laptops & hotspots", "💻 Laptops & HotSpots" },
            { "ukuleles", "🎵 Ukuleles" },
            { "volunteer", "🙋 Volunteer" }
        };
        [NotMapped]
        private static readonly Dictionary<string, string> LocationServiceNames = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "7-day express", "📚 7-Day Express" },
            { "low lision resource center", "👓 Low Vision Resource Center" },
            { "seed library", "🌿 Seed Library" },
            { "solar status", "☀️ Solar Status" },
            { "leed gold", "LEED Gold" },
            { "leed platinum", "LEED Platinum" },
            { "printing", "🖨️ Printing" },
            { "browse events", "📖 Browse Events" },
            { "send me events", "📧 Send Me Events" },
            { "wi-fi", "🛜🌐 Wi-Fi" },
            { "curbside pickup", "🚙 Curbside Pickup" },
            { "facebook", "Facebook" },
        };

        public static string GetDisplayName(string name)
        {
            var sanitized = name?.Trim();
            if (LocationFeatureNames.ContainsKey(sanitized))
            {
                return LocationFeatureNames[sanitized];
            } else if (LocationServiceNames.ContainsKey(sanitized)) 
            { 
                return LocationServiceNames[sanitized]; 
            }
            return "";
        }

        public static bool IsLocationFeature(string name)
        {
            var sanitized = name?.Trim();
            return LocationFeatureNames.ContainsKey(sanitized);
        }

        public static bool IsLocationService(string name)
        {
            var sanitized = name?.Trim();
            return LocationServiceNames.ContainsKey(sanitized);
        }
    }
}
