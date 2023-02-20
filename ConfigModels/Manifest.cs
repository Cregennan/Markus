using Newtonsoft.Json;

namespace Markus.Configmodels
{


    /// <summary>
    /// Application project manifest file
    /// </summary>
    public partial class Manifest
    {
        /// <summary>
        /// Входной файл приложения
        /// </summary>
        [JsonProperty("entrypoint", Required = Required.Always)]
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        public string Entrypoint { get; set; }

        /// <summary>
        /// Нумеровать заголовки (кроме специальных, вроде "Приложение", "Введение" и т.д.
        /// </summary>
        [JsonProperty("enumerateHeadings", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool? EnumerateHeadings { get; set; }

        /// <summary>
        /// Название проекта
        /// </summary>
        [JsonProperty("projectName", Required = Required.Always)]
        [JsonConverter(typeof(FluffyMinMaxLengthCheckConverter))]
        public string ProjectName { get; set; }

        /// <summary>
        /// Рекурсивное добавление других файлов Markdown в рабочий (по умолчанию - false) (не
        /// поддерживается на данный момент)
        /// </summary>
        [JsonProperty("recursive", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool? Recursive { get; set; }


        /// <summary>
        /// Добавление титульной страницы
        /// </summary>
        [JsonProperty("includeTitle", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool? IncludeTitle { get; set; }


        /// <summary>
        /// Название файла с шаблоном оформления
        /// </summary>
        [JsonProperty("template", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(TentacledMinMaxLengthCheckConverter))]
        public string Template { get; set; }
    }

    public partial class Manifest
    {
        internal class PurpleMinMaxLengthCheckConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(string);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                var value = serializer.Deserialize<string>(reader);
                if (value.Length >= 1)
                {
                    return value;
                }
                throw new Exception("Cannot unmarshal type string");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                var value = (string)untypedValue;
                if (value.Length >= 1)
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
                if (value.Length >= 6)
                {
                    return value;
                }
                throw new Exception("Cannot unmarshal type string");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                var value = (string)untypedValue;
                if (value.Length >= 6)
                {
                    serializer.Serialize(writer, value);
                    return;
                }
                throw new Exception("Cannot marshal type string");
            }

            public static readonly FluffyMinMaxLengthCheckConverter Singleton = new FluffyMinMaxLengthCheckConverter();
        }

        internal class TentacledMinMaxLengthCheckConverter : JsonConverter
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

            public static readonly TentacledMinMaxLengthCheckConverter Singleton = new TentacledMinMaxLengthCheckConverter();
        }
    }

    
}