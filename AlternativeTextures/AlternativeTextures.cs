using AlternativeTextures.Framework.External.ContentPatcher;
using AlternativeTextures.Framework.Managers;
using AlternativeTextures.Framework.Models;
using AlternativeTextures.Framework.Patches;
using AlternativeTextures.Framework.Patches.AnimatedObjects;
using AlternativeTextures.Framework.Patches.StandardObjects;
using AlternativeTextures.Framework.Patches.Tools;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeTextures
{
    public class AlternativeTextures : Mod
    {
        internal const string TOKEN_HEADER = "AlternativeTextures/Textures/";
        internal const string DEFAULT_OWNER = "Stardew.Default";

        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static Multiplayer multiplayer;

        // Managers
        internal static TextureManager textureManager;
        internal static ApiManager apiManager;
        internal static AssetManager assetManager;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Setup our managers
            textureManager = new TextureManager(monitor);
            apiManager = new ApiManager(monitor);
            assetManager = new AssetManager(helper);

            // Load the asset manager
            helper.Content.AssetLoaders.Add(assetManager);

            // Load our Harmony patches
            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply texture override related patches
                new GameLocationPatch(monitor).Apply(harmony);
                new ObjectPatch(monitor).Apply(harmony);
                new FencePatch(monitor).Apply(harmony);
                new HoeDirtPatch(monitor).Apply(harmony);
                new CropPatch(monitor).Apply(harmony);
                new GiantCropPatch(monitor).Apply(harmony);
                new GrassPatch(monitor).Apply(harmony);
                new TreePatch(monitor).Apply(harmony);
                new FruitTreePatch(monitor).Apply(harmony);
                new ResourceClumpPatch(monitor).Apply(harmony);
                new BushPatch(monitor).Apply(harmony);
                new FlooringPatch(monitor).Apply(harmony);

                // Start of animated objects
                new ChestPatch(monitor).Apply(harmony);
                new CrabPotPatch(monitor).Apply(harmony);
                new IndoorPotPatch(monitor).Apply(harmony);
                new PhonePatch(monitor).Apply(harmony);
                /*
                 * Not supported:
                 * - Wood Chipper
                 */

                // Paint tool related patches
                new UtilityPatch(monitor).Apply(harmony);
                new ToolPatch(monitor).Apply(harmony);
                new MilkPailPatch(monitor).Apply(harmony);

                /* 
                 * TODO: Implement the following
                 * - Add default modData to all valid placed objects / terrain features
                 */
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Add in our debug commands
            helper.ConsoleCommands.Add("at_spawn_gc", "Spawns a giant crop based given harvest product id (e.g. Melon == 254).\n\nUsage: at_spawn_gc [HARVEST_ID]", this.DebugSpawnGiantCrop);
            helper.ConsoleCommands.Add("at_spawn_rc", "Spawns a resource clump based given resource name (e.g. Stump).\n\nUsage: at_spawn_rc [RESOURCE_NAME]", this.DebugSpawnResourceClump);
            helper.ConsoleCommands.Add("at_paint_shop", "Shows the carpenter shop with the paint bucket for sale.\n\nUsage: at_paint_shop", this.DebugShowPaintShop);
            helper.ConsoleCommands.Add("at_reload", "Reloads all Alternative Texture content packs.\n\nUsage: at_reload", delegate { this.LoadContentPacks(); });

            // Hook into GameLoop events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;

            // Hook into Player events
            helper.Events.Player.Warped += this.OnWarped;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Hook into the APIs we utilize
            if (Helper.ModRegistry.IsLoaded("spacechase0.JsonAssets"))
            {
                apiManager.HookIntoJsonAssets(Helper);
            }

            if (Helper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher") && apiManager.HookIntoContentPatcher(Helper))
            {
                apiManager.GetContentPatcherInterface().RegisterToken(ModManifest, "Textures", new TextureToken(textureManager, assetManager));
            }

            // Load any owned content packs
            this.LoadContentPacks();
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (var texture in textureManager.GetAllTextures())
            {
                var test = Helper.Content.Load<Texture2D>($"{AlternativeTextures.TOKEN_HEADER}{texture.GetId()}", ContentSource.GameContent);
                textureManager.UpdateTexture(texture.GetId(), test);
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            foreach (var texture in textureManager.GetAllTextures())
            {
                var test = Helper.Content.Load<Texture2D>($"{AlternativeTextures.TOKEN_HEADER}{texture.GetId()}", ContentSource.GameContent);
                textureManager.UpdateTexture(texture.GetId(), test);
            }
        }

        private void LoadContentPacks()
        {
            // Load owned content packs
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Loading textures from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Debug);

                try
                {
                    var textureFolders = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Textures")).GetDirectories("*", SearchOption.AllDirectories);
                    if (textureFolders.Count() == 0)
                    {
                        Monitor.Log($"No sub-folders found under Textures for the content pack {contentPack.Manifest.Name}!", LogLevel.Warn);
                        continue;
                    }

                    // Load in the alternative textures
                    foreach (var textureFolder in textureFolders)
                    {
                        if (!File.Exists(Path.Combine(textureFolder.FullName, "texture.json")))
                        {
                            if (textureFolder.GetDirectories().Count() == 0)
                            {
                                Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a texture.json under {textureFolder.Name}!", LogLevel.Warn);
                            }

                            continue;
                        }


                        var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + "\\", String.Empty);
                        var modelPath = Path.Combine(parentFolderName, textureFolder.Name, "texture.json");
                        var seasons = contentPack.ReadJsonFile<AlternativeTextureModel>(modelPath).Seasons;
                        for (int s = 0; s < 4; s++)
                        {
                            if ((seasons.Count() == 0 && s > 0) || (seasons.Count() > 0 && s >= seasons.Count()))
                            {
                                continue;
                            }

                            // Parse the model and assign it the content pack's owner
                            AlternativeTextureModel textureModel = contentPack.ReadJsonFile<AlternativeTextureModel>(modelPath);
                            textureModel.Owner = contentPack.Manifest.UniqueID;

                            // Add the UniqueId to the top-level Keywords
                            textureModel.Keywords.Add(contentPack.Manifest.UniqueID);

                            // Add the top-level Keywords to any ManualVariations.Keywords
                            foreach (var variation in textureModel.ManualVariations)
                            {
                                variation.Keywords.AddRange(textureModel.Keywords);
                            }

                            // Verify we are given a texture and if so, track it
                            if (!File.Exists(Path.Combine(textureFolder.FullName, "texture.png")))
                            {
                                Monitor.Log($"Unable to add alternative texture for item {textureModel.ItemName} from {contentPack.Manifest.Name}: No associated texture.png given", LogLevel.Warn);
                                continue;
                            }

                            // Load in the texture
                            textureModel.TileSheetPath = contentPack.GetActualAssetKey(Path.Combine(parentFolderName, textureFolder.Name, "texture.png"));
                            textureModel.Texture = contentPack.LoadAsset<Texture2D>(textureModel.TileSheetPath);

                            // Set the season (if any)
                            textureModel.Season = seasons.Count() == 0 ? String.Empty : seasons[s];

                            // Track the texture model
                            textureManager.AddAlternativeTexture(textureModel);

                            // Log it
                            Monitor.Log(textureModel.ToString(), LogLevel.Trace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Error loading content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
                }
            }
        }

        private void DebugSpawnGiantCrop(string command, string[] args)
        {
            if (args.Length == 0)
            {
                Monitor.Log($"Missing required arguments: [HARVEST_ID]", LogLevel.Warn);
                return;
            }

            if (!(Game1.currentLocation is Farm))
            {
                Monitor.Log($"Command can only be used on player's farm.", LogLevel.Warn);
                return;
            }

            var environment = Game1.currentLocation;
            foreach (var tile in environment.terrainFeatures.Pairs.Where(t => t.Value is HoeDirt))
            {
                int xTile = 0;
                int yTile = 0;
                var hoeDirt = tile.Value as HoeDirt;

                if (hoeDirt.crop is null || hoeDirt.crop.indexOfHarvest != int.Parse(args[0]))
                {
                    continue;
                }

                xTile = (int)tile.Key.X;
                yTile = (int)tile.Key.Y;

                if ((int.Parse(args[0]) == 276 || int.Parse(args[0]) == 190 || int.Parse(args[0]) == 254) && xTile != 0 && yTile != 0)
                {
                    for (int x = xTile - 1; x <= xTile + 1; x++)
                    {
                        for (int y2 = yTile - 1; y2 <= yTile + 1; y2++)
                        {
                            Vector2 v3 = new Vector2(x, y2);
                            if (!environment.terrainFeatures.ContainsKey(v3) || !(environment.terrainFeatures[v3] is HoeDirt) || (environment.terrainFeatures[v3] as HoeDirt).crop == null)
                            {
                                continue;
                            }

                            (environment.terrainFeatures[v3] as HoeDirt).crop = null;
                        }
                    }

                    (environment as Farm).resourceClumps.Add(new GiantCrop(int.Parse(args[0]), new Vector2(xTile - 1, yTile - 1)));
                }
            }
        }

        private void DebugSpawnResourceClump(string command, string[] args)
        {
            if (args.Length == 0)
            {
                Monitor.Log($"Missing required arguments: [RESOURCE_NAME]", LogLevel.Warn);
                return;
            }

            if (!(Game1.currentLocation is Farm))
            {
                Monitor.Log($"Command can only be used on player's farm.", LogLevel.Warn);
                return;
            }

            if (args[0].ToLower() != "stump")
            {
                Monitor.Log($"That resource isn't supported.", LogLevel.Warn);
                return;
            }

            (Game1.currentLocation as Farm).resourceClumps.Add(new ResourceClump(600, 2, 2, Game1.player.getTileLocation() + new Vector2(1, 1)));
        }

        private void DebugShowPaintShop(string command, string[] args)
        {
            Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
        }
    }
}
