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
            line = line.Trim();

            // Multiplayer line (now allows 1+ spaces between Location and Player N)
            var multiMatch = Regex.Match(line,
                @"^(.*?)\s{2,}(World \d+ .*?)\s+(Player \d+ .+)$",
                RegexOptions.IgnoreCase);

            if (multiMatch.Success)
            {
                return new WayOfTheHero
                {
                    World = null, // Will be set later
                    GossipStone = multiMatch.Groups[1].Value.Trim(),
                    Location = multiMatch.Groups[2].Value.Trim(),
                    Item = multiMatch.Groups[3].Value.Trim()
                };
            }

            // Singleplayer line: 3 parts, split by 2+ spaces
            var parts = Regex.Split(line, @"\s{2,}");

            if (parts.Length == 3)
            {
                return new WayOfTheHero
                {
                    World = null,
                    GossipStone = parts[0].Trim(),
                    Location = parts[1].Trim(),
                    Item = parts[2].Trim()
                };
            }

            throw new FormatException($"WayOfHero Couldn't CreateFromLine due to line format in: '{line}'");

        }
    }
}

