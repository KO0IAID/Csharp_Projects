using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranslationLibrary.SpoilerLog.Interfaces;

namespace TranslationLibrary.SpoilerLog.Models
{
    public class Glitch : ICreateFromLine<Glitch>
    {
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public int? LogOrder { get; set; } = 0;

        public Glitch CreateFromLine(string line)
        {
            return new Glitch
            {
                Description = line,
                Difficulty = null,           //Temporarily Null until a difficulty ranking system for each glitch is established
                LogOrder = LogOrder++
            };
        }
    }
}
