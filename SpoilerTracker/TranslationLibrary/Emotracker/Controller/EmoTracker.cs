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
using TranslationLibrary.SpoilerLog.Models;


namespace TranslationLibrary.Emotracker.Controller
{
    public class EmoTracker
    {
        public Tracker? Tracker { get; private set; }
        public Spoiler? Spoiler { get; private set; }
        public List<ItemMap>? Maps { get; private set; }
        public int? ChangeCount { get; private set; }
        public string? ChangeLog { get; private set; }

        public async Task ConvertSpoilerToEmotracker(string spoilerPath, string trackerTemplatePath, string[] mapPaths, string outputPath, bool showDebug = false) 
        {
            ChangeCount = 0;
            Stopwatch sw = Stopwatch.StartNew();

            await ImportSpoiler(spoilerPath);
            await ImportTracker(spoilerPath);
            await ImportMaps(mapPaths);
            await UpdateTracker(Spoiler,showDebug);
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
            await UpdateTracker(Spoiler, showDebug);
            await ExportTracker(outputPath);

            if (showDebug) { DebugStats(sw); }
        }
        public async Task ImportSpoiler(string filePath)
        {
            Spoiler = new Spoiler();
            await Spoiler.AddFileContents(filePath);
            if (Spoiler == null)
            {
                Debug.WriteLine("Failed to Import Spoiler");
            }
        }
        public void ImportSpoiler(Spoiler spoiler) 
        {
            Spoiler = spoiler;
        }
        public async Task ImportMaps(string[] filePaths)
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
        public async Task ImportTracker(string filePath)
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
        public async Task UpdateTracker(Spoiler? spoilerLog, bool debugStats = false)
        {
            await Task.Run(() =>
            {

                if (spoilerLog == null || Maps == null || Tracker == null)
                {
                    Debug.WriteLine("\n--------- Tracker FAILED to Update! ---------");
                    Debug.WriteLineIf(spoilerLog == null, "Spoilerlog is null/empty");
                    Debug.WriteLineIf(Maps == null, "Maps is null/empty");
                    Debug.WriteLineIf(Tracker == null, "Tracker is null/empty");
                    return;
                }

                Stopwatch stopWatch = Stopwatch.StartNew();
                int settingMatchesMap = 0;
                int mapMatchesItem = 0;

                // Get the setting
                if (spoilerLog.GameSettings != null && Tracker.ItemDatabase != null)
                {
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

                                // get the Item
                                for (int i = 0; i < Tracker.ItemDatabase.Count; i++)
                                {
                                    Item item = Tracker.ItemDatabase[i];
                                    string? itemName = item.ParsedItemReference;
                                    string? cleanItemName = item.CleanItemReference;
                                    string? itemType = item.Type;

                                    // compare map to item
                                    if (mapName == itemName && mapType == itemType)
                                    {
                                        mapMatchesItem++;

                                        // Attempt to get setting Value from item map
                                        if (settingValue != null && itemMap.Values != null)
                                        {
                                            itemMap.Values.TryGetValue(settingValue, out int mappedValue);

                                            switch (itemType)
                                            {
                                                case "progressive":
                                                    if (mappedValue != item.StageIndex)
                                                    {
                                                        item.StageIndex = mappedValue;
                                                        item.NewValue = mappedValue.ToString();
                                                        ChangeCount++;
                                                        ChangeLog += $"Original: {item.OldValue}\t\t\tChange: {item.NewValue}\t\tEmo: {itemName}\t\n";
                                                    }
                                                    break;

                                                case "toggle":
                                                    bool newToggleValue = mappedValue > 0 ? false : true;

                                                    if (item.Active != newToggleValue)
                                                    {
                                                        item.Active = newToggleValue;
                                                        item.NewValue = mappedValue.ToString();
                                                        ChangeCount++;
                                                        ChangeLog += $"Original: {(item.OldValue)}\t\tChange: {item.NewValue}\t\tEmo: {itemName}\t\n";
                                                    }
                                                    break;

                                                case "consumable":
                                                    if (mappedValue != item.AcquiredCount)
                                                    {
                                                        item.AcquiredCount = mappedValue;
                                                        item.NewValue = mappedValue.ToString();
                                                        ChangeLog += $"Original: {item.OldValue}\t\t\tChange: {item.NewValue}\t\tEmo: {itemName}\t\n";
                                                    }
                                                    break;

                                                case "lua":
                                                    bool newLuaValue = mappedValue > 0 ? false : true;

                                                    if (item.Active != newLuaValue)
                                                    {
                                                        item.Active = newLuaValue;
                                                        item.NewValue = mappedValue.ToString();
                                                        ChangeCount++;
                                                        ChangeLog += $"Original: {(item.OldValue)}\t\tChange: {item.NewValue}\t\tEmo: {itemName}\t\n";
                                                    }
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            Debug.WriteLineIf(debugStats,
                                            $"*** BAD/DUPLICATE MAP VALUES ***\n" +
                                            $"Item:\t\t{itemName}\n" +
                                            $"Map:\t\t{mapName}\n" +
                                            $"Setting:\t{settingName} Value:{settingValue}\n" +
                                            $"********************************\n");
                                        }

                                    }

                                }

                            }
                        }
                    }

                }


                // Get the Trick 
                if (spoilerLog.Tricks != null && Tracker.ItemDatabase != null)
                {
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

                            // compare setting to item
                            if (string.Equals(trickName, mapSpoilerLabel, StringComparison.OrdinalIgnoreCase))
                            {
                                settingMatchesMap++;

                                // get the Item
                                foreach (Item item in Tracker.ItemDatabase)
                                {
                                    string? itemName = item.ParsedItemReference;
                                    string? cleanItemName = item.CleanItemReference;
                                    string? itemType = item.Type;

                                    // compare map to item
                                    if (mapName == itemName && mapType == itemType)
                                    {
                                        mapMatchesItem++;
                                        // Attempt to get setting Value from item map
                                        if (trickValue != null && itemMap.Values != null)
                                        {
                                            itemMap.Values.TryGetValue(trickValue, out int mappedValue);

                                            switch (itemType)
                                            {
                                                case "progressive":
                                                    if (mappedValue != item.StageIndex)
                                                    {
                                                        item.StageIndex = mappedValue;
                                                        item.NewValue = mappedValue.ToString();
                                                        ChangeCount++;
                                                        ChangeLog += $"Original: {item.OldValue}\t\t\tChange: {item.NewValue}\t\tEmo: {itemName}\t\n";
                                                    }
                                                    break;

                                                case "toggle":
                                                    bool newToggleValue = mappedValue > 0 ? false : true;

                                                    if (item.Active != newToggleValue)
                                                    {
                                                        item.Active = newToggleValue;
                                                        item.NewValue = mappedValue.ToString();
                                                        ChangeCount++;
                                                        ChangeLog += $"Original: {item.OldValue}\t\tChange: {item.NewValue}\t\tEmo: {itemName}\t\n";
                                                    }
                                                    break;

                                                case "consumable":
                                                    if (mappedValue != item.AcquiredCount)
                                                    {
                                                        item.AcquiredCount = mappedValue;
                                                        item.NewValue = mappedValue.ToString();
                                                        ChangeCount++;
                                                        ChangeLog += $"Original: {item.OldValue}\t\t\tChange: {item.NewValue}\t\tEmo: {itemName}\t\n";
                                                    }
                                                    break;

                                                case "lua":
                                                    bool newLuaValue = mappedValue > 0 ? false : true;

                                                    if (item.Active != newLuaValue)
                                                    {
                                                        item.Active = newLuaValue;
                                                        item.NewValue = mappedValue.ToString();
                                                        ChangeCount++;
                                                        ChangeLog += $"Original: {item.OldValue}\t\tChange: {item.NewValue}\t\tEmo: {itemName}\t\n";
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
                }
            });
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
        private void DebugStats(Stopwatch stopWatch)
        {
            stopWatch.Stop();

                Debug.WriteLine(
                $"--- Spoiler to Emotracker Converted! ---" +
                $"\nSpoiler:\t\t\t{Spoiler != null}"+
                $"\nMaps:\t\t\t\t{(Maps != null ? Maps.Count : 0)}" +
                $"\nTracker Items:\t\t{(Tracker?.ItemDatabase?.Count ?? 0)}" +
                $"\nTracker Locations:\t{(Tracker?.LocationDatabase?.Count ?? 0)}" +
                $"\nChanges:\t\t\t{ChangeCount}" +
                $"\nTime Taken:\t\t\t{stopWatch.Elapsed}"
            );            
        }
    }
}
