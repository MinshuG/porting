using System;
using System.IO;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse_Conversion;
using Serilog;
using porting.Models;

namespace porting
{
    class Program
    {
        static void Main(string[] args)
        { 
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            
            System.Console.WriteLine(args[0]);
            string _gameDirectory = @"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks";
            string _aesKey = "0x26CD203A3B9D9163BE126BFD09910594FE7A322CE0103E6B7DD8EEAD494AC023";

            var provider = new DefaultFileProvider(_gameDirectory, SearchOption.TopDirectoryOnly, true);
            provider.Initialize();
            
            provider.SubmitKey(new FGuid(), new FAesKey(_aesKey));
            
            provider.MappingsContainer = new FileUsmapTypeMappingsProvider(@"C:\Users\Minshu\Desktop\BlenderUmap\run\mappings\++Fortnite+Release-18.00-CL-17468642-Windows_oo.usmap");

            var allExports = provider.LoadObjectExports(args[0]);

            ExportBase exporter;
            foreach (var export in allExports){
                switch (export.ExportType)
                {
                    case "AthenaCharacterItemDefinition":
                        exporter = new Character(export);
                        break;
                    case "AthenaBackpackItemDefinition":
                        exporter = new Backpack(export);
                        break;
                    default:
                        exporter = new ExportBase();
                        break;
                };

                exporter.SaveToDisk(@"C:\Users\Minshu\Desktop\BlenderUmap\porting");
            }
        }
    }
}