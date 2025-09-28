using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;
using TranslationLibrary.SpoilerLog.Controller;
using TranslationLibrary.Emotracker.Models;
using TranslationLibrary.Emotracker.Models.Items;
using TranslationLibrary.Emotracker.Models.Locations;
using Item = TranslationLibrary.Emotracker.Models.Items.Item;


namespace TranslationLibrary.Emotracker.Controller
{
    public class EmoTracker
    {
        private readonly string TemplatePath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\TranslationLibrary\Emotracker\Resources\TemplateTracker - SoulShuffle.json")
        );
        public Tracker? Tracker { get; set; }

        #region Collections
        public List<Toggle> Toggles { get; private set; } = new();
        public List<Progressive> Progressives { get; private set; } = new();
        public List<Consumable> Consumables { get; private set; } = new();
        public List<Lua> Luas { get; private set; } = new();
        public List<Location> Locations { get; private set; } = new();
        #endregion

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
                foreach (Item item in Tracker.ItemDatabase)
                {
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
                Locations = Tracker.LocationDatabase.Locations;
            }

            if (showDebugStats)
            {
                DebugStats(stopWatch);
            }
        }



        public async Task UpdateTracker(Spoiler log)
        {

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
