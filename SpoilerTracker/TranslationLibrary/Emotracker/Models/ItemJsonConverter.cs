using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TranslationLibrary.Emotracker.Models.Items;

namespace TranslationLibrary.Emotracker.Models
{
    public class ItemJsonConverter : JsonConverter<Item>
    {
        public override Item? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            if (!root.TryGetProperty("item_reference", out var itemRefProp))
            {
                throw new JsonException("Missing item_reference");
            }

            var itemRef = itemRefProp.GetString();
            if (itemRef == null)
                throw new JsonException("item_reference is null");

            // Extract type from item_reference (e.g., 626:progressive:Skeleton Key)
            var parts = itemRef.Split(':');
            if (parts.Length < 2)
                throw new JsonException($"Invalid item_reference format: {itemRef}");

            var type = parts[1].ToLowerInvariant();

            return type switch
            {
                "toggle" or "toggle_badged" => JsonSerializer.Deserialize<Toggle>(root.GetRawText(), options),
                "progressive" => JsonSerializer.Deserialize<Progressive>(root.GetRawText(), options),
                "consumable" => JsonSerializer.Deserialize<Consumable>(root.GetRawText(), options),
                "lua" => JsonSerializer.Deserialize<Lua>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown item type '{type}' in item_reference")
            };
        }
        public override void Write(Utf8JsonWriter writer, Item value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }
}
