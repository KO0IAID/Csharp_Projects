using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TranslationLibrary.Emotracker.Maps
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
        public int? Id { get; set; }

        [JsonIgnore]
        public string? Type { get; set; }

        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(ItemReference))
                return;

            string[] parts = ItemReference.Split(':');

            if (parts.Length >= 3)
            {
                if (int.TryParse(parts[0], out int id))
                    Id = id;

                Type = parts[1];
                ItemReference = Uri.UnescapeDataString(parts[2]);
                ItemReference = ItemReference.Replace(" ","");

            }
        }
    }
}