using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.Models.Items
{
    public class Lua : Item
    {
        [JsonIgnore]
        public int? Id { get; set; }

        [JsonPropertyName("active")]
        public bool? Active { get; set; }

        [JsonPropertyName("stage")]
        public double? Stage { get; set; }

        [JsonPropertyName("presetNum")]
        public double? PresetNum { get; set; }
        
    }
}
