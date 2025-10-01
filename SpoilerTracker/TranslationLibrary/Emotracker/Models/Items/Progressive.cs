using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.Models.Items
{
    public class Progressive : Itempoly
    {
        [JsonPropertyName("stage_index")]
        public int? StageIndex { get; set; }

    }
}
