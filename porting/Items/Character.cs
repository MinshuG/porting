using System;
using System.Collections.Generic;
using System.IO;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse_Conversion.Meshes;
using Newtonsoft.Json;

namespace porting.Models
{
    public class Mesh : Base
    {
        public FSoftObjectPath MeshPath;
        public UObject[] Sockets;
        public List<Material> Materials;
        public List<Material> OverrideMaterials;
        public string DiskPath;

        public void LoadInfo()
        {
            Materials = new();
            var obj = MeshPath.Load<USkeletalMesh>();
            foreach (var material in obj.Materials)
            {
                Materials.Add(new Material() { Mat = material.Material.ToString(), MaterialIndex = -1});
            }
            Sockets = obj.GetOrDefault<FPackageIndex>("Skeleton").Load()?.GetOrDefault<UObject[]>("Sockets");
        }
        protected internal override void WriteJson(JsonWriter writer, JsonSerializer serializer)
        {
            base.WriteJson(writer, serializer);

            writer.WritePropertyName("MeshPath");
            serializer.Serialize(writer, MeshPath);

            writer.WritePropertyName("OverrideMaterials");
            writer.WriteStartArray();
            {
                foreach (var j in OverrideMaterials)
                {
                    serializer.Serialize(writer, j);
                }
            }
            writer.WriteEndArray();

            writer.WritePropertyName("Materials");
            writer.WriteStartArray();
            {
                foreach (var j in Materials)
                {
                    serializer.Serialize(writer, j);
                }
            }
            writer.WriteEndArray();

            if (Sockets != null)
            {
                writer.WritePropertyName("Sockets");
                writer.WriteStartArray();
                {
                    foreach (var j in Sockets)
                    {
                        writer.WriteStartObject();
                        foreach (var property in j.Properties)
                        {
                            writer.WritePropertyName(property.Name.Text);
                            serializer.Serialize(writer, property.Tag);
                        }
                        writer.WriteEndObject();
                    }
                }
                writer.WriteEndArray();
            }            

            writer.WritePropertyName("DiskPath");
            writer.WriteValue(DiskPath);
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
        public string Mat;
        public int? MaterialIndex;

        protected internal override void WriteJson(JsonWriter writer, JsonSerializer serializer)
        {
            base.WriteJson(writer, serializer);

            writer.WritePropertyName("Mat");
            writer.WriteValue(Mat);
            // serializer.Serialize(writer, Mat);

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
                OverrideMaterials = new List<Material>()
            };

            foreach (var mat in export.GetOrDefault<FStructFallback[]>("MaterialOverrides", Array.Empty<FStructFallback>()))
            {
                mesh.OverrideMaterials.Add(new Material
                {
                    Mat = mat.Get<FSoftObjectPath>("OverrideMaterial").ToString(),
                    MaterialIndex = mat.Get<int>("MaterialOverrideIndex")
                });
            }
            mesh.LoadInfo();
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