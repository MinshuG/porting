using System;
using System.Collections.Generic;
using System.IO;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Animation;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;
using CUE4Parse_Conversion;
using CUE4Parse_Conversion.Meshes;
using Newtonsoft.Json;

namespace porting.Models
{
    public class Mesh : Base
    {
        public string MeshPath;
        public FSoftObjectPath? SoftMeshPath;
        public FPackageIndex MeshPathindex;
        public UObject[] Sockets;
        public List<Material> Materials;
        public List<Material> OverrideMaterials;
        public string DiskPath;

        private bool _isSk;
        private USkeletalMesh _loadedObjsk;
        private UStaticMesh _loadedObjsm;

        public void LoadInfo()
        {
            Materials = new();

            UObject loadedObj;
            if (SoftMeshPath != null)
            {
                loadedObj = SoftMeshPath?.Load<UObject>();
                MeshPath = SoftMeshPath.ToString();
            }
            else
            {
                loadedObj = MeshPathindex.Load<UObject>();
                MeshPath = MeshPathindex.ConvertToString();
            }

            if (loadedObj is USkeletalMesh skmesh)
            {
                _isSk = true;
                _loadedObjsk = skmesh;
                foreach (var material in skmesh.Materials)
                {
                    Materials.Add(new Material() { Mat = material.Material.ConvertToString(), MaterialIndex = -1});
                    Sockets = loadedObj.GetOrDefault<FPackageIndex>("Skeleton").Load()?.GetOrDefault<UObject[]>("Sockets");
                }    
            }
            else if (loadedObj is UStaticMesh mesh)
            {
                _loadedObjsm = mesh;
                foreach (var material in mesh.Materials)
                {
                    Materials.Add(new Material() { Mat = material.ConvertToString(), MaterialIndex = -1});
                    Sockets = loadedObj.GetOrDefault<FPackageIndex>("Skeleton").Load()?.GetOrDefault<UObject[]>("Sockets");
                }
            }
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
            MeshExporter a;
            if (_isSk)
                a = new MeshExporter(_loadedObjsk);
            else
                a = new MeshExporter(_loadedObjsm);

            if (a.TryWriteToDir(info, out path))
            {
                var filePath = FixAndCreatePath(info, a.MeshName);
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

    public class Character : ExportBase
    {
        public Character(UObject export)
        {
            DisplayName = export.Get<FText>("DisplayName").ToString();
            ID = export.Name;
            ActualMeshes = new List<Mesh>();

            var hid = export.GetOrDefault<FPackageIndex>("HeroDefinition").Load();
            if (hid == null)
                throw new Exception("failed to load hid");

            foreach (var cs in hid.Get<FSoftObjectPath[]>("Specializations"))
            {
                foreach (var cp in cs.Load().Get<FSoftObjectPath[]>("CharacterParts"))
                {
                    ActualMeshes.AddRange(ParseCP(cp.Load()));
                }
            }
        }

        public static List<Mesh> ParseCP(UObject export)
        {
            var meshes = new List<Mesh>();
            if(export == null)
                return meshes;

            var mesh = new Mesh
            {
                SoftMeshPath = export.Get<FSoftObjectPath>("SkeletalMesh"),
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
            meshes.Add(mesh);
            return meshes;
        }
        
    }
}