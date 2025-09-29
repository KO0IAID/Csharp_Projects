using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.Models.Items
{
    public class Consumable : Item
    {

        [JsonPropertyName("acquired_count")]
        public int? AcquiredCount { get; set; }

        [JsonPropertyName("consumed_count")]
        public int? ConsumedCount { get; set; }

        [JsonPropertyName("max_count")]
        public int? MaxCount { get; set; }

        [JsonPropertyName("min_count")]
        public int? MinCount { get; set; }
        
    }
}
