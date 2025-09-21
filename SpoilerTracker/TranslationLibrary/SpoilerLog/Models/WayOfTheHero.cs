using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TranslationLibrary.SpoilerLog.Interfaces;

namespace TranslationLibrary.SpoilerLog.Models
{
    public class WayOfTheHero : ICreateFromLine<WayOfTheHero>
    {
        
        public string? World { get; set; }
        public string? GossipStone { get; set; }
        public string? Location { get; set; }
        public string? Item { get; set; }

        public WayOfTheHero CreateFromLine(string line)
        {
            string[] parts = Regex.Split(line, @"\s{2,}");

            return new WayOfTheHero
            {
                World = null,
                GossipStone = parts[0].Trim(),
                Location = parts[1].Trim(),
                Item = parts[2].Trim(),
            };
        }
    }
}

