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
        public static string ToString(this SortBy sortBy)
        {
            return sortBy switch
            {
                SortBy.Default => "Default",
                SortBy.GameSettingsAlphabetic => "A-Z",
                SortBy.GameSettingsReverseAlphabetic => "Z-A",
                SortBy.GameSettingsLogOrder => "Log Order",
                SortBy.EntrancesLong => "Long",
                SortBy.EntrancesShort => "Short",
                SortBy.EntrancesLongAlphabetic => "Long A-Z",
                SortBy.EntrancesLongReverseAlphabetic => "Long Z-A",
                SortBy.EntrancesShortAlphabetic => "Short A-Z",
                SortBy.EntrancesShortReverseAlphabetic => "Short Z-A",
                SortBy.EntrancesLongGame => "Long by Game",
                SortBy.EntrancesShortGame => "Short by Game",
                SortBy.TricksAlphabetic => "A-Z",
                SortBy.TricksReverseAlphabetic => "Z-A",
                SortBy.TricksDifficulty => "Tricks by Difficulty",
                SortBy.TricksLogOrder => "Tricks Log Order"
            };

        }
    }
}
