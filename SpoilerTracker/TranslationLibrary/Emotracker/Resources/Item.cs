using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TranslationLibrary.Emotracker.Models.Locations;

namespace TranslationLibrary.Emotracker.Resources
{
    //Big catch all item class
    public class Item
    {
        [JsonIgnore]
        public string? Type { get; set; }

        [JsonIgnore]
        public int? Id { get; set; }

        [JsonPropertyName("item_reference")]
        public string? ItemReference { get; set; }

        [JsonPropertyName("acquired_count")]
        public int? AcquiredCount { get; set; }

        [JsonPropertyName("consumed_count")]
        public int? ConsumedCount { get; set; }

        [JsonPropertyName("max_count")]
        public int? MaxCount { get; set; }

        [JsonPropertyName("min_count")]
        public int? MinCount { get; set; }

        [JsonPropertyName("active")]
        public bool? Active { get; set; }

        [JsonPropertyName("stage")]
        public double? Stage {  get; set; }

        [JsonPropertyName("presetNum")]
        public double? PresetNum { get; set; }

        [JsonIgnore]
        public string? Acronym { get; set; }

        [JsonPropertyName("location_reference")]
        public string? LocationReference { get; set; }

        [JsonPropertyName("modified_by_user")]
        public string? ModifiedByUser { get; set; }

        [JsonPropertyName("sections")]
        public List<Section>? Sections { get; set; }

        [JsonPropertyName("section_reference")]
        public string? SectionReference { get; set; }

        [JsonPropertyName("available_chest_count")]
        public int? AvailableChestCount { get; set; }

        /*
          
         Item Types:
         
            Toggle:
                Number                  int
                Item_Reference:         string
                Active:                 bool

            Progressive:
                Item_Reference:         string
                Number                  int
                Stage_Index:            int

            Consumable:
                Number                  int
                Item_Reference:         string
                Acquired_count:         int
                Consumed_coutn:         int
                Max_count:              int
                Min_count:              int
                
            Lua:
                Number                  int
                Item_Reference:         string
                Active:                 bool
                Stage:                  double
                PresetNum               double
            
            Location:
                Acronym                 string
                Number                  int
                Location_Reference      string
                Modified_by_user        bool
                Sections                Section[]

            Section:
                Acronym                 string
                Section_Reference       string
                Available_Chest_Count   int
                
        */

    }
}
