using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TranslationLibrary.Emotracker.Models.Locations;

namespace TranslationLibrary.Emotracker.Models.Items
{
    public class Itempoly
    {
        [JsonIgnore]
        public int? Id { get; set; }

        [JsonIgnore]
        public string? Type { get; set; }

        [JsonIgnore]
        public string? CleanItemReference { get; set; }

        [JsonIgnore]
        public string? ParsedItemReference { get; set; }

        [JsonIgnore]
        public string? Changes { get; set; }

        [JsonPropertyOrder(-1)]
        [JsonPropertyName("item_reference")]
        public string? ItemReference { get; set; }


        public void Initialize() 
        {
            if (string.IsNullOrWhiteSpace(ItemReference))
                return;

            string[] parts = ItemReference.Split(':');

            if (parts.Length >= 3)
            {
                if (int.TryParse(parts[0], out int id))
                    Id = id;

                this.Type = parts[1];
                ParsedItemReference = parts[1] + ":" + parts[2];

                CleanItemReference = Uri.UnescapeDataString(parts[2]).Replace(" ", "");
            }
        }

        /*

        Itempoly Types:

           Toggle:
               Number                  int
               Item_Reference:         string
               Active:                 bool

           Progressive:
               Number                  int
               Item_Reference:         string
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
               Number                  int
               Acronym                 string
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
