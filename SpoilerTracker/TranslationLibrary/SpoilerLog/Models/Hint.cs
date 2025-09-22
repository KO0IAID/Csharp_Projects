using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TranslationLibrary.SpoilerLog.Interfaces;

namespace TranslationLibrary.SpoilerLog.Models
{
    public class Hint : ICreateFromLine<Hint>
    {
        public string? World { get; set; }
        public string? GossipStone { get; set; }
        public string? Location { get; set; }
        public string? Item { get; set; }
        public int? Count { get; set; }


        public Hint CreateFromLine(string line)
        {

            return new Hint
            {

            };

        }
    }
}
