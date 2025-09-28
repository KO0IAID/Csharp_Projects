using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.Models.Json
{
    public class LocationDatabase
    {
        [JsonPropertyName("locations")]
        public List<Location>? Locations { get; set; }
    }
}
