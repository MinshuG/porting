using System;
using System.Collections.Generic;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.UObject;

namespace porting.Models
{
    public class Mesh
    {
        public FSoftObjectPath MeshPath;
        public List<Material> OverrideMaterial;
    }

    public class Material
    {
        public FSoftObjectPath Mat;
        public int? MaterialIndex;
    }

    public class Character
    {
        public string DisplayName { get; set; }
        public List<Mesh> ActualMeshes { get; set; }

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
            // System.Diagnostics.Debugger.Break();
        }
 
        public bool SaveToDisk(string path)
        {
            // todo
            return false;
        }

        public void ParseCP(UObject export)
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
    }
}