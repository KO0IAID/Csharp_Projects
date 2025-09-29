using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TranslationLibrary.Emotracker.Models.Items
{
    public class Toggle : Item
    {
        [JsonPropertyName("active")]
        public bool? Active { get; set; }

    }
}
