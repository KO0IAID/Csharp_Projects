using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.LocationDatabase
{
    public class LocationDatabase
    {
        [JsonPropertyName("location_database")]
        public List<Location> Locations { get; set; } = new();
    }
}
