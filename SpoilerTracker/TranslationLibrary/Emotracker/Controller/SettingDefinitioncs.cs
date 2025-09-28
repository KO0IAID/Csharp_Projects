using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationLibrary.Emotracker.Controller
{
    public class SettingDefinition
    {
        public string? ItemReference { get; set; }
        public string? DisplayName { get; set; }
        public string? SpoilerLogLabel { get; set; }

        public Dictionary<int, string>? StageDescriptions { get; set; } // 0-3: Human-friendly
        public Dictionary<int, string>? SpoilerLogValues { get; set; }  // 0-3: SpoilerLog values
    }
}
