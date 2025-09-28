using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.Models.Items
{
    public class Item
    {
        [JsonPropertyName("item_reference")]
        public string? ItemReference { get; set; }

        /*

        Item Types:

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
