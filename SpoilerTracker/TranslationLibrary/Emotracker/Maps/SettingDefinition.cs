using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.Maps
{
    public class SettingDefinition
    {
        public string TrackerItemReference { get; set; } = "";
        public string SpoilerLabel { get; set; } = "";

        // This comes from the JSON as { "0": "none", "1": "dungeons", ... }
        public Dictionary<int, string> IndexToLabel { get; set; } = new();

        [JsonIgnore]
        public Dictionary<string, int> LabelToIndex =>
            IndexToLabel.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public string? GetLabelFromIndex(int index) =>
            IndexToLabel.TryGetValue(index, out var label) ? label : null;

        public int? GetIndexFromLabel(string label) =>
            LabelToIndex.TryGetValue(label, out var index) ? index : null;
    }
}

