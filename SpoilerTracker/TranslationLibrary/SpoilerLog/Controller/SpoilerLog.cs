using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TranslationLibrary;
using TranslationLibrary.SpoilerLog.Enumerators;
using TranslationLibrary.SpoilerLog.Interfaces;
using TranslationLibrary.SpoilerLog.Models;
using TranslationLibrary.SpoilerLog.UI_Notify;

namespace TranslationLibrary.SpoilerLog.Controller
{
    public class SpoilerLog : INotifyPropertyChanged
    {
        public string[] FileContents;
        public bool DebugMode = true;
        public UINotifier? UINotify { get; set; } = new UINotifier();
        public event PropertyChangedEventHandler? PropertyChanged;

        #region Backer Properties
        private ObservableCollection<SeedInfo> _seedInfo = new ObservableCollection<SeedInfo>();
        private ObservableCollection<Setting> _gameSettings = new ObservableCollection<Setting>();
        private ObservableCollection<Condition> _specialConditions = new ObservableCollection<Condition>();
        private ObservableCollection<Trick> _tricks = new ObservableCollection<Trick>();
        private ObservableCollection<string> _junkLocations = new ObservableCollection<string>();
        private ObservableCollection<WorldFlag> _worldFlags = new ObservableCollection<WorldFlag>();
        private ObservableCollection<Entrance> _entrances = new ObservableCollection<Entrance>();
        private ObservableCollection<Hint> _wayOfTheHeroHints = new ObservableCollection<Hint>();
        private ObservableCollection<Hint> _foolishHints = new ObservableCollection<Hint>();
        private ObservableCollection<Hint> _specificHints = new ObservableCollection<Hint>();
        private ObservableCollection<Hint> _regionalHints = new ObservableCollection<Hint>();
        private ObservableCollection<Hint> _foolishRegions = new ObservableCollection<Hint>();
        private ObservableCollection<Pathway> _paths = new ObservableCollection<Pathway>();
        private ObservableCollection<Sphere> _spheres = new ObservableCollection<Sphere>();
        private ObservableCollection<ItemLocation> _locationList = new ObservableCollection<ItemLocation>();
        #endregion
        public ObservableCollection<SeedInfo> SeedInfo
        {
            get => _seedInfo;
            set { if (_seedInfo != value) { _seedInfo = value; OnPropertyChanged(nameof(SeedInfo)); } }
        }
        public ObservableCollection<Setting> GameSettings 
        {
            get => _gameSettings; 
            set {if (_gameSettings != value) {_gameSettings = value; OnPropertyChanged(nameof(GameSettings));}}
        }
        public ObservableCollection<Condition> SpecialConditions
        {
            get => _specialConditions;
            set { if (_specialConditions != value) { _specialConditions = value; OnPropertyChanged(nameof(SpecialConditions)); } }
        }
        public ObservableCollection<Trick> Tricks
        {
            get => _tricks;
            set { if (_tricks != value) { _tricks = value; OnPropertyChanged(nameof(Tricks)); } }
        }
        public ObservableCollection<string> JunkLocations
        {
            get => _junkLocations;
            set { if (_junkLocations != value) { _junkLocations = value; OnPropertyChanged(nameof(JunkLocations)); } }
        }
        public ObservableCollection<WorldFlag> WorldFlags
        {
            get => _worldFlags;
            set { if (_worldFlags != value) { _worldFlags = value; OnPropertyChanged(nameof(WorldFlags)); } }
        }
        public ObservableCollection<Entrance> Entrances
        {
            get => _entrances;
            set { if (_entrances != value) { _entrances = value; OnPropertyChanged(nameof(Entrances)); } }
        }
        public ObservableCollection<Hint> WayOfTheHeroHints
        {
            get => _wayOfTheHeroHints;
            set { if (_wayOfTheHeroHints != value) { _wayOfTheHeroHints = value; OnPropertyChanged(nameof(WayOfTheHeroHints)); } }
        }
        public ObservableCollection<Hint> FoolishHints
        {
            get => _foolishHints;
            set { if (_foolishHints != value) { _foolishHints = value; OnPropertyChanged(nameof(FoolishHints)); } }
        }
        public ObservableCollection<Hint> SpecificHints
        {
            get => _specificHints;
            set { if (_specificHints != value) { _specificHints = value; OnPropertyChanged(nameof(SpecificHints)); } }
        }
        public ObservableCollection<Hint> RegionalHints
        {
            get => _regionalHints;
            set { if (_regionalHints != value) { _regionalHints = value; OnPropertyChanged(nameof(RegionalHints)); } }
        }
        public ObservableCollection<Hint> FoolishRegions
        {
            get => _foolishRegions;
            set { if (_foolishRegions != value) { _foolishHints = value; OnPropertyChanged(nameof(FoolishRegions)); } }
        }
        public ObservableCollection<Pathway> Paths
        {
            get => _paths;
            set { if (_paths != value) { _paths = value; OnPropertyChanged(nameof(Paths)); } }
        }
        public ObservableCollection<Sphere> Spheres
        {
            get => _spheres;
            set { if (_spheres != value) { _spheres = value; OnPropertyChanged(nameof(Spheres)); } }
        }
        public ObservableCollection<ItemLocation> LocationList
        {
            get => _locationList;
            set { if (_locationList != value) { _locationList = value; OnPropertyChanged(nameof(LocationList)); } }
        }


        public async Task AddFileContents(string filePath)
        {
            
            Stopwatch sw = new Stopwatch();
            sw.Start();
                
            
            FileContents = await File.ReadAllLinesAsync(filePath);

            try
            {
                SeedInfo = await Parse_SeedInfo();
                GameSettings = await Parse_GameSettings();
                SpecialConditions = await Parse_SpecialConditions();
                Tricks = await Parse_Tricks();
                JunkLocations = await Parse_JunkLocations();
                WorldFlags = await Parse_WorldFlags();
                Entrances = await Parse_Entrances();
                WayOfTheHeroHints = await Parse_WayOfTheHeroHints();
                


                await SortCollections();

                await DebugStats(sw);


            }
            catch (Exception ex)
            {

                Debug.WriteLineIf(DebugMode, $"Exception: {ex}\nMessage: {ex.Message}\nSource: {ex.Source}");
            }

        }
        public void ClearFileContents()
        {
            SeedInfo.Clear();
            GameSettings.Clear();
            SpecialConditions.Clear();
            JunkLocations.Clear();
            WorldFlags.Clear();
            Entrances.Clear();
            WayOfTheHeroHints.Clear();
            FoolishHints.Clear();
            FoolishRegions.Clear();
            Paths.Clear();
            Spheres.Clear();
            LocationList.Clear();
        }
        public bool HasValue()
        {
            // Check if FileContents is not null and contains at least one line
            return FileContents != null && FileContents.Length > 0;
        }
        public async Task DebugStats(Stopwatch sw)
        {
            sw.Stop();
            Debug.WriteLineIf(DebugMode,
                $"--- Spoiler Sheet Data Added! Time Taken: {sw.Elapsed.ToString()}---" +
                $"\nSeed Info:\t\t\t{SeedInfo.Count}" +
                $"\nGame Settings:\t\t{GameSettings.Count}" +
                $"\nSpecial Conditions:\t{SpecialConditions.Count}" +
                $"\nJunk Locations:\t\t{JunkLocations.Count}" +
                $"\nWorld Flags:\t\t{WorldFlags.Count}" +
                $"\nEntrances:\t\t\t{Entrances.Count}"
                //$"\n:\t{}"
                );
        }
        public async Task SortCollections(SortBy sort = SortBy.Default)
        {

            #region GameSettings
            // GameSettings - Alphabetic - (Default)
            if (sort == SortBy.GameSettingsAlphabetic || sort == SortBy.Default)
            {
                var sortedGameSettings = new ObservableCollection<Setting>(
                GameSettings.OrderBy(e => e.Name)
                .ThenBy(e => e.Value)
            );
                GameSettings = sortedGameSettings;
            }
            // GameSettings - LogOrder
            if (sort == SortBy.GameSettingsLogOrder)
            {
                var sortedGameSettings = new ObservableCollection<Setting>(
                GameSettings.OrderBy(e => e.LogOrder)
            );
                GameSettings = sortedGameSettings;
            }
            #endregion
            #region Entrances
            // Entrances - Short - (Default)
            if (sort == SortBy.EntrancesShort || sort == SortBy.Default)
            {
                var sortedShortEntrances = new ObservableCollection<Entrance>(
                Entrances.OrderBy(e => e.World)
                .ThenByDescending(e => e.FromGame)
                .ThenBy(e => e.ShortEntrance)
                .ThenBy(e => e.ShortDestination)
            );
                Entrances = sortedShortEntrances;
            }
            // Entrances - Long
            if (sort == SortBy.EntrancesLong)
            {
                var sortedLongEntrances = new ObservableCollection<Entrance>(
                Entrances.OrderBy(e => e.World)
                .ThenByDescending(e => e.FromGame)
                .ThenBy(e => e.LongEntrance)
                .ThenBy(e => e.LongDestination)
            );
                Entrances = sortedLongEntrances;
            }

            // Entrances - Short - Game
            if (sort == SortBy.EntrancesShortGame)
            {
                var sortedLongEntrances = new ObservableCollection<Entrance>(
                Entrances.OrderByDescending(e => e.FromGame)
                .ThenBy(e => e.ShortEntrance)
                .ThenBy(e => e.ShortDestination)
                .ThenBy(e => e.World)
            );
                Entrances = sortedLongEntrances;
            }

            // Entrances - Long - Game
            if (sort == SortBy.EntrancesLongGame)
            {
                var sortedLongEntrances = new ObservableCollection<Entrance>(
                Entrances.OrderBy(e => e.World)
                .ThenByDescending(e => e.FromGame)
                .ThenBy(e => e.LongEntrance)
                .ThenBy(e => e.LongDestination)
            );
                Entrances = sortedLongEntrances;
            }

            // Entrances - Short - Alphabetic
            if (sort == SortBy.EntrancesShortAlphabetic)
            {
                var sortedLongEntrances = new ObservableCollection<Entrance>(
                Entrances.OrderBy(e => e.ShortEntrance)
                .ThenBy(e => e.ShortDestination)
                .ThenByDescending(e => e.FromGame)
                .ThenBy(e => e.World)
            );
                Entrances = sortedLongEntrances;
            }

            // Entrances - Long - Alphabetic
            if (sort == SortBy.EntrancesLongAlphabetic)
            {
                var sortedLongEntrances = new ObservableCollection<Entrance>(
                Entrances.OrderBy(e => e.LongEntrance)
                .ThenBy(e => e.LongDestination)
                .ThenByDescending(e => e.FromGame)
                .ThenBy(e => e.World)
            );
                Entrances = sortedLongEntrances;
            }

            #endregion

            // Tricks - Alphabetic - (Default)
            if (sort == SortBy.TricksAlphabetic || sort == SortBy.Default)
            {
                var sortedTricks = new ObservableCollection<Trick>(
                Tricks.OrderBy(e => e.Description)
                .ThenBy(e => e.Difficulty)
                .ThenBy(e => e.LogOrder)
            );
                Tricks = sortedTricks;
            }

            // Tricks - Difficulty
            if (sort == SortBy.TricksDifficulty)
            {
                var sortedTricks = new ObservableCollection<Trick>(
                Tricks.OrderBy(e => e.Difficulty)
                .ThenBy(e => e.Description)
                .ThenBy(e => e.LogOrder)
            );
                Tricks = sortedTricks;
            }

            // Tricks - Log Order
            if (sort == SortBy.TricksLogOrder)
            {
                var sortedTricks = new ObservableCollection<Trick>(
                Tricks.OrderBy(e => e.LogOrder)
                .ThenBy(e => e.Description)
                .ThenBy(e => e.Difficulty)
            );
                Tricks = sortedTricks;
            }

            UINotify?.NotifyAll();
        }






        #region Data Parsing Helper Methods

        private async Task<ObservableCollection<T>?> AddValues<T>(Tuple<int, int> range, string[] fileContents) where T : ICreateFromLine<T>, new()
        {
            if (range.Item1 == -1 || range.Item2 == -1)
                return null;

            var collection = new ObservableCollection<T>();
            var parser = new T();

            for (int i = range.Item1; i <= range.Item2; i++)
            {
                string line = fileContents[i];
                T item = parser.CreateFromLine(line);
                collection.Add(item);
            }
            return collection;

        } 
        private async Task<KeyValuePair<string, string>?> Parse_SingleKeyValueAsync(string[] file, string categoryName, string delimiter = ":", int startingPosition = 0)
        {
            return await Task.Run(() =>
            {
                int position = startingPosition;

                while (position < file.Length &&
                       !file[position].TrimStart().StartsWith(categoryName + delimiter, StringComparison.OrdinalIgnoreCase))
                {
                    position++;
                }

                if (position >= file.Length)
                    return (KeyValuePair<string, string>?)null;

                string[] line = file[position].Split(new[] { delimiter }, 2, StringSplitOptions.None);

                if (line.Length < 2)
                    return (KeyValuePair<string, string>?)null;

                return new KeyValuePair<string, string>(line[0].Trim(), line[1].Trim());
            });
        }
        private async Task<ObservableCollection<string>> Parse_MultipleStringsAsync(string categoryName, string[] file, int startingPosition = 0)
        {
            int position = startingPosition;
            ObservableCollection<string> list = new ObservableCollection<string>();

            // Find the category header
            while (position < file.Length && !file[position].TrimStart().StartsWith(categoryName, StringComparison.OrdinalIgnoreCase))
            {
                position++;
            }

            // If the category header is not found, return null
            if (position >= file.Length)
                return list;
           
                

            // Move past the category header to the first data line
            position++;

            // Parse strings until a blank line or un-indented line is found
            while (position < file.Length)
            {
                string currentLine = file[position];

                // Trim leading and trailing whitespace
                string trimmedLine = currentLine.Trim();

                // Check for the end of the list:
                // 1. A completely empty line
                // 2. A line that is not indented, indicating the start of a new category or a file end
                if (string.IsNullOrEmpty(trimmedLine) || !currentLine.StartsWith("  "))
                {
                    // If the current line is not an indented value, the category list has ended.
                    break;
                }

                // Add the trimmed line to the list
                list.Add(trimmedLine);

                // Move to the next line
                position++;
            }

            return list;
        }
        private ObservableCollection<KeyValuePair<string, string>> Parse_FlatKeyValueBlock(string categoryName, string[] fileContents)
        {
            var result = new ObservableCollection<KeyValuePair<string, string>>();


            for (int i = 0; i < fileContents.Length; i++)
            {
                var line = fileContents[i];

                if (fileContents[i].Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                {
                    i++; // Move to line after header

                    while (i < fileContents.Length && !string.IsNullOrWhiteSpace(fileContents[i]))
                    {
                        string value = fileContents[i];
                        string[] parts = fileContents[i].Trim().Split(':', 2);

                        if (parts.Length == 2)
                        {
                            result.Add(new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim()));
                        }

                        i++;
                    }

                    break; // done with this block
                }
            }

            return result;
        }
        private ObservableCollection<KeyValuePair<string, string>> Parse_SpecificFlatKeyValueBlock(string categoryName, string[] fileContents)
        {
            var result = new ObservableCollection<KeyValuePair<string, string>>();

            for (int i = 0; i < fileContents.Length; i++)
            {
                var line = fileContents[i];

                if (line.Trim().Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                {
                    i++; // Move to line after header

                    while (i < fileContents.Length)
                    {
                        string currentLine = fileContents[i];

                        // Stop if we hit a new category or sub-category
                        int indent = currentLine.TakeWhile(char.IsWhiteSpace).Count();
                        if (currentLine.TrimEnd().EndsWith(":") && indent <= 4)
                        {
                            break;
                        }

                        // Process non-empty lines
                        if (!string.IsNullOrWhiteSpace(currentLine))
                        {
                            string[] parts = currentLine.Trim().Split(':', 2);
                            if (parts.Length == 2)
                            {
                                result.Add(new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim()));
                            }
                        }

                        i++;
                    }

                    break; // Only one block to parse
                }
            }

            return result;
        }

        private async Task<Tuple<int, int>> FindCategoryRangeAsync(string categoryName, string[] file, int startingPosition = 0)
        {
            int position = startingPosition;
            int start = -1;
            int end = file.Length - 1;

            // Search for Category Header
            while (position < file.Length && !file[position].Equals(categoryName, StringComparison.Ordinal))
            {
                position++;
            }

            // Category not found
            if (position >= file.Length)
            {
                return Tuple.Create(-1, -1);
            }

            // Category Found: 
            // Move past the category header
            position++;
            start = position;

            while (position < file.Length)
            {
                string line = file[position];

                // Still In Category
                if (line.StartsWith("  "))
                {
                    position++;
                }
                // Possible End of Category
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // Peek ahead && Still indented
                    if (position + 1 < file.Length && file[position + 1].StartsWith("  "))
                    {
                        position += 2;
                    }
                    // End of Category Reached
                    else
                    {
                        break;
                    }
                }
                // Safety Net
                else
                {
                    break;
                }
            }
            end = position - 1;
            return Tuple.Create(start, end);
        }
        private async Task<BlockInfo?> FindBlock(string header, string[] file, int startingPosition = 0)
        {
            int position = startingPosition;

            while (position < file.Length && !file[position].Trim().Equals(header, StringComparison.OrdinalIgnoreCase))
                position++;

            if (position >= file.Length)
                return null;

            int startLine = position + 1;
            int endLine = startLine;
            string? subHeader = null;

            while (endLine < file.Length)
            {
                string line = file[endLine];

                // Detect first subheader (e.g., "  BRIDGE:")
                if (subHeader == null && line.StartsWith("  ") && line.Trim().EndsWith(":") && !line.Contains(" "))
                {
                    subHeader = line.Trim().TrimEnd(':');
                }

                // Exit block if we hit a new top-level section
                if (!line.StartsWith("  ") && !string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                endLine++;
            }

            return new BlockInfo
            {
                Header = header,
                StartLine = startLine,
                EndLine = endLine - 1,
                SubHeader = subHeader
            };
        }
        #endregion


        #region Data Parsing
        private async Task<ObservableCollection<SeedInfo>> Parse_SeedInfo()
        {
            var seedInfo = new ObservableCollection<SeedInfo>();

            SeedInfo seed = new SeedInfo { Pair = await Parse_SingleKeyValueAsync(FileContents, "Seed") };
            SeedInfo version = new SeedInfo { Pair = await Parse_SingleKeyValueAsync(FileContents, "Version") };
            SeedInfo settings = new SeedInfo { Pair = await Parse_SingleKeyValueAsync(FileContents, "SettingsString") };

            if (seed.Pair.HasValue) seedInfo.Add(seed);
            if (version.Pair.HasValue) seedInfo.Add(version);
            if (settings.Pair.HasValue) seedInfo.Add(settings);

            return seedInfo;


        }
        private async Task<ObservableCollection<Setting>?> Parse_GameSettings()
        {

            var range = await FindCategoryRangeAsync("Settings", FileContents);

            var settings = await AddValues<Setting>(range, FileContents);

            return settings;
        }
        private async Task<ObservableCollection<Condition>?> Parse_SpecialConditions()
        {
            var result = new ObservableCollection<Condition>();
            var block = await FindBlock("Special Conditions", FileContents);

            if (block == null) return result;

            string? currentType = null;

            for (int i = block.StartLine; i <= block.EndLine; i++)
            {
                string line = FileContents[i].Trim();

                if (string.IsNullOrWhiteSpace(line)) continue;

                // Detect new subheader (e.g., BRIDGE:)
                if (!line.StartsWith("-") && line.EndsWith(":") && !line.Contains(" "))
                {
                    currentType = line.Replace(":", "").Trim();
                    continue;
                }

                // Parse key-value under current type
                var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
                if (parts.Length == 2)
                {
                    string key = parts[0];
                    string value = parts[1];

                    if (key.Equals("count", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out int count))
                        result.Add(new Condition(currentType, key, value, count));
                    else
                        result.Add(new Condition(currentType, key, value));
                }
            }

            return result;
        }
        private async Task<ObservableCollection<Trick>?> Parse_Tricks()
        {
            var range = await FindCategoryRangeAsync("Tricks", FileContents);

            var tricks = await AddValues<Trick>(range, FileContents);

            return tricks;
        }
        private async Task<ObservableCollection<string>>? Parse_JunkLocations()
        {
            return await Parse_MultipleStringsAsync("Junk Locations", FileContents);
        }
        private async Task<ObservableCollection<WorldFlag>> Parse_WorldFlags()
        {
            var worldFlags = new ObservableCollection<WorldFlag>();

            var (start, end) = await FindCategoryRangeAsync("World Flags", FileContents);
            if (start == -1) return worldFlags;

            string? currentWorld = null;

            for (int i = start; i <= end; i++)
            {
                string line = FileContents[i];

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Detect World header for multiplayer
                if (!line.StartsWith("    ") && line.Trim().StartsWith("World", StringComparison.OrdinalIgnoreCase))
                {
                    currentWorld = line.Trim();
                    continue;
                }
                // Detect if its singleplayer
                else if (line.StartsWith("  "))
                {
                    currentWorld = "";
                }

                // Parse Multiplayer indented flag lines (e.g., "    Ganon Trials: none")
                if (line.StartsWith("    ") && currentWorld != null)
                {
                    var parts = line.Trim().Split(':', 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        var condition = parts[0].Trim();
                        var value = parts[1].Trim();

                        worldFlags.Add(new WorldFlag
                        {
                            World = currentWorld,
                            Condition = condition,
                            Value = value
                        });
                    }
                }
                // Parse Single Player indented flag lines (e.g., "    Ganon Trials: none")
                if (line.StartsWith("  ") && currentWorld == "")
                {
                    var parts = line.Trim().Split(':', 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        var condition = parts[0].Trim();
                        var value = parts[1].Trim();

                        worldFlags.Add(new WorldFlag
                        {
                            World = currentWorld,
                            Condition = condition,
                            Value = value
                        });
                    }
                }
            }

            return worldFlags;
        }
        private async Task<ObservableCollection<Entrance>> Parse_Entrances()
        {
            var entrances = new ObservableCollection<Entrance>();

            var (start, end) = await FindCategoryRangeAsync("Entrances", FileContents);
            if (start == -1) return entrances;

            string? currentWorld = null;

            for (int i = start; i <= end; i++)
            {
                string line = FileContents[i];

                if (string.IsNullOrWhiteSpace(line)) continue;

                // Detect world header for multiplayer
                if (!line.StartsWith("    ") && line.Trim().StartsWith("World", StringComparison.OrdinalIgnoreCase))
                {
                    currentWorld = line.Trim(); // e.g., "World 1"
                    continue;
                }

                // Detect singleplayer section
                if (line.StartsWith("  ") && !line.StartsWith("    "))
                {
                    currentWorld = ""; // Singleplayer: world is empty
                }

                // Multiplayer or singleplayer line
                if (line.StartsWith("    ") || line.StartsWith("  "))
                {
                    var parts = line.Trim().Split(new[] { "->" }, 2, StringSplitOptions.None);
                    if (parts.Length != 2) continue;

                    // Use regex to extract long/short names
                    var pattern = @"^(.*?)\((.*?)\)\s*->\s*(.*?)\((.*?)\)$";
                    var match = Regex.Match(line.Trim(), pattern);
                    if (!match.Success) continue;

                    var entranceLong = match.Groups[1].Value.Trim();
                    var entranceShortRaw = match.Groups[2].Value.Trim();
                    var destinationLong = match.Groups[3].Value.Trim();
                    var destinationShortRaw = match.Groups[4].Value.Trim();

                    // Game code detection (based on raw short values)
                    var fromGame = entranceShortRaw.StartsWith("MM") ? "MM" : "OOT";
                    var toGame = destinationShortRaw.StartsWith("MM") ? "MM" : "OOT";

                    // Remove Game code from (based on raw values)
                    entranceLong = entranceLong.StartsWith("MM") ? entranceLong.Remove(0, 2).Trim() : entranceLong.Remove(0, 4).Trim();
                    entranceShortRaw = entranceShortRaw.StartsWith("MM") ? entranceShortRaw.Remove(0, 2).Trim() : entranceShortRaw.Remove(0, 4).Trim();
                    destinationLong = destinationLong.StartsWith("MM") ? destinationLong.Remove(0, 2).Trim() : destinationLong.Remove(0, 4).Trim();
                    destinationShortRaw = destinationShortRaw.StartsWith("MM") ? destinationShortRaw.Remove(0, 2).Trim() : destinationShortRaw.Remove(0, 4).Trim();

                    // Remove _ from short
                    var entranceShort = entranceShortRaw.Replace('_', ' ').Trim();
                    var destinationShort = destinationShortRaw.Replace('_', ' ').Trim();

                    // Lower case, then upper case First letter for shorts
                    TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
                    entranceShort = textInfo.ToTitleCase(entranceShort.ToLower());
                    destinationShort = textInfo.ToTitleCase(destinationShort.ToLower());
                    

                    entrances.Add(new Entrance
                    {
                        World = currentWorld,
                        FromGame = fromGame,
                        ToGame = toGame,
                        LongEntrance = entranceLong,
                        LongDestination = destinationLong,
                        ShortEntrance = entranceShort,
                        ShortDestination = destinationShort
                    });
                }
            }

            return entrances; 
        }
        private async Task<ObservableCollection<Hint>> Parse_WayOfTheHeroHints()
        {
            var hints = new ObservableCollection<Hint>();

            var (start, end) = await FindCategoryRangeAsync("Hints", FileContents);
            if (start == -1) return hints;

            string? currentWorld = null;
            string? currentType = null;

            for (int i = start; i <= end; i++)
            {
                string line = FileContents[i];

                // Skip blank lines
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Detect world header (e.g., "  World 1")
                if (!line.StartsWith("  ") && line.Trim().StartsWith("World", StringComparison.OrdinalIgnoreCase))
                {
                    currentWorld = line.Trim();     // May still include :
                    continue;
                }

                // Detect world header (e.g., "  World 1")
                if (!line.StartsWith("    ") || !line.StartsWith("  ") && line.Trim().StartsWith("Way of the Hero", StringComparison.OrdinalIgnoreCase))
                {
                    currentType = line.Trim();     // May still include :
                    continue;
                }
              
                // Single Player
                if (line.StartsWith("    ") && currentWorld != null)
                {
                    var parts = line.Trim().Split("  ");
                    if (parts.Length != 3) continue;

                    var gossipStone = parts[0].Trim();
                    var location = parts[1].Trim();
                    var item = parts[2].Trim();

                    WayOfTheHeroHints.Add(new Hint
                    {
                        World = currentWorld,
                        Type = currentType,
                        GossipStone = gossipStone,
                        Location = location,
                        Item = item
                    });
                }

                // Multi-player
                if (line.StartsWith("      ") && currentWorld != null)
                {
                    var parts = line.Trim().Split("World");
                    if (parts.Length != 3) continue;

                    var gossipStone = parts[0].Trim();
                    var location = parts[1].Trim();
                    var item = parts[2].Trim();

                    WayOfTheHeroHints.Add(new Hint
                    {
                        World = currentWorld,
                        Type = currentType,
                        GossipStone = gossipStone,
                        Location = location,
                        Item = item
                    });
                }
            }

            return hints;
        }


        #endregion

        #region UI Notifyer
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
