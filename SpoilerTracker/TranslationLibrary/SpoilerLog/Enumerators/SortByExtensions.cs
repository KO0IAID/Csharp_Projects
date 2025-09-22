using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationLibrary.SpoilerLog.Enumerators
{
    public static class SortByExtensions
    {
        public static string CustomToString(this SortBy sortBy)
        {
            return sortBy switch
            {
                SortBy.Default => "Default",
                SortBy.GameSettingsAlphabetic => "A-Z",
                SortBy.GameSettingsReverseAlphabetic => "Z-A",
                SortBy.GameSettingsLogOrder => "Log Order",
                SortBy.EntrancesLong => "Long Game OOT/MM A-Z",
                SortBy.EntrancesShort => "Short Game OOT/MM A-Z",
                SortBy.EntrancesLongAlphabetic => "Long A-Z",
                SortBy.EntrancesLongReverseAlphabetic => "Long Z-A",
                SortBy.EntrancesShortAlphabetic => "Short A-Z",
                SortBy.EntrancesShortReverseAlphabetic => "Short Z-A",
                SortBy.EntrancesLongGame => "Long Game OOT/MM A-Z",
                SortBy.EntrancesShortGame => "Short Game OOT/MM A-Z",
                SortBy.EntrancesLongReverseGame => "Long Game MM/OOT A-Z",
                SortBy.EntrancesShortReverseGame => "Short Game MM/OOT A-Z",
                SortBy.TricksAlphabetic => "A-Z",
                SortBy.TricksReverseAlphabetic => "Z-A",
                SortBy.TricksDifficulty => "Tricks by Difficulty",
                SortBy.TricksLogOrder => "Tricks Log Order",
                SortBy.WayOfTheHeroWorld => "World",
                SortBy.WayOfTheHeroItems => "Item A-Z",
                SortBy.WayOfTheHeroLocation => "Location A-Z",
                SortBy.FoolishHintsWorld => "World",
                SortBy.FoolishHintsGossip => "Gossip A-Z",
                SortBy.FoolishHintsLocation => "Location A-Z"
            };
        }
    }
}
