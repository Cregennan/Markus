namespace Markus.Configmodels
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Описание шаблонов проекта
    /// </summary>
    public partial class Themes
    {
        [JsonProperty("definitions", Required = Required.DisallowNull)]
        public Theme[] Definitions { get; set; }
    }

    public partial class Theme
    {
        /// <summary>
        /// Название файла шаблона
        /// </summary>
        [JsonProperty("file", Required = Required.Always)]
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        public string File { get; set; }

        /// <summary>
        /// Описание файла шаблона
        /// </summary>
        [JsonProperty("outputText", Required = Required.Always)]
        [JsonConverter(typeof(FluffyMinMaxLengthCheckConverter))]
        public string OutputText { get; set; }


        [JsonProperty("defaultTheme")]
        public bool Default { get; set; }

    }

    public partial class Themes
    {
        public static Themes FromJson(string json) => JsonConvert.DeserializeObject<Themes>(json, Markus.Configmodels.Theme.Converter.Settings);
    }

    partial class Theme
    {
        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

        internal class PurpleMinMaxLengthCheckConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(string);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                var value = serializer.Deserialize<string>(reader);
                if (value.Length >= 4)
                {
                    return value;
                }
                throw new Exception("Cannot unmarshal type string");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                var value = (string)untypedValue;
                if (value.Length >= 4)
                {
                    serializer.Serialize(writer, value);
                    return;
                }
                throw new Exception("Cannot marshal type string");
            }

            public static readonly PurpleMinMaxLengthCheckConverter Singleton = new PurpleMinMaxLengthCheckConverter();
        }

        internal class FluffyMinMaxLengthCheckConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(string);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                var value = serializer.Deserialize<string>(reader);
                if (value.Length >= 5)
                {
                    return value;
                }
                throw new Exception("Cannot unmarshal type string");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                var value = (string)untypedValue;
                if (value.Length >= 5)
                {
                    serializer.Serialize(writer, value);
                    return;
                }
                throw new Exception("Cannot marshal type string");
            }

            public static readonly FluffyMinMaxLengthCheckConverter Singleton = new FluffyMinMaxLengthCheckConverter();
        }
    }
}
