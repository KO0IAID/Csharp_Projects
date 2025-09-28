using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.Models.Locations
{
    public class Section
    {
        [JsonIgnore]
        public string? Acronym { get; set; }

        [JsonPropertyName("section_reference")]
        public string? SectionReference { get; set; }

        [JsonPropertyName("available_chest_count")]
        public int? AvailableChestCount { get; set; }

    }
}
