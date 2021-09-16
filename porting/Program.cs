﻿using System;
using System.IO;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.Utils;
using porting.Models;

namespace porting
{
    class Program
    {
        static void Main(string[] args)
        { 
            System.Console.WriteLine(args[0]);
            string _gameDirectory = @"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks";
            string _aesKey = "0x26CD203A3B9D9163BE126BFD09910594FE7A322CE0103E6B7DD8EEAD494AC023";

            var provider = new DefaultFileProvider(_gameDirectory, SearchOption.TopDirectoryOnly, true);
            provider.Initialize();
            
            provider.SubmitKey(new FGuid(), new FAesKey(_aesKey)); // decrypt basic info (1 guid - 1 key)
            
            provider.LoadMappings();

            var allExports = provider.LoadObjectExports(args[0]);

            foreach (var export in allExports)
            {
                if (export.ExportType == "AthenaCharacterItemDefinition")
                {
                    var something = new Character(export);
                }
            }
        }
    }
}