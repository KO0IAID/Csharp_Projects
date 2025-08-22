using System.Text.Json.Serialization;

namespace TranslationLibrary.Emotracker.ItemDatabase
{
    public class Item
    {
        [JsonPropertyName("item_reference")]
        public string ItemReference { get; set; }

        // These properties are optional and depend on the 'type' of item
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
        public double? Stage { get; set; } // For "lua" type items

        [JsonPropertyName("presetNum")]
        public double? PresetNum { get; set; } // For "lua" type "Presets"

    }
}
