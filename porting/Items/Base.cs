using System;
using System.IO;
using CUE4Parse.Utils;
using Newtonsoft.Json;

namespace porting.Models
{
    [JsonConverter(typeof(Converter))]
    public class Base
    {
        protected internal virtual void WriteJson(JsonWriter writer, JsonSerializer serializer)
        {
        }
        
        protected string FixAndCreatePath(DirectoryInfo baseDirectory, string fullPath, string? ext = null)
        {
            if (fullPath.StartsWith("/")) fullPath = fullPath[1..];
            var ret = Path.Combine(baseDirectory.FullName, fullPath) + (ext != null ? $".{ext.ToLower()}" : "");
            Directory.CreateDirectory(ret.Replace('\\', '/').SubstringBeforeLast('/'));
            return ret;
        }
    }

    public class Converter : JsonConverter<Base>
    {
        public override void WriteJson(JsonWriter writer, Base value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            value.WriteJson(writer, serializer);
            writer.WriteEndObject();
        }

        public override Base ReadJson(JsonReader reader, Type objectType, Base existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

}