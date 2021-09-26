using System.Collections.Generic;
using System.IO;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace porting.Models
{
    public class Backpack : ExportBase
    {
        private string SocketName;
        public Backpack(UObject export)
        {
            DisplayName = export.Get<FText>("DisplayName").ToString();
            
            foreach (var cp in export.Get<UObject[]>("CharacterParts"))
            {
                SocketName = cp.GetOrDefault<UObject>("AdditionalData")
                    .GetOrDefault<FName>("AttachSocketName").ToString();
                ActualMeshes = Character.ParseCP(cp);
            }
        }

        protected internal override void WriteJson(JsonWriter writer, JsonSerializer serializer)
        {
            base.WriteJson(writer, serializer);
            
            writer.WritePropertyName("SocketName");
            writer.WriteValue(SocketName);
        }
    }
}