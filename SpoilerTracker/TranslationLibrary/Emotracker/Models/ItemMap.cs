using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TranslationLibrary.Emotracker.Models
{
    public class ItemMap
    {
        [JsonPropertyName("spoiler_label")]
        public string? SpoilerLabel { get; set; }

        [JsonPropertyName("item_reference")]
        public string? ItemReference { get; set; }

        [JsonPropertyName("values")]
        public Dictionary<string, int>? Values { get; set; }

        [JsonIgnore]
        public string? Type { get; set; }
        [JsonIgnore]
        public string? File { get; set; }

        [JsonIgnore]
        public string? FullItemReference { get; set; }

        [JsonIgnore]
        public string? SpecialType { get; set; }


        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(ItemReference))
                return;

            string[] parts = ItemReference.Split(':');

            if (parts.Length > 1)
            {
                Type = parts[0];
                FullItemReference = ItemReference;
                ItemReference = parts[1].Trim();
            }
            if (!string.IsNullOrEmpty(ItemReference))
            {
                SpecialType = ItemReference switch
                {
                    var s when s.StartsWith("Rainbow") => "BRIDGE",
                    var s when s.StartsWith("Moon") => "MOON",
                    var s when s.StartsWith("LACS") => "LACS",
                    var s when s.StartsWith("Majora") => "MAJORA",
                    var s when s.StartsWith("Triforce") => "TRIFORCE",
                    _ => SpecialType // keep old value if no match
                };
            }
        }
    }
}