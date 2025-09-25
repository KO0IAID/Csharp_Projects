using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.LocationDatabase
{
    public class Section
    {
        [JsonPropertyName("section_reference")]
        public string SectionReference { get; set; }

        [JsonPropertyName("available_chest_count")]
        public int AvailableChestCount { get; set; }
    }
}
