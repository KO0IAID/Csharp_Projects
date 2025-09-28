using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;
using TranslationLibrary.SpoilerLog.Controller;
using TranslationLibrary.SpoilerLog.Models;
using TranslationLibrary.Emotracker.Models.Json;


namespace TranslationLibrary.Emotracker.Controller
{
    public class Tracker
    {
        private readonly string TemplatePath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\TranslationLibrary\Emotracker\Resources\TemplateTracker.json")
        );
        public Root? EmoTracker { get; set; }
        #region Collections
        public List<Item> Toggles { get; private set; } = new();
        public List<Item> ProgressiveToggles { get; private set; } = new();
        public List<Item> ConsumableItems { get; private set; } = new();
        public List<Item> LuaItems { get; private set; } = new();
        public List<Item> UnknownTypeItems { get; private set; } = new();
        public List<Item>? ItemLocations {get; set;}
        public List<Section>? Sections {get; set;}
        #endregion

        public async Task ImportTracker(string? filePath = null, bool showDebugStats = false) 
        {
            // Default: Null will pull from Resources folder, Optionally you can specify with your own path.
            Stopwatch stopWatch = Stopwatch.StartNew();
            string pathToUse = filePath ?? TemplatePath;

            if (File.Exists(pathToUse))
            { 
                using var stream = File.OpenRead(pathToUse);
                
                EmoTracker = await JsonSerializer.DeserializeAsync<Root>(stream);

                if (EmoTracker == null)
                    return;

                if (EmoTracker.ItemDatabase != null)
                {
                    foreach (Item item in EmoTracker.ItemDatabase)
                    {
                        string[] parts = item.ItemReference?.Split(':') ?? Array.Empty<string>();

                        if (parts.Length >= 3)
                        {
                            string type = parts[1].ToLowerInvariant();

                            switch (type)
                            {
                                case "toggle":
                                    Toggles.Add(item);
                                    break;
                                case "progressive":
                                    ProgressiveToggles.Add(item);
                                    break;
                                case "consumable":
                                    ConsumableItems.Add(item);
                                    break;
                                case "lua":
                                    LuaItems.Add(item);
                                    break;
                                default:
                                    UnknownTypeItems.Add(item);
                                    break;
                            }
                        }
                        else
                        {
                            UnknownTypeItems.Add(item); // malformed reference
                        }

                    }
                   
                }

                


                
            }
            else
            {
                throw new FileNotFoundException("Template file not found.", pathToUse);
            }

            if (showDebugStats) 
            {
                DebugStats(stopWatch);
            }
            
        }


        public async Task UpdateTracker(Spoiler log)
        {
            if (EmoTracker == null || EmoTracker.ItemDatabase == null)
                throw new InvalidOperationException("Tracker not imported yet.");

            // Step 1: Flatten spoiler log into dictionary
            var settingsDictionary = log.ToSettingsDictionary();

            // Step 2: Load mapping JSON (you can cache this like TemplatePath)
            string mappingPath = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\TranslationLibrary\Emotracker\Maps\TrackerToSpoilerMap.json")
            );

            var mappingJson = await File.ReadAllTextAsync(mappingPath);
            var mapping = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(mappingJson);

            if (mapping == null)
                throw new InvalidOperationException("Mapping file could not be loaded.");

            // Step 3: Apply spoiler settings into tracker
            foreach (var setting in settingsDictionary)
            {
                string spoilerKey = setting.Key;     // e.g. "goldSkulltulaTokens"
                string spoilerValue = setting.Value; // e.g. "overworld"

                if (!mapping.TryGetValue(spoilerKey, out var optionMap))
                    continue; // skip if no mapping exists

                if (!optionMap.TryGetValue(spoilerValue, out int mappedIndex))
                    continue; // skip if spoiler value not recognized

                foreach (var item in EmoTracker.ItemDatabase)
                {
                    if (item.ItemReference != null &&
                        item.ItemReference.Contains(spoilerKey, StringComparison.OrdinalIgnoreCase))
                    {
                        if (item.StageIndex.HasValue)
                            item.StageIndex = mappedIndex;
                        else if (item.Active.HasValue)
                            item.Active = (mappedIndex == 1);
                    }
                }

            }





        }

        private void DebugStats(Stopwatch stopWatch)
        {
            stopWatch.Stop();
            Debug.WriteLine(
            $"--- EmoTracker Template Imported! ---" +
            $"\nTime Taken:\t\t\t{stopWatch.Elapsed}"
            );
        }
    }
}
