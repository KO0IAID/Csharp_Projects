using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OoTMMSpoilerToTracker.Tracker.Models.Locations
{

    public class LocationDatabase
    {
        [JsonPropertyName("locations")]
        public List<Location>? Locations { get; set; }

        [JsonIgnore]
        public int Count => Locations?.Count ?? 0;
    }
}
