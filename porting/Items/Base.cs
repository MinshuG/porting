using System;
using System.Collections.Generic;
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

    public class ExportBase : Base
    {
        public string DisplayName { get; set; }
        public List<Mesh> ActualMeshes { get; set; }
        private string SaveDir;

        protected internal override void WriteJson(JsonWriter writer, JsonSerializer serializer)
        {
            base.WriteJson(writer, serializer);

            writer.WritePropertyName("DisplayName");
            writer.WriteValue(DisplayName);

            writer.WritePropertyName("SaveDir");
            writer.WriteValue(SaveDir);

            writer.WritePropertyName("Meshes");
            writer.WriteStartArray();
            {
                foreach (var j in ActualMeshes)
                {
                    serializer.Serialize(writer, j);
                }
            }
            writer.WriteEndArray();
        }

        public void SaveToDisk(string path)
        {
            if (ActualMeshes.Count == 0)
                return;
            SaveDir = path;

            var dirinfo = new DirectoryInfo(path);
            foreach (var mesh in ActualMeshes)
            {
                mesh.SaveToDisk(dirinfo);
            }

            var j = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(Path.Join(path, DisplayName + ".json"), j);
        }
    }
}