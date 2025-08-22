using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.ItemDatabase
{
    public class ItemDatabase
    {
        [JsonPropertyName("item_database")]
        public List<Item> Items { get; set; }

        public ItemDatabase()
        {
            Items = new List<Item>();
        }
        public void AddItem(Item item)
        {
            Items.Add(item);
        }
        public Item GetItemByReference(string itemReference)
        {
            return Items.Find(i => i.ItemReference == itemReference);
        }
    }
}
