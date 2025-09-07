using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranslationLibrary.SpoilerLog.Interfaces;

namespace TranslationLibrary.SpoilerLog.Models
{
    public class Trick : ICreateFromLine<Trick>
    {
        public string Description { get; set; }
        public string Difficulty { get; set; }

        public Trick CreateFromLine(string line)
        {
            return new Trick
            {
                Description = line,
                Difficulty = null           //Temporarily Null until I can establish a difficulty ranking system for each trick
            };
        }
    }
}
