using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TranslationLibrary.Emotracker.Models.Items;

namespace TranslationLibrary.Emotracker.Models.Locations
{
    public class Location
    {
        [JsonIgnore]
        public int? Id { get; set; }

        [JsonIgnore]
        public string? Acronym { get; set; }

        [JsonPropertyName("location_reference")]
        public string? LocationReference { get; set; }

        [JsonPropertyName("modified_by_user")]
        public bool? ModifiedByUser { get; set; }

        [JsonPropertyName("sections")]
        public List<Section>? Sections { get; set; }
    }
}
