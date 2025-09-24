using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationLibrary.SpoilerLog.Models
{
    public class BlockInfo
    {
        
        public string? Header { get; set; }
        public string? SubHeader { get; set; }
        public int HeaderPosition { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string? World {  get; set; }
        public bool MultiWorld { get; set; }
        public bool HasValue { get; set; }
    }
}
