using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TranslationLibrary.Emotracker.Maps;
using TranslationLibrary.Emotracker.Models;
using TranslationLibrary.Emotracker.Models.Items;
using TranslationLibrary.Emotracker.Models.Locations;
using TranslationLibrary.SpoilerLog.Controller;
using TranslationLibrary.SpoilerLog.Models;
using Itempoly = TranslationLibrary.Emotracker.Models.Items.Itempoly;


namespace TranslationLibrary.Emotracker.Controller
{
    public class EmoTracker
    {
        public Tracker? Tracker { get; private set; }

        #region Collections
        public List<Itempoly> AllItems { get; private set; } = new();
        public List<Toggle> Toggles { get; private set; } = new();
        public List<Progressive> Progressives { get; private set; } = new();
        public List<Consumable> Consumables { get; private set; } = new();
        public List<Lua> Luas { get; private set; } = new();
        public List<Location> Locations { get; private set; } = new();
        #endregion

        #region Maps
        public List<ItemMap> Maps { get; private set; } = new();

        #endregion


        public async Task ImportMaps(string[] filePaths, bool showDebugStats = false)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Maps = new List<ItemMap>(); // Reset master list

            var mapOptions = new JsonSerializerOptions  {   PropertyNameCaseInsensitive = true  };

            foreach (string filePath in filePaths)
            {
                if (!File.Exists(filePath))
                {
                    Debug.WriteLine($"File not found: {filePath}");
                    continue;
                }

                try
                {
                    using FileStream stream = File.OpenRead(filePath);
                    var items = await JsonSerializer.DeserializeAsync<List<ItemMap>>(stream, mapOptions);

                    if (items == null)
                    {
                        Debug.WriteLine($"Failed to deserialize JSON file: {filePath}");
                        continue;
                    }

                    foreach (var item in items)
                    {
                        item.Initialize();
                        item.File = filePath;
                        Maps.Add(item);
                    }
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"JSON error in file '{filePath}': {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing file '{filePath}': {ex.Message}");
                }
            }
            if (showDebugStats) 
            {
                DebugStats(sw,"Maps");
            }
                
        }
        public async Task ImportTracker(string filePath, bool showDebugStats = false)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            string pathToUse = filePath;

            if (!File.Exists(pathToUse))
                throw new FileNotFoundException("Tracker file not found.", pathToUse);

            // Open the JSON file for reading
            using var stream = File.OpenRead(pathToUse);

            // Register custom converter for Itempoly
            var options = new JsonSerializerOptions
            {
                Converters = { new ItemJsonConverter() },
                PropertyNameCaseInsensitive = true
            };

            // Deserialize the tracker JSON using your Tracker class
            Tracker = await JsonSerializer.DeserializeAsync<Tracker>(stream, options);

            if (Tracker == null)
                return;

            // Clear existing collections before repopulating (optional safety)
            Toggles?.Clear();
            Progressives?.Clear();
            Consumables?.Clear();
            Luas?.Clear();
            Locations?.Clear();

            // Process item_database
            if (Tracker.ItemDatabase != null)
            {
                foreach (Itempoly item in Tracker.ItemDatabase)
                {
                    item.Initialize();

                    AllItems.Add(item);

                    switch (item)
                    {
                        case Toggle toggle:
                            Toggles?.Add(toggle);
                            break;
                        case Progressive progressive:
                            Progressives?.Add(progressive);
                            break;
                        case Consumable consumable:
                            Consumables?.Add(consumable);
                            break;
                        case Lua lua:
                            Luas?.Add(lua);
                            break;
                        default:
                            Debug.WriteLine($"Unknown or unhandled item type: {item.ItemReference}");
                            break;
                    }
                }
            }

            // Process location_database
            if (Tracker.LocationDatabase?.Locations != null)
            {
                foreach (var location in Tracker.LocationDatabase.Locations)
                {
                    location.Initialize(); // Will also initialize its sections
                }

                Locations = Tracker.LocationDatabase.Locations;
            }

            if (showDebugStats)
            {
                DebugStats(stopWatch,"Tracker");
            }
        }
        public async Task<bool> UpdateTracker(Spoiler? spoilerLog, bool debugStats = false)
        {
            if (spoilerLog == null || Maps == null || Tracker == null)
            {
                Debug.WriteLine("\n--------- Tracker FAILED to Update! ---------");
                Debug.WriteLineIf(IsNullOrEmpty(spoilerLog), "Spoilerlog is null/empty");
                Debug.WriteLineIf(IsNullOrEmpty(Maps), "Maps is null/empty");
                Debug.WriteLineIf(IsNullOrEmpty(Tracker), "Tracker is null/empty");
                return false;
            }

            Stopwatch stopWatch = Stopwatch.StartNew();
            int settingMatchesMap = 0;
            int mapMatchesItem = 0;
            int actualChanges = 0;

            // Get the setting
            foreach (Setting setting in spoilerLog.GameSettings) 
            { 
                string? settingName = setting.Name;
                string? settingValue = setting.Value;

                // Get the itemMap
                foreach (ItemMap itemMap in Maps) 
                {
                    string? mapName = itemMap.FullItemReference;
                    string? mapSpoilerLabel = itemMap.SpoilerLabel;
                    string? mapType = itemMap.Type;

                    // compare setting to item
                    if (settingName == mapSpoilerLabel) 
                    { 
                        settingMatchesMap++;

                        // get the Itempoly
                        foreach (Itempoly item in Tracker.ItemDatabase) 
                        {
                            string? itemName = item.ParsedItemReference;
                            string? cleanItemName = item.CleanItemReference;
                            string? itemType = item.Type;

                            // compare map to item
                            if (mapName == itemName && mapType == itemType)
                            {
                                mapMatchesItem++;
                                // Attempt to get setting Value from item map
                                if (itemMap.Values.TryGetValue(settingValue, out int mappedValue))
                                {
                                    
                                    // Convert the item, to get access its proper properties, so we can modify the value.
                                    switch (item)
                                    {
                                        case Toggle toggle:
                                            // For some reason, they decided 0 = true, 1 = false OKAY...
                                            bool oldToggle = !toggle.Active ?? false;
                                            bool newToggle = mappedValue > 0;

                                            // Changes the Value of Itempoly
                                            if (oldToggle != newToggle)
                                            {
                                                toggle.Active = newToggle;
                                                toggle.Changes = $"{!oldToggle} is now {!newToggle}";
                                                Debug.WriteLineIf(debugStats, $"{cleanItemName}\n\t{!oldToggle} is now {!newToggle}");
                                                actualChanges++;
                                            }
                                            break;

                                        case Progressive progressive:
                                            int oldProgressiveStage = progressive.StageIndex ?? 0;

                                            // Changes the Value of Itempoly
                                            if (oldProgressiveStage != mappedValue)
                                            {
                                                progressive.StageIndex = mappedValue;
                                                progressive.Changes = $"{oldProgressiveStage} is now {mappedValue}";
                                                Debug.WriteLineIf(debugStats, $"{cleanItemName}\n\t{oldProgressiveStage} is now {mappedValue}");
                                                actualChanges++;
                                            }
                                            break;

                                        case Consumable consumable:
                                            int oldCount = consumable.AcquiredCount ?? 0;

                                            // Changes the Value of Itempoly
                                            if (oldCount != mappedValue)
                                            {
                                                consumable.AcquiredCount = mappedValue;
                                                consumable.Changes = $"{oldCount} is now {mappedValue}";
                                                Debug.WriteLineIf(debugStats, $"{cleanItemName}\n\t{oldCount} is now {mappedValue}");
                                                actualChanges++;
                                            }
                                            break;

                                        case Lua lua:
                                            double? oldLuaStage = lua.Stage;

                                            // Changes the Value of Itempoly
                                            if (oldLuaStage != mappedValue)
                                            {
                                                lua.Stage = mappedValue;
                                                lua.Changes = $"{oldLuaStage} is now {mappedValue}";
                                                Debug.WriteLineIf(debugStats, $"{cleanItemName}\n\t{oldLuaStage} is now {mappedValue}");
                                                actualChanges++;
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    Debug.WriteLineIf(debugStats,
                                    $"*** BAD/DUPLICATE MAP VALUES***\n" +
                                    $"Itempoly:\t\t{cleanItemName}\t= {itemType}\n" +
                                    $"Map:\t\t{mapName}\t= {mapType}\n" +
                                    $"Setting:\t{settingName} = {settingValue}\n");
                                }
                                
                            }

                        }

                    }
                }
            }

            foreach (Trick trick in spoilerLog.Tricks)
            {
                string? trickName = trick.Name;
                string? trickValue = trick.Value;

                // Get the itemMap
                foreach (ItemMap itemMap in Maps)
                {
                    string? mapName = itemMap.FullItemReference;
                    string? mapSpoilerLabel = itemMap.SpoilerLabel;
                    string? mapType = itemMap.Type;


                    if (mapSpoilerLabel == "Access Adult Spirit as Child using Hover Boots")
                    { 
                        int test = 1; 
                    }
                    // compare setting to item
                    if (string.Equals(trickName, mapSpoilerLabel,StringComparison.OrdinalIgnoreCase))
                    {
                        settingMatchesMap++;

                        // get the Itempoly
                        foreach (Itempoly item in Tracker.ItemDatabase)
                        {
                            string? itemName = item.ParsedItemReference;
                            string? cleanItemName = item.CleanItemReference;
                            string? itemType = item.Type;

                            // compare map to item
                            if (mapName == itemName && mapType == itemType)
                            {
                                mapMatchesItem++;
                                // Attempt to get setting Value from item map
                                if (itemMap.Values.TryGetValue(trickValue, out int mappedValue))
                                {

                                    // Convert the item, to get access its proper properties, so we can modify the value.
                                    switch (item)
                                    {
                                        case Toggle toggle:
                                            // For some reason, they decided 0 = true, 1 = false OKAY...
                                            bool oldToggle = !toggle.Active ?? false;
                                            bool newToggle = mappedValue > 0;

                                            // Changes the Value of Itempoly
                                            if (oldToggle != newToggle)
                                            {
                                                toggle.Active = newToggle;
                                                toggle.Changes = $"{!oldToggle} is now {!newToggle}";
                                                Debug.WriteLineIf(debugStats, $"{cleanItemName}\n\t{!oldToggle} is now {!newToggle}");
                                                actualChanges++;
                                            }
                                            break;

                                        case Progressive progressive:
                                            int oldProgressiveStage = progressive.StageIndex ?? 0;

                                            // Changes the Value of Itempoly
                                            if (oldProgressiveStage != mappedValue)
                                            {
                                                progressive.StageIndex = mappedValue;
                                                progressive.Changes = $"{oldProgressiveStage} is now {mappedValue}";
                                                Debug.WriteLineIf(debugStats, $"{cleanItemName}\n\t{oldProgressiveStage} is now {mappedValue}");
                                                actualChanges++;
                                            }
                                            break;

                                        case Consumable consumable:
                                            int oldCount = consumable.AcquiredCount ?? 0;

                                            // Changes the Value of Itempoly
                                            if (oldCount != mappedValue)
                                            {
                                                consumable.AcquiredCount = mappedValue;
                                                consumable.Changes = $"{oldCount} is now {mappedValue}";
                                                Debug.WriteLineIf(debugStats, $"{cleanItemName}\n\t{oldCount} is now {mappedValue}");
                                                actualChanges++;
                                            }
                                            break;

                                        case Lua lua:
                                            double? oldLuaStage = lua.Stage;

                                            // Changes the Value of Itempoly
                                            if (oldLuaStage != mappedValue)
                                            {
                                                lua.Stage = mappedValue;
                                                lua.Changes = $"{oldLuaStage} is now {mappedValue}";
                                                Debug.WriteLineIf(debugStats, $"{cleanItemName}\n\t{oldLuaStage} is now {mappedValue}");
                                                actualChanges++;
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    Debug.WriteLineIf(debugStats,
                                    $"*** DUPLICATE MAP NAME***\n" +
                                    $"Itempoly:\t\t{cleanItemName}\t= {itemType}\n" +
                                    $"Map:\t\t{mapName}\t= {mapType}\n" +
                                    $"Setting:\t{trickName} = {trickValue}\n");
                                    
                                }

                            }

                        }

                    }
                }
            }

            stopWatch.Stop();

            Debug.WriteLineIf(debugStats,
                $"----------- Tracker Updates ------------" +
                $"\nSet Matches Map:\t{settingMatchesMap}" +
                $"\nMap Matches Itempoly:\t{mapMatchesItem}" +
                $"\nChanges Made:\t\t{actualChanges}" +
                $"\nTime:\t\t\t\t{stopWatch.Elapsed}");

            return true;
        }
        public async Task ExportTracker(string filePath)
        {
            if (Tracker == null)
            {
                Debug.WriteLine("Tracker is null, cannot save.");
                return;
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,  // Makes the JSON nicely formatted
                Converters = { new ItemJsonConverter() }, // Add your custom converter if needed
            };

            try
            {
                using FileStream stream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(stream, Tracker, options);
                Debug.WriteLine($"Tracker saved successfully to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save Tracker to {filePath}: {ex.Message}");
            }
        }

        public static bool IsNullOrEmpty(object obj)
        {
            if (obj == null)
                return true;

            if (obj is string str)
                return string.IsNullOrWhiteSpace(str);

            if (obj is ICollection collection)
                return collection.Count == 0;

            if (obj is IEnumerable enumerable)
                return !enumerable.Cast<object>().Any();

            return false;
        }

        private void DebugStats(Stopwatch stopWatch, string sender)
        {
            stopWatch.Stop();
            if (sender == "Maps")
            {
                Debug.WriteLine(
                $"--- {sender} Imported! ---" +
                $"\nMaps:\t\t\t\t{Maps.Count}" +
                $"\nTime Taken:\t\t\t{stopWatch.Elapsed}"
            );
            }
            else if (sender == "Tracker")
            {
                Debug.WriteLine(
                $"--- {sender} Imported! ---" +
                $"\nToggles:\t\t\t{Toggles.Count}" +
                $"\nProgressives:\t\t{Progressives.Count}" +
                $"\nConsumables:\t\t{Consumables.Count}" +
                $"\nLuas:\t\t\t\t{Luas.Count}" +
                $"\nLocations:\t\t\t{Locations.Count}" +
                $"\nAllItems:\t\t\t{AllItems.Count}" +
                $"\nTime Taken:\t\t\t{stopWatch.Elapsed}\n"
                );
            }
            
        }
    }
}
