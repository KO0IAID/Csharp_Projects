using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TranslationLibrary.Emotracker.ItemDatabase;
using TranslationLibrary.Emotracker.LocationDatabase;


namespace TranslationLibrary.Emotracker.TrackerHeader
{
    public class Header
    {
        [JsonPropertyName("package_uid")]
        public string PackageUid { get; set; }

        [JsonPropertyName("package_variant_uid")]
        public string PackageVariantUid { get; set; }

        [JsonPropertyName("package_version")]
        public string PackageVersion { get; set; }

        [JsonPropertyName("creation_time")]
        public string CreationTime { get; set; }

        [JsonPropertyName("ignore_all_logic")]
        public bool IgnoreAllLogic { get; set; }

        [JsonPropertyName("display_all_locations")]
        public bool DisplayAllLocations { get; set; }

        [JsonPropertyName("always_allow_chest_manipulation")]
        public bool AlwaysAllowChestManipulation { get; set; }

        [JsonPropertyName("auto_unpin_locations_on_clear")]
        public bool AutoUnpinLocationsOnClear { get; set; }

        [JsonPropertyName("pin_locations_on_item_capture")]
        public bool PinLocationsOnItemCapture { get; set; }

        [JsonPropertyName("item_database")]
        public List<Item> ItemDatabase { get; set; }

        //[JsonPropertyName("location_database")]
        //public LocationDatabase< LocationDatabase { get; set; }
    }
}
