using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TranslationLibrary.Emotracker.Models;
using TranslationLibrary.Emotracker.Models.Items;
using TranslationLibrary.Emotracker.Models.Locations;
using TranslationLibrary.SpoilerLog.Controller;
using TranslationLibrary.SpoilerLog.Interfaces;
using TranslationLibrary.SpoilerLog.Models;


namespace TranslationLibrary.Emotracker.Controller
{
    public class EmoTracker
    {
        public Tracker? Tracker { get; private set; }
        public Spoiler? Spoiler { get; private set; }
        public List<ItemMap>? Maps { get; private set; }
        public List<string>? SharedItems { get; private set; } = new();
        public int? ChangeCount { get; private set; }
        public string? ChangeLog { get; private set; }

        #region Core Functionality
        public async Task ConvertSpoilerToEmotracker(string spoilerPath, string trackerTemplatePath, string[] mapPaths, string outputPath, bool showDebug = false) 
        {
            ChangeCount = 0;
            Stopwatch sw = Stopwatch.StartNew();

            await ImportSpoiler(spoilerPath);
            await ImportTracker(spoilerPath);
            await ImportMaps(mapPaths);
            await UpdateTracker(showDebug);
            await ExportTracker(outputPath);

            if (showDebug) { DebugStats(sw); }

        }
        public async Task ConvertSpoilerToEmotracker(Spoiler spoiler, string trackerTemplatePath, string[] mapPaths, string outputPath, bool showDebug = false)
        {
            ChangeCount = 0;
            Stopwatch sw = Stopwatch.StartNew();

            ImportSpoiler(spoiler);
            await ImportTracker(trackerTemplatePath);
            await ImportMaps(mapPaths);
            await UpdateTracker(showDebug);
            await ExportTracker(outputPath);

            if (showDebug) { DebugStats(sw); }
        }

        private void DebugStats(Stopwatch stopWatch)
        {
            stopWatch.Stop();

            Debug.WriteLine(
            $"--- Spoiler to Emotracker Converted! ---" +
            $"\nSpoiler:\t\t\t{Spoiler != null}" +
            $"\nMaps:\t\t\t\t{(Maps != null ? Maps.Count : 0)}" +
            $"\nTracker Items:\t\t{(Tracker?.ItemDatabase?.Count ?? 0)}" +
            $"\nTracker Locations:\t{(Tracker?.LocationDatabase?.Count ?? 0)}" +
            $"\nChanges:\t\t\t{ChangeCount}" +
            $"\nTime Taken:\t\t\t{stopWatch.Elapsed}"
        );
        }
        #endregion
        #region File Imports & Export Processes
        private async Task ImportSpoiler(string filePath)
        {
            Spoiler = new Spoiler();
            await Spoiler.AddFileContents(filePath);
            if (Spoiler == null)
            {
                Debug.WriteLine("Failed to Import Spoiler");
            }
        }
        private void ImportSpoiler(Spoiler spoiler) 
        {
            Spoiler = spoiler;
        }
        private async Task ImportMaps(string[] filePaths)
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
        }
        private async Task ImportTracker(string filePath)
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
                PropertyNameCaseInsensitive = true
            };

            // Deserialize the tracker JSON using your Tracker class
            Tracker = await JsonSerializer.DeserializeAsync<Tracker>(stream, options);

            if (Tracker == null)
                return;

            if (Tracker.ItemDatabase != null) 
            {
                foreach (Item item in Tracker.ItemDatabase)
                {
                    item?.Initialize();
                }
            }
        }
        private async Task ExportTracker(string filePath)
        {
            if (Tracker == null)
            {
                Debug.WriteLine("Tracker is null, cannot save.");
                return;
            }

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Ignore null properties
                WriteIndented = true,  // Makes the JSON nicely formatted
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
        #endregion
        #region Mapping Processes
        private async Task UpdateTracker(bool debugStats = false)
        {
            await Task.Run(() =>
            {

                if (Spoiler == null || Maps == null || Tracker == null)
                {
                    Debug.WriteLine("\n--------- Tracker FAILED to Update! ---------");
                    Debug.WriteLineIf(Spoiler == null, "Spoilerlog is null/empty");
                    Debug.WriteLineIf(Maps == null, "Maps is null/empty");
                    Debug.WriteLineIf(Tracker == null, "Tracker is null/empty");
                    return;
                }

                MapGameSettings(Spoiler.GameSettings, "GameSettings", debugStats);
                MapSpecialConditions(Spoiler.SpecialConditions, "SpecialConditions", debugStats);
                MapItems(Spoiler.Tricks, "Tricks", debugStats);
                MapSharedItems();
                MapItems(Spoiler.StartingItems, "StartingItems", debugStats);
                
            });
        }
        private void MapGameSettings(List<Setting>? source, string sourceType, bool debugStats)
        {
            if (source == null || Maps == null || Tracker?.ItemDatabase == null)
                return;

            // get Spoiler log item
            foreach (Setting entry in source)
            {
                string? entryName = entry.Name;
                string? entryValue = entry.Value;
                int? entryCount = entry.Count;

                // get Map
                foreach (ItemMap itemMap in Maps)
                {
                    string? mapName = itemMap.NoIDItemReference;
                    string? mapSpoilerLabel = itemMap.SpoilerLabel;
                    string? mapType = itemMap.Type;

                    // Compare spoilerlog item to map
                    if (string.Equals(entryName, mapSpoilerLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (Item item in Tracker.ItemDatabase)
                        {
                            string? itemName = item.NoIDItemReference;
                            string? itemType = item.Type;

                            // Compare map to item
                            if (mapName == itemName && mapType == itemType)
                            {

                                if (entryValue != null && itemMap.Values != null)
                                {
                                    itemMap.Values.TryGetValue(entryValue, out int mappedValue);

                                    switch (itemType)
                                    {
                                        case "progressive":
                                            if (mappedValue != item.StageIndex)
                                            {
                                                if (mapSpoilerLabel.StartsWith("shared")) 
                                                { 
                                                    SharedItems.Add(mapSpoilerLabel);
                                                }
                                                item.StageIndex = mappedValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "toggle":
                                            bool newToggleValue = mappedValue <= 0;
                                            if (newToggleValue != item.Active)
                                            {
                                                item.Active = newToggleValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "toggle_badged":
                                            bool newBadgedValue = mappedValue <= 0;
                                            if (newBadgedValue != item.Active)
                                            {
                                                item.Active = newBadgedValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;
                                        case "lua":
                                            bool newLuaValue = mappedValue > 0;
                                            if (item.Active != newLuaValue)
                                            {
                                                item.Active = newLuaValue;
                                                item.Stage = (double)mappedValue;
                                                item.NewValue = newLuaValue.ToString() + "," + mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "consumable":
                                            if (entryCount != item.AcquiredCount)
                                            {
                                                item.AcquiredCount = entryCount;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    Debug.WriteLineIf(debugStats,
                                        $"*** BAD/DUPLICATE MAP VALUES ***\n" +
                                        $"Source:\t{sourceType}\n" +
                                        $"Item:\t{itemName}\n" +
                                        $"Map:\t{mapName}\n" +
                                        $"Entry:\t{entryName} Value:{entryValue}\n" +
                                        $"********************************\n");
                                }
                            }
                        }
                    }
                }
            }
        }
        private void MapItems<T>(IEnumerable<T>? source, string sourceType, bool debugStats) where T : INameValueCount
        {
            if (source == null || Maps == null || Tracker?.ItemDatabase == null)
                return;

            // get Spoiler log item
            foreach (T entry in source)
            {
                string? entryName = entry.Name;
                string? entryValue = entry.Value;
                int? entryCount = entry.Count;

                // get Map
                foreach (ItemMap itemMap in Maps)
                {
                    string? mapName = itemMap.NoIDItemReference;
                    string? mapSpoilerLabel = itemMap.SpoilerLabel;
                    string? mapType = itemMap.Type;

                    // Compare spoilerlog item to map
                    if (string.Equals(entryName, mapSpoilerLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (Item item in Tracker.ItemDatabase)
                        {
                            string? itemName = item.NoIDItemReference;
                            string? itemType = item.Type;

                            // Compare map to item
                            if (mapName == itemName && mapType == itemType)
                            {

                                if (entryValue != null && itemMap.Values != null)
                                {
                                    itemMap.Values.TryGetValue(entryValue, out int mappedValue);

                                    switch (itemType)
                                    {
                                        case "progressive":
                                            if (mappedValue != item.StageIndex)
                                            {
                                                item.StageIndex = mappedValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "toggle":
                                            bool newToggleValue = mappedValue <= 0;
                                            if (newToggleValue != item.Active)
                                            {
                                                item.Active = newToggleValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "toggle_badged":
                                            bool newBadgedValue = mappedValue <= 0;
                                            if (newBadgedValue != item.Active)
                                            {
                                                item.Active = newBadgedValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;
                                        case "lua":
                                            bool newLuaValue = mappedValue > 0;
                                            if (item.Active != newLuaValue)
                                            {
                                                item.Active = newLuaValue;
                                                item.Stage = (double)mappedValue;
                                                item.NewValue = newLuaValue.ToString() + "," + mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "consumable":
                                            if (entryCount != item.AcquiredCount)
                                            {
                                                item.AcquiredCount = entryCount;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    Debug.WriteLineIf(debugStats,
                                        $"*** BAD/DUPLICATE MAP VALUES ***\n" +
                                        $"Source:\t{sourceType}\n" +
                                        $"Item:\t{itemName}\n" +
                                        $"Map:\t{mapName}\n" +
                                        $"Entry:\t{entryName} Value:{entryValue}\n" +
                                        $"********************************\n");
                                }
                            }
                        }
                    }
                }
            }
        }
        private void MapSpecialConditions(List<Conditions>? source, string sourceType, bool debugStats)
        {
            if (source == null || Maps == null || Tracker?.ItemDatabase == null)
                return;

            // get Spoiler log item
            foreach (Conditions condition in source)
            {
                string? conditionName = condition.Name;
                string? conditionValue = condition.Value;
                string? conditionType = condition.Type;

                // get Map
                foreach (ItemMap itemMap in Maps)
                {
                    string? mapName = itemMap.NoIDItemReference;
                    string? mapSpoilerLabel = itemMap.SpoilerLabel;
                    string? mapType = itemMap.Type;
                    string? mapSpecialType = itemMap.SpecialType;

                    // Compare spoilerlog item to map
                    if (string.Equals(conditionName, mapSpoilerLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (Item item in Tracker.ItemDatabase)
                        {
                            string? itemName = item.NoIDItemReference;
                            string? itemType = item.Type;
                            string? itemSpecialType = item.SpecialType;
                            
                            // Compare map to item
                            if (mapSpecialType == itemSpecialType && mapType == itemType && itemName == mapName)
                            {
                                if (conditionValue != null && itemMap.Values != null)
                                {
                                    itemMap.Values.TryGetValue(conditionValue, out int mappedValue);

                                    switch (itemType)
                                    {
                                        case "progressive":
                                            if (mappedValue != item.StageIndex)
                                            {
                                                item.StageIndex = mappedValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "toggle":
                                            bool newToggleValue = mappedValue <= 0;
                                            if (newToggleValue != item.Active)
                                            {
                                                item.Active = newToggleValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;
                                        case "lua":
                                            bool newLuaValue = mappedValue <= 0;
                                            if (item.Active != newLuaValue)
                                            {
                                                item.Active = newLuaValue;
                                                item.Stage = (double)mappedValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "consumable":
                                            // Parse the condition value, and if able use it, otherwise use the original, with a fallback of 0
                                            mappedValue = int.TryParse(conditionValue, out int result) ? result: item.AcquiredCount ?? 0;
                                            if (mappedValue != item.AcquiredCount)
                                            {
                                                item.AcquiredCount = mappedValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    Debug.WriteLineIf(debugStats,
                                        $"*** BAD/DUPLICATE MAP VALUES ***\n" +
                                        $"Source:\t{sourceType}\n" +
                                        $"Item:\t{itemName}\n" +
                                        $"Map:\t{mapName}\n" +
                                        $"Entry:\t{conditionName} Value:{conditionValue}\n" +
                                        $"********************************\n");
                                }
                            }
                        }
                    }
                }
            }
        }
        private void MapSharedItems() 
        {
            if (SharedItems == null || Maps == null || Tracker.ItemDatabase == null)
                return;

            // Get the sharedItem
            foreach (string sharedItem in SharedItems) 
            { 
                // Get the Map
                foreach (ItemMap itemMap in Maps) 
                { 
                    string? mapShared = itemMap.Shared;
                    string? mapItemRef1 = itemMap.RawItemReference;
                    string? mapItemRef2 = itemMap.RawItemReference2;

                    int? onValue1 = itemMap.OnValue;
                    int? onValue2 = itemMap.OnValue2;

                    if (itemMap.Shared == sharedItem) 
                    { 
                        // Get the item from database
                        foreach (Item item in Tracker.ItemDatabase) 
                        {
                            string? itemName = item.ItemReference;
                            string? itemType = item.Type;

                            if (itemName == mapItemRef1 || itemName == mapItemRef2) 
                            {
                                if (itemName == "636:progressive:Hylian%20Shield" && item.StageIndex == 1)
                                {
                                    string test = "A";
                                }

                                switch (itemType)
                                {
                                    case "progressive":
                                        if (itemName == mapItemRef1) 
                                        {
                                            int? newValue1 = onValue1;
                                            if (newValue1 !=item.StageIndex) 
                                            {
                                                item.StageIndex = onValue1;
                                                item.NewValue = onValue1.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                        }
                                        else if (itemName == mapItemRef2) 
                                        {
                                            int? newValue2 = onValue2 != null ? onValue2 : onValue1;

                                            if (newValue2 != item.StageIndex) 
                                            {
                                                item.StageIndex = newValue2;
                                                item.NewValue = item.StageIndex.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

    }
}
