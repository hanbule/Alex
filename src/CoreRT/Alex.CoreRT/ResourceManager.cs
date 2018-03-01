﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using Alex.CoreRT.Utils;
using log4net;
using Microsoft.Xna.Framework.Graphics;
using ResourcePackLib;
using ResourcePackLib.CoreRT;

namespace Alex.CoreRT
{
    public class ResourceManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResourceManager));

		public McResourcePack ResourcePack { get; private set; }
		public AtlasGenerator Atlas { get; private set; }

		private GraphicsDevice Graphics { get; set; }

	    public ResourceManager(GraphicsDevice graphics)
	    {
		    Graphics = graphics;
		    Atlas = new AtlasGenerator(Graphics);
		}

	    private const string AssetVersion = "1.12"; //"18w07c";
		private static readonly string ResourcePackDirectory = Path.Combine("assets", "resourcepacks");
	    private static readonly string DefaultResourcePackPath = Path.Combine(ResourcePackDirectory, $"{AssetVersion}.zip");
		private byte[] DownloadDefaultResources()
	    {
		    var sw = new Stopwatch();

		    Log.Info("Downloading vanilla Minecraft resources...");
		    byte[] resourceData;

		    sw.Start();
		    using (var client = new WebClient())
		    {
			    resourceData =
				    client.DownloadData(string.Format("https://s3.amazonaws.com/Minecraft.Download/versions/{0}/{0}.jar",
					    AssetVersion));
		    }
		    sw.Stop();

		    Log.Info("Download took: " + Math.Round(sw.ElapsedMilliseconds / 1000D, 2) +
		             " seconds to finish");

		    Log.Info("Saving default resources...");
		    File.WriteAllBytes(DefaultResourcePackPath, resourceData);
		    return resourceData; 
		}

	    private McResourcePack LoadResourcePack(GraphicsDevice graphics, Stream stream, bool replaceModels = false, bool replaceTextures = false, bool reportMissingModels = false)
	    {
		    McResourcePack resourcePack = null;

			using (var archive = new ZipArchive(stream))
		    {
			    resourcePack = new McResourcePack(archive);
			    if (!replaceTextures)
			    {
				    Atlas.LoadResourcePackOnTop(graphics, archive);
			    }
			    else
			    {
				    Atlas.GenerateAtlas(graphics, archive);
			    }

				int imported = BlockFactory.LoadResources(this, resourcePack, replaceModels, reportMissingModels);
				Log.Info($"Imported {imported} blockstates from resourcepack!");
		    }

		    return resourcePack;
	    }

        public void CheckResources(GraphicsDevice device, Settings setings)
        {
	        if (!Directory.Exists("assets"))
				Directory.CreateDirectory("assets");

			if (!Directory.Exists(ResourcePackDirectory))
			{
				Directory.CreateDirectory(ResourcePackDirectory);
			}

	        byte[] defaultResources;
	        if (!File.Exists(DefaultResourcePackPath))
	        {
		        defaultResources = DownloadDefaultResources();
	        }
	        else
	        {
		        defaultResources = File.ReadAllBytes(DefaultResourcePackPath);
	        }

	        Log.Info($"Loading vanilla resourcepack...");
	        using (MemoryStream stream = new MemoryStream(defaultResources))
	        {
		        ResourcePack = LoadResourcePack(device, stream, true, true, true);
	        }

	        foreach (string file in setings.ResourcePacks)
	        {
		        try
		        {
			        string resourcePackPath = Path.Combine(ResourcePackDirectory, file);
			        if (File.Exists(resourcePackPath))
			        {
				        Log.Info($"Loading resourcepack {file}...");

				        using (FileStream stream = new FileStream(resourcePackPath, FileMode.Open))
				        {
					        LoadResourcePack(device, stream, true, false);
				        }
			        }
		        }
		        catch (Exception e)
		        {
					Log.Warn($"Could not load resourcepack {file}!", e);
		        }
	        }
        }
    }
}
