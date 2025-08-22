using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.LocationDatabase
{
    public class Location
    {
        [JsonPropertyName("location_reference")]
        public string LocationReference { get; set; }

        [JsonPropertyName("modified_by_user")]
        public bool ModifiedByUser { get; set; }

        [JsonPropertyName("sections")]
        public List<Section> Sections { get; set; }

        [JsonIgnore]
        public int StartLine { get; set; }

        [JsonIgnore]
        public int EndLine { get; set; }
    }
}

