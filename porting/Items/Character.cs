using System;
using System.Collections.Generic;
using System.IO;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse_Conversion.Materials;
using CUE4Parse_Conversion.Meshes;
using Newtonsoft.Json;

namespace porting.Models
{
    public class Mesh : Base
    {
        public FSoftObjectPath MeshPath;
        public List<Material> OverrideMaterial;
        public string DiskPath;

        protected internal override void WriteJson(JsonWriter writer, JsonSerializer serializer)
        {
            base.WriteJson(writer, serializer);
            
            writer.WritePropertyName("MeshPath");
            serializer.Serialize(writer, MeshPath);

            writer.WritePropertyName("OverrideMaterials");
            writer.WriteStartArray();
            {
                foreach (var j in OverrideMaterial)
                {
                    serializer.Serialize(writer, j);
                }
            }
            writer.WriteEndArray();
            
            writer.WritePropertyName("DiskPath");
            writer.WriteValue(DiskPath);
            
            writer.WritePropertyName("Props");
            writer.WriteStartArray();
            {
                serializer.Serialize(writer, MeshPath.Load());
            }
            writer.WriteEndArray();
        }


        public void SaveToDisk(DirectoryInfo info)
        {
            string path;
            var sk = MeshPath.Load<USkeletalMesh>();
            var a = new MeshExporter(sk).MeshLods[0];
            if (a.TryWriteToDir(info, out path))
            {
                var filePath = FixAndCreatePath(info, a.FileName);
                DiskPath = filePath;
            }
        }
    }

    public class Material: Base
    {
        public FSoftObjectPath Mat;
        public int? MaterialIndex;

        protected internal override void WriteJson(JsonWriter writer, JsonSerializer serializer)
        {
            base.WriteJson(writer, serializer);

            writer.WritePropertyName("Mat");
            serializer.Serialize(writer, Mat);

            writer.WritePropertyName("MaterialIndex");
            writer.WriteValue(MaterialIndex);
        }
    }

    public class Character : Base
    {
        public string DisplayName { get; set; }
        public List<Mesh> ActualMeshes { get; set; }

        private string SaveDir;

        public Character(UObject export)
        {
            DisplayName = export.Get<FText>("DisplayName").ToString();
            ActualMeshes = new List<Mesh>();

            var hid = export.Get<FPackageIndex>("HeroDefinition").Load();
            if (hid == null)
                throw new Exception("failed to load hid");

            foreach (var cs in hid.Get<FSoftObjectPath[]>("Specializations"))
            {
                foreach (var cp in cs.Load().Get<FSoftObjectPath[]>("CharacterParts"))
                {
                    ParseCP(cp.Load());
                }
            }
        }

        public void SaveToDisk(string path)
        {
            SaveDir = path;

            var dirinfo = new DirectoryInfo(path);
            foreach (var mesh in ActualMeshes)
            {
                mesh.SaveToDisk(dirinfo);
            }

            var j = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(Path.Join(path, DisplayName + ".json"), j);
        }

        private void ParseCP(UObject export)
        {
            if(export == null)
                return;

            var mesh = new Mesh
            {
                MeshPath = export.Get<FSoftObjectPath>("SkeletalMesh"),
                OverrideMaterial = new List<Material>()
            };

            foreach (var mat in export.GetOrDefault<FStructFallback[]>("MaterialOverrides", Array.Empty<FStructFallback>()))
            {
                mesh.OverrideMaterial.Add(new Material
                {
                    Mat = mat.Get<FSoftObjectPath>("OverrideMaterial"),
                    MaterialIndex = mat.Get<int>("MaterialOverrideIndex")
                });
            }
            ActualMeshes.Add(mesh);
        }

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
    }
}