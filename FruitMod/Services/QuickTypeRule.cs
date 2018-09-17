using System;
using System.Globalization;
using FruitMod.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QuickType
{
    [SetService]
    public partial class Welcome
    {
        [JsonProperty("file_url")] public string FileUrl { get; set; }
    }

    public partial class Welcome
    {
        public static Welcome[] FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Welcome[]>(json, Converter.Settings);
        }
    }

    public static class Serialize
    {
        public static string ToJson(this Welcome[] self)
        {
            return JsonConvert.SerializeObject(self, Converter.Settings);
        }
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
            }
        };
    }

    internal class PurpleParseStringConverter : JsonConverter
    {
        public static readonly PurpleParseStringConverter Singleton = new PurpleParseStringConverter();

        public override bool CanConvert(Type t)
        {
            return t == typeof(long) || t == typeof(long?);
        }

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (long.TryParse(value, out var l)) return l;
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (long) untypedValue;
            serializer.Serialize(writer, value.ToString());
        }
    }

    internal class FluffyParseStringConverter : JsonConverter
    {
        public static readonly FluffyParseStringConverter Singleton = new FluffyParseStringConverter();

        public override bool CanConvert(Type t)
        {
            return t == typeof(bool) || t == typeof(bool?);
        }

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            bool b;
            if (bool.TryParse(value, out b)) return b;
            throw new Exception("Cannot unmarshal type bool");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (bool) untypedValue;
            var boolString = value ? "true" : "false";
            serializer.Serialize(writer, boolString);
        }
    }
}