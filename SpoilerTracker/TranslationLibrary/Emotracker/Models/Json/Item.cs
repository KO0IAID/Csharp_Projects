using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.Models.Json
{
    public class Item
    {
        [JsonPropertyName("item_reference")]
        public string ItemReference { get; set; }

        // Optional Properties

        [JsonPropertyName("active")]
        public bool? Active { get; set; }

        [JsonPropertyName("stage_index")]
        public int? StageIndex { get; set; }

        [JsonPropertyName("acquired_count")]
        public int? AcquiredCount { get; set; }

        [JsonPropertyName("consumed_count")]
        public int? ConsumedCount { get; set; }

        [JsonPropertyName("max_count")]
        public int? MaxCount { get; set; }

        [JsonPropertyName("min_count")]
        public int? MinCount { get; set; }

        [JsonPropertyName("stage")]
        public double? Stage { get; set; }

        [JsonPropertyName("presetNum")]
        public double? PresetNum { get; set; }
    }
}
