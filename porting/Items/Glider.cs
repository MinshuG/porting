using System.Collections.Generic;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.UObject;

namespace porting.Models
{
    public class Glider : ExportBase
    {
        public Glider(UObject export)
        {
            ActualMeshes = new List<Mesh>();
            DisplayName = export.Get<FText>("DisplayName").ToString();
            var mesh = export.GetOrDefault<FSoftObjectPath>("SkeletalMesh"); //.ConvertToString();
            var overrideMatsexport = export.GetOrDefault<FStructFallback[]>("MaterialOverrides");
            var  overrideMats = new List<Material>();
            
            foreach (var mat in overrideMatsexport)
            {
                overrideMats.Add(new Material()
                {
                    Mat = mat.GetOrDefault<FSoftObjectPath>("OverrideMaterial").ToString(),
                    MaterialIndex = mat.GetOrDefault<int>("MaterialOverrideIndex", -1)
                });
            }

            var realmesh = new Mesh()
            {
                SoftMeshPath = mesh,
                OverrideMaterials = overrideMats
            };
            realmesh.LoadInfo();
            ActualMeshes.Add(realmesh);
        }
    }
}