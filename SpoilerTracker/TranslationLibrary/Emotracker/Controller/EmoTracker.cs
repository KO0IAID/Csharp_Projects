using System;
using System.Collections.Generic;
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
using Item = TranslationLibrary.Emotracker.Models.Items.Item;


namespace TranslationLibrary.Emotracker.Controller
{
    public class EmoTracker
    {
        public string? TemplatePath {  get; private set; }
        public Tracker? OriginalTracker { get; private set; }
        public Tracker? UpdatedTracker { get; private set; }

        #region Collections
        public List<Item> AllItems { get; private set; } = new();
        public List<Toggle> Toggles { get; private set; } = new();
        public List<Progressive> Progressives { get; private set; } = new();
        public List<Consumable> Consumables { get; private set; } = new();
        public List<Lua> Luas { get; private set; } = new();
        public List<Location> Locations { get; private set; } = new();
        #endregion

        #region Maps
        public Dictionary<string, ItemMap>? Map { get; private set; }
        public List<ItemMap> MapItems { get; private set; } = new();

        #endregion

        public void SetTemplatePath(string filePath = null) 
        {
            if (filePath == null)
            {
                TemplatePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\TranslationLibrary\Emotracker\Resources\TemplateTracker - SoulShuffle.json"));
            }
            else
            { 
                TemplatePath = filePath;
            }
        }
        public async Task ImportMap(string filePath) 
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Mapping JSON not found.", filePath);

            using FileStream mapStream = File.OpenRead(filePath);

            var mapOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var mappingDictionary = await JsonSerializer.DeserializeAsync<Dictionary<string, ItemMap>>(mapStream, mapOptions);

            if (mappingDictionary == null)
            {
                Debug.WriteLine("Failed to deserialize JSON mapping file.");
                return;
            }

            foreach ((string spoilerSetting, ItemMap item) in mappingDictionary)
            {
                item.ItemReference = spoilerSetting;
                item.Initialize();
                MapItems.Add(item);
            }

            Map = mappingDictionary;
        }
        public async Task ImportTracker(string? filePath = null, bool showDebugStats = false)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            string pathToUse = filePath ?? TemplatePath;

            if (!File.Exists(pathToUse))
                throw new FileNotFoundException("Template file not found.", pathToUse);

            // Open the JSON file for reading
            using var stream = File.OpenRead(pathToUse);

            // Register custom converter for Item
            var options = new JsonSerializerOptions
            {
                Converters = { new ItemJsonConverter() },
                PropertyNameCaseInsensitive = true
            };

            // Deserialize the tracker JSON using your OriginalTracker class
            OriginalTracker = await JsonSerializer.DeserializeAsync<Tracker>(stream, options);

            if (OriginalTracker == null)
                return;

            // Clear existing collections before repopulating (optional safety)
            Toggles?.Clear();
            Progressives?.Clear();
            Consumables?.Clear();
            Luas?.Clear();
            Locations?.Clear();

            // Process item_database
            if (OriginalTracker.ItemDatabase != null)
            {
                foreach (Item item in OriginalTracker.ItemDatabase)
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
            if (OriginalTracker.LocationDatabase?.Locations != null)
            {
                foreach (var location in OriginalTracker.LocationDatabase.Locations)
                {
                    location.Initialize(); // Will also initialize its sections
                }

                Locations = OriginalTracker.LocationDatabase.Locations;
            }

            if (showDebugStats)
            {
                DebugStats(stopWatch);
            }
        }
        public async Task<bool> UpdateTracker(Spoiler log)
        {

            //  Map.Key         =   goldSkulltulaTokens     (Spoiler log name)
            //  Map.Value       =   ItemMap
            //  ItemMap.key     =   ItemReference           GoldSkulltulaTokensShuffle (emotracker name)
            //  ItemMap.Value   =   dictionary of [string, int]

            if (OriginalTracker == null || log == null)
            {
                Debug.WriteLine("Tracker or spoiler log is null.");
                return false;
            }

            if (Map == null)
            {
                Debug.WriteLine("Mapping dictionary (Map) is null.");
                return false;
            }

            int changeCount = 0;

            foreach (var setting in log.GameSettings)
            {
                if (string.IsNullOrWhiteSpace(setting?.Name) || string.IsNullOrWhiteSpace(setting?.Value))
                    continue;

                string settingName = setting.Name.Trim();
                string settingValue = setting.Value.Trim();

                if (!Map.TryGetValue(settingName, out var mapEntry))
                    continue;

                if (!mapEntry.Values.TryGetValue(settingValue, out int mappedValue))
                    continue;

                var item = OriginalTracker.ItemDatabase?.FirstOrDefault(i =>
                    !string.IsNullOrWhiteSpace(i.ItemReference) &&
                    i.ItemReference.Equals(mapEntry.ItemReference, StringComparison.OrdinalIgnoreCase));

                if (item == null)
                    continue;

                string cleanName = GetCleanName(item); // Gets final part of item reference, e.g. "Hookshot"

                switch (item)
                {
                    case Toggle toggle:
                        bool newToggle = mappedValue > 0;
                        if (toggle.Active != newToggle)
                        {
                            toggle.Active = newToggle;
                            LogSimpleChange(settingName, settingValue, cleanName, newToggle.ToString());
                            changeCount++;
                        }
                        break;

                    case Progressive progressive:
                        if (progressive.StageIndex != mappedValue)
                        {
                            progressive.StageIndex = mappedValue;
                            LogSimpleChange(settingName, settingValue, cleanName, mappedValue.ToString());
                            changeCount++;
                        }
                        break;

                    case Consumable consumable:
                        if (consumable.AcquiredCount != mappedValue)
                        {
                            consumable.AcquiredCount = mappedValue;
                            LogSimpleChange(settingName, settingValue, cleanName, mappedValue.ToString());
                            changeCount++;
                        }
                        break;

                    case Lua lua:
                        if (lua.Stage != mappedValue)
                        {
                            lua.Stage = mappedValue;
                            LogSimpleChange(settingName, settingValue, cleanName, mappedValue.ToString());
                            changeCount++;
                        }
                        break;
                }
            }

            Debug.WriteLine($"--- Tracker updated from spoiler log ---");
            Debug.WriteLine($"Total changes applied: {changeCount}");

            return changeCount > 0;
        }



        private Item? FindItemByReference(string? itemReference)
        {
            if (string.IsNullOrWhiteSpace(itemReference))
                return null;


            return OriginalTracker?.ItemDatabase?.FirstOrDefault(i =>
                !string.IsNullOrEmpty(i.ItemReference) &&
                i.ItemReference.Replace(" ", "").EndsWith(itemReference, StringComparison.OrdinalIgnoreCase));
        }
        private string GetCleanName(Item item)
        {
            // Return the final part of the item reference, e.g., "Hookshot" from "837:progressive:Hookshot"
            return item.ItemReference?.Split(':').Last() ?? "Unknown";
        }
        private void LogSimpleChange(string settingName, string settingValue, string itemName, string newValue)
        {
            Debug.WriteLine($"[Change] {settingName}, {settingValue} → {itemName}, {newValue}");
        }


        private void DebugStats(Stopwatch stopWatch)
        {
            stopWatch.Stop();
            Debug.WriteLine(
            $"--- EmoTracker Template Imported! ---" +
            $"\nToggles:\t\t\t{Toggles.Count}" +
            $"\nProgressives:\t\t{Progressives.Count}" +
            $"\nConsumables:\t\t{Consumables.Count}" +
            $"\nLuas:\t\t\t\t{Luas.Count}" +
            $"\nLocations:\t\t\t{Locations.Count}" +
            $"\nTime Taken:\t\t\t{stopWatch.Elapsed}"
            );
        }
    }
}
