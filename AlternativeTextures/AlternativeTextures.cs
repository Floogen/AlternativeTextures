using AlternativeTextures.Framework.External.ContentPatcher;
using AlternativeTextures.Framework.Interfaces.API;
using AlternativeTextures.Framework.Managers;
using AlternativeTextures.Framework.Models;
using AlternativeTextures.Framework.Patches;
using AlternativeTextures.Framework.Patches.AnimatedObjects;
using AlternativeTextures.Framework.Patches.Buildings;
using AlternativeTextures.Framework.Patches.Entities;
using AlternativeTextures.Framework.Patches.StandardObjects;
using AlternativeTextures.Framework.Patches.Tools;
using AlternativeTextures.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlternativeTextures
{
    public class AlternativeTextures : Mod
    {
        // Core modData keys
        internal const string TEXTURE_TOKEN_HEADER = "AlternativeTextures/Textures/";
        internal const string TOOL_TOKEN_HEADER = "AlternativeTextures/Tools/";
        internal const string DEFAULT_OWNER = "Stardew.Default";

        // Compatibility keys
        internal const string TOOL_CONVERSION_COMPATIBILITY = "AlternativeTextures.HasConvertedMilkPails";
        internal const string TYPE_FIX_COMPATIBILITY = "AlternativeTextures.HasFixedBadObjectTyping";

        // Tool related keys
        internal const string PAINT_BUCKET_FLAG = "AlternativeTextures.PaintBucketFlag";
        internal const string OLD_PAINT_BUCKET_FLAG = "AlternativeTexturesPaintBucketFlag";
        internal const string PAINT_BRUSH_FLAG = "AlternativeTextures.PaintBrushFlag";
        internal const string PAINT_BRUSH_SCALE = "AlternativeTextures.PaintBrushScale";
        internal const string SCISSORS_FLAG = "AlternativeTextures.ScissorsFlag";

        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static Multiplayer multiplayer;

        // Managers
        internal static TextureManager textureManager;
        internal static ApiManager apiManager;
        internal static AssetManager assetManager;

        // Utilities
        internal static FpsCounter fpsCounter;

        // Debugging flags
        private bool _displayFPS = false;

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

            // Setup our utilities
            fpsCounter = new FpsCounter();

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
                new FurniturePatch(monitor).Apply(harmony);
                new BedFurniturePatch(monitor).Apply(harmony);
                new FishTankFurniturePatch(monitor).Apply(harmony);

                // Start of animated objects
                new ChestPatch(monitor).Apply(harmony);
                new CrabPotPatch(monitor).Apply(harmony);
                new IndoorPotPatch(monitor).Apply(harmony);
                new PhonePatch(monitor).Apply(harmony);
                /*
                 * Not supported:
                 * - Wood Chipper
                 */

                // Start of entity patches
                new CharacterPatch(monitor).Apply(harmony);
                new ChildPatch(monitor).Apply(harmony);
                new FarmAnimalPatch(monitor).Apply(harmony);
                new HorsePatch(monitor).Apply(harmony);
                new PetPatch(monitor).Apply(harmony);

                // Start of building patches
                new BuildingPatch(monitor).Apply(harmony);

                // Paint tool related patches
                new UtilityPatch(monitor).Apply(harmony);
                new ToolPatch(monitor).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Add in our debug commands
            helper.ConsoleCommands.Add("at_spawn_gc", "Spawns a giant crop based given harvest product id (e.g. Melon == 254).\n\nUsage: at_spawn_gc [HARVEST_ID]", this.DebugSpawnGiantCrop);
            helper.ConsoleCommands.Add("at_spawn_rc", "Spawns a resource clump based given resource name (e.g. Stump).\n\nUsage: at_spawn_rc [RESOURCE_NAME]", this.DebugSpawnResourceClump);
            helper.ConsoleCommands.Add("at_spawn_child", "Spawns a child. Potentially buggy / gamebreaking, do not use. \n\nUsage: at_spawn_child [AGE] [IS_MALE] [SKIN_TONE]", this.DebugSpawnChild);
            helper.ConsoleCommands.Add("at_set_age", "Sets age for all children in location. Potentially buggy / gamebreaking, do not use. \n\nUsage: at_set_age [AGE]", this.DebugSetAge);
            helper.ConsoleCommands.Add("at_display_fps", "Displays FPS counter. Use again to disable. \n\nUsage: at_display_fps", delegate { _displayFPS = !_displayFPS; });
            helper.ConsoleCommands.Add("at_paint_shop", "Shows the carpenter shop with the paint bucket for sale.\n\nUsage: at_paint_shop", this.DebugShowPaintShop);
            helper.ConsoleCommands.Add("at_reload", "Reloads all Alternative Texture content packs.\n\nUsage: at_reload", delegate { this.LoadContentPacks(); });

            // Hook into GameLoop events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;

            // Hook into Player events
            helper.Events.Player.Warped += this.OnWarped;

            // Hook into Input events
            helper.Events.Input.ButtonsChanged += OnButtonChanged;
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            // Hook into Display events
            helper.Events.Display.Rendered += OnDisplayRendered;
        }

        private void OnDisplayRendered(object sender, RenderedEventArgs e)
        {
            if (!_displayFPS)
            {
                return;
            }

            fpsCounter.OnRendered(sender, e);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu is null && Game1.player.CurrentTool is GenericTool tool && tool.modData.ContainsKey(PAINT_BRUSH_FLAG) && e.Button is SButton.MouseRight)
            {
                Helper.Input.Suppress(e.Button);

                var xTile = (int)e.Cursor.Tile.X * 64;
                var yTile = (int)e.Cursor.Tile.Y * 64;

                // Verify that a supported object exists at the tile
                var placedObject = PatchTemplate.GetObjectAt(Game1.currentLocation, xTile, yTile);
                if (placedObject is null)
                {
                    var terrainFeature = PatchTemplate.GetTerrainFeatureAt(Game1.currentLocation, xTile, yTile);
                    if (terrainFeature is Flooring flooring)
                    {
                        var modelType = AlternativeTextureModel.TextureType.Flooring;
                        if (!flooring.modData.ContainsKey("AlternativeTextureName") || !flooring.modData.ContainsKey("AlternativeTextureVariation"))
                        {
                            // Assign default modData
                            var instanceSeasonName = $"{modelType}_{PatchTemplate.GetFlooringName(flooring)}_{Game1.GetSeasonForLocation(Game1.currentLocation)}";
                            PatchTemplate.AssignDefaultModData(flooring, instanceSeasonName, true);
                        }

                        Game1.addHUDMessage(new HUDMessage($"Texture copied!", 2) { timeLeft = 1000 });
                        tool.modData[PAINT_BRUSH_FLAG] = $"{modelType}_{PatchTemplate.GetFlooringName(flooring)}";
                        tool.modData[PAINT_BRUSH_SCALE] = 0.5f.ToString();
                        tool.modData["AlternativeTextureOwner"] = flooring.modData["AlternativeTextureOwner"];
                        tool.modData["AlternativeTextureName"] = flooring.modData["AlternativeTextureName"];
                        tool.modData["AlternativeTextureVariation"] = flooring.modData["AlternativeTextureVariation"];
                    }
                    else
                    {
                        tool.modData[PAINT_BRUSH_FLAG] = String.Empty;
                        tool.modData[PAINT_BRUSH_SCALE] = 0.5f.ToString();
                        if (terrainFeature != null)
                        {
                            Game1.addHUDMessage(new HUDMessage($"The brush doesn't support copying textures from this object.", 3) { timeLeft = 2000 });
                        }
                        else
                        {
                            Game1.addHUDMessage(new HUDMessage($"Cleared brush!", 2) { timeLeft = 1000 });
                        }
                    }
                }
                else
                {
                    var modelType = placedObject is Furniture ? AlternativeTextureModel.TextureType.Furniture : AlternativeTextureModel.TextureType.Craftable;
                    if (!placedObject.modData.ContainsKey("AlternativeTextureName") || !placedObject.modData.ContainsKey("AlternativeTextureVariation"))
                    {
                        var instanceSeasonName = $"{modelType}_{PatchTemplate.GetObjectName(placedObject)}_{Game1.currentSeason}";
                        PatchTemplate.AssignDefaultModData(placedObject, instanceSeasonName, true);
                    }

                    Game1.addHUDMessage(new HUDMessage($"Texture copied!", 2) { timeLeft = 1000 });
                    tool.modData[PAINT_BRUSH_FLAG] = $"{modelType}_{PatchTemplate.GetObjectName(placedObject)}";
                    tool.modData[PAINT_BRUSH_SCALE] = 0.5f.ToString();
                    tool.modData["AlternativeTextureOwner"] = placedObject.modData["AlternativeTextureOwner"];
                    tool.modData["AlternativeTextureName"] = placedObject.modData["AlternativeTextureName"];
                    tool.modData["AlternativeTextureVariation"] = placedObject.modData["AlternativeTextureVariation"];
                }
            }
        }

        private void OnButtonChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (Game1.activeClickableMenu is null && Game1.player.CurrentTool is GenericTool tool && tool.modData.ContainsKey(PAINT_BRUSH_FLAG) && e.Held.Contains(SButton.MouseLeft))
            {
                var xTile = (int)e.Cursor.Tile.X * 64;
                var yTile = (int)e.Cursor.Tile.Y * 64;

                if (String.IsNullOrEmpty(tool.modData[PAINT_BRUSH_FLAG]))
                {
                    Game1.addHUDMessage(new HUDMessage($"The brush doesn't have a copied texture! Right click on an object to copy a texture.", 3) { timeLeft = 2000 });
                }
                else
                {
                    // Verify that a supported object exists at the tile
                    var placedObject = PatchTemplate.GetObjectAt(Game1.currentLocation, xTile, yTile);
                    if (placedObject is null)
                    {
                        var terrainFeature = PatchTemplate.GetTerrainFeatureAt(Game1.currentLocation, xTile, yTile);
                        if (terrainFeature is Flooring flooring)
                        {
                            var modelType = AlternativeTextureModel.TextureType.Flooring;
                            if (tool.modData[PAINT_BRUSH_FLAG] == $"{modelType}_{PatchTemplate.GetFlooringName(flooring)}")
                            {
                                flooring.modData["AlternativeTextureOwner"] = tool.modData["AlternativeTextureOwner"];
                                flooring.modData["AlternativeTextureName"] = tool.modData["AlternativeTextureName"];
                                flooring.modData["AlternativeTextureVariation"] = tool.modData["AlternativeTextureVariation"];
                            }
                            else
                            {
                                Game1.addHUDMessage(new HUDMessage($"The copied texture {tool.modData[PAINT_BRUSH_FLAG]} isn't valid for this object!", 3) { timeLeft = 2000 });
                            }
                        }
                        else if (terrainFeature != null)
                        {
                            Game1.addHUDMessage(new HUDMessage($"You can't paint that!", 3) { timeLeft = 2000 });
                        }
                    }
                    else
                    {
                        var modelType = placedObject is Furniture ? AlternativeTextureModel.TextureType.Furniture : AlternativeTextureModel.TextureType.Craftable;
                        if (tool.modData[PAINT_BRUSH_FLAG] == $"{modelType}_{PatchTemplate.GetObjectName(placedObject)}")
                        {
                            placedObject.modData["AlternativeTextureOwner"] = tool.modData["AlternativeTextureOwner"];
                            placedObject.modData["AlternativeTextureName"] = tool.modData["AlternativeTextureName"];
                            placedObject.modData["AlternativeTextureVariation"] = tool.modData["AlternativeTextureVariation"];
                        }
                        else
                        {
                            Game1.addHUDMessage(new HUDMessage($"The copied texture {tool.modData[PAINT_BRUSH_FLAG]} isn't valid for this object!", 3) { timeLeft = 2000 });
                        }
                    }
                }
            }
        }

        public override object GetApi()
        {
            return new Api(this);
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
                apiManager.GetContentPatcherInterface().RegisterToken(ModManifest, "Tools", new ToolToken(textureManager, assetManager));
            }

            // Load any owned content packs
            this.LoadContentPacks();
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Load all available textures to account for any Content Patcher's OnDayStart updates
            UpdateTextures();

            // Backwards compatibility logic
            if (!Game1.player.modData.ContainsKey(TOOL_CONVERSION_COMPATIBILITY))
            {
                Monitor.Log($"Converting old Paint Buckets into generic tools...", LogLevel.Debug);
                Game1.player.modData[TOOL_CONVERSION_COMPATIBILITY] = true.ToString();
                ConvertPaintBucketsToGenericTools(Game1.player);
            }
            if (!Game1.player.modData.ContainsKey(TYPE_FIX_COMPATIBILITY))
            {
                Monitor.Log($"Fixing bad object and bigcraftable typings...", LogLevel.Debug);
                Game1.player.modData[TYPE_FIX_COMPATIBILITY] = true.ToString();
                FixBadObjectTyping();
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            // Load all available textures to account for any Content Patcher's OnWarped updates
            UpdateTextures();
        }

        private void UpdateTextures()
        {
            foreach (var texture in textureManager.GetAllTextures().Where(t => t.EnableContentPatcherCheck))
            {
                var loadedTexture = Helper.Content.Load<Texture2D>($"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{texture.GetId()}", ContentSource.GameContent);
                textureManager.UpdateTexture(texture.GetId(), loadedTexture);
            }

            foreach (var tool in assetManager.toolNames.ToList())
            {
                var loadedTexture = Helper.Content.Load<Texture2D>($"{AlternativeTextures.TOOL_TOKEN_HEADER}{tool.Key}", ContentSource.GameContent);
                assetManager.toolNames[tool.Key] = loadedTexture;
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
                            textureModel.Type = textureModel.GetTextureType();

                            // Override Grass Alternative Texture pack ItemNames to always be Grass, in order to be compatible with translations 
                            textureModel.ItemName = textureModel.Type == "Grass" ? "Grass" : textureModel.ItemName;

                            // Add the UniqueId to the top-level Keywords
                            textureModel.Keywords.Add(contentPack.Manifest.UniqueID);

                            // Add the top-level Keywords to any ManualVariations.Keywords
                            foreach (var variation in textureModel.ManualVariations)
                            {
                                variation.Keywords.AddRange(textureModel.Keywords);
                            }

                            // Set the season (if any)
                            textureModel.Season = seasons.Count() == 0 ? String.Empty : seasons[s];

                            // Set the ModelName and TextureId
                            textureModel.ModelName = String.IsNullOrEmpty(textureModel.Season) ? String.Concat(textureModel.GetTextureType(), "_", textureModel.ItemName) : String.Concat(textureModel.GetTextureType(), "_", textureModel.ItemName, "_", textureModel.Season);
                            textureModel.TextureId = String.Concat(textureModel.Owner, ".", textureModel.ModelName);

                            // Verify we are given a texture and if so, track it
                            if (!File.Exists(Path.Combine(textureFolder.FullName, "texture.png")))
                            {
                                // No texture.png found, may be using split texture files (texture_1.png, texture_2.png, etc.)
                                var textureFilePaths = Directory.GetFiles(textureFolder.FullName, "texture_*.png")
                                    .Select(t => Path.GetFileName(t))
                                    .Where(t => t.Any(char.IsDigit))
                                    .OrderBy(t => Int32.Parse(Regex.Match(t, @"\d+").Value));

                                if (textureFilePaths.Count() == 0)
                                {
                                    Monitor.Log($"Unable to add alternative texture for item {textureModel.ItemName} from {contentPack.Manifest.Name}: No associated texture.png or split textures (texture_1.png, texture_2.png, etc.) given", LogLevel.Warn);
                                    continue;
                                }

                                // Load in the first texture_#.png to get its dimensions for creating stitchedTexture
                                Texture2D baseTexture = contentPack.LoadAsset<Texture2D>(Path.Combine(parentFolderName, textureFolder.Name, textureFilePaths.First()));
                                Texture2D stitchedTexture = new Texture2D(Game1.graphics.GraphicsDevice, baseTexture.Width, baseTexture.Height * textureFilePaths.Count());

                                // Now stitch together the split textures into a single texture
                                Color[] pixels = new Color[stitchedTexture.Width * stitchedTexture.Height];
                                for (int x = 0; x < textureFilePaths.Count(); x++)
                                {
                                    var fileName = textureFilePaths.ElementAt(x);
                                    Monitor.Log($"Stitching together {textureModel.TextureId}: {fileName}", LogLevel.Trace);

                                    var offset = x * baseTexture.Width * baseTexture.Height;
                                    var subTexture = contentPack.LoadAsset<Texture2D>(Path.Combine(parentFolderName, textureFolder.Name, fileName));

                                    Color[] subPixels = new Color[subTexture.Width * subTexture.Height];
                                    subTexture.GetData(subPixels);
                                    for (int i = 0; i < subPixels.Length; i++)
                                    {
                                        pixels[i + offset] = subPixels[i];
                                    }
                                }

                                stitchedTexture.SetData(pixels);
                                textureModel.TileSheetPath = contentPack.GetActualAssetKey(Path.Combine(parentFolderName, textureFolder.Name, textureFilePaths.First()));
                                textureModel.Texture = stitchedTexture;
                            }
                            else
                            {
                                // Load in the single vertical texture
                                textureModel.TileSheetPath = contentPack.GetActualAssetKey(Path.Combine(parentFolderName, textureFolder.Name, "texture.png"));
                                textureModel.Texture = contentPack.LoadAsset<Texture2D>(textureModel.TileSheetPath);
                            }

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

        private void DebugSpawnChild(string command, string[] args)
        {
            if (args.Length < 2)
            {
                Monitor.Log($"Missing required arguments: [AGE] [IS_MALE] [SKIN_TONE]", LogLevel.Warn);
                return;
            }

            var age = -1;
            if (!int.TryParse(args[0], out age) || age < 0)
            {
                Monitor.Log($"Invalid number given: {args[0]}", LogLevel.Warn);
                return;
            }

            var isMale = false;
            if (args[1].ToLower() == "true")
            {
                isMale = true;
            }

            var hasDarkSkin = false;
            if (args[2].ToLower() == "dark")
            {
                hasDarkSkin = true;
            }

            var child = new Child("Test", isMale, hasDarkSkin, Game1.player);
            child.Position = Game1.player.Position;
            child.Age = age;
            Game1.currentLocation.characters.Add(child);
        }

        private void DebugSetAge(string command, string[] args)
        {
            if (args.Length == 0)
            {
                Monitor.Log($"Missing required arguments: [AGE]", LogLevel.Warn);
                return;
            }

            var age = -1;
            if (!int.TryParse(args[0], out age))
            {
                Monitor.Log($"Invalid number given: {args[0]}", LogLevel.Warn);
                return;
            }

            foreach (var child in Game1.currentLocation.characters.Where(c => c is Child))
            {
                child.Age = 3;
            }
        }

        private void DebugShowPaintShop(string command, string[] args)
        {
            Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
        }

        private void ConvertPaintBucketsToGenericTools(Farmer who)
        {
            // Check player's inventory first
            for (int i = 0; i < (int)who.maxItems; i++)
            {
                if (who.items[i] is MilkPail milkPail && milkPail.modData.ContainsKey(OLD_PAINT_BUCKET_FLAG))
                {
                    who.items[i] = PatchTemplate.GetPaintBucketTool();
                }
            }

            foreach (var location in Game1.locations)
            {
                ConvertStoredPaintBucketsToGenericTools(who, location);

                if (location is BuildableGameLocation)
                {
                    foreach (var building in (location as BuildableGameLocation).buildings)
                    {
                        GameLocation indoorLocation = building.indoors.Value;
                        if (indoorLocation is null)
                        {
                            continue;
                        }

                        ConvertStoredPaintBucketsToGenericTools(who, indoorLocation);
                    }
                }
            }
        }

        private void ConvertStoredPaintBucketsToGenericTools(Farmer who, GameLocation location)
        {
            foreach (var chest in location.Objects.Pairs.Where(p => p.Value is Chest).Select(p => p.Value as Chest).ToList())
            {
                if (chest.isEmpty())
                {
                    continue;
                }

                if (chest.SpecialChestType == Chest.SpecialChestTypes.JunimoChest)
                {
                    NetObjectList<Item> actual_items = chest.GetItemsForPlayer(who.UniqueMultiplayerID);
                    for (int j = actual_items.Count - 1; j >= 0; j--)
                    {
                        if (actual_items[j] is MilkPail milkPail && milkPail.modData.ContainsKey(OLD_PAINT_BUCKET_FLAG))
                        {
                            actual_items[j] = PatchTemplate.GetPaintBucketTool();
                        }
                    }
                }
                else
                {
                    for (int i = chest.items.Count - 1; i >= 0; i--)
                    {
                        if (chest.items[i] is MilkPail milkPail && milkPail.modData.ContainsKey(OLD_PAINT_BUCKET_FLAG))
                        {
                            chest.items[i] = PatchTemplate.GetPaintBucketTool();
                        }
                    }
                }
            }
        }

        private void FixBadObjectTyping()
        {
            foreach (var location in Game1.locations)
            {
                ConvertBadTypedObjectToNormalType(location);

                if (location is BuildableGameLocation)
                {
                    foreach (var building in (location as BuildableGameLocation).buildings)
                    {
                        GameLocation indoorLocation = building.indoors.Value;
                        if (indoorLocation is null)
                        {
                            continue;
                        }

                        ConvertBadTypedObjectToNormalType(indoorLocation);
                    }
                }
            }
        }

        private void ConvertBadTypedObjectToNormalType(GameLocation location)
        {
            foreach (var obj in location.objects.Values.Where(o => o.modData.ContainsKey("AlternativeTextureName")))
            {
                if (obj.Type == "Craftable" || obj.Type == "Unknown")
                {
                    if (obj.bigCraftable && Game1.bigCraftablesInformation.TryGetValue(obj.parentSheetIndex, out var bigObjectInfo))
                    {
                        string[] objectInfoArray = bigObjectInfo.Split('/');
                        string[] typeAndCategory = objectInfoArray[3].Split(' ');
                        obj.type.Value = typeAndCategory[0];

                        if (typeAndCategory.Length > 1)
                        {
                            obj.Category = Convert.ToInt32(typeAndCategory[1]);
                        }
                    }
                    else if (!obj.bigCraftable && Game1.objectInformation.TryGetValue(obj.parentSheetIndex, out var objectInfo))
                    {
                        string[] objectInfoArray = objectInfo.Split('/');
                        string[] typeAndCategory = objectInfoArray[3].Split(' ');
                        obj.type.Value = typeAndCategory[0];
                        if (typeAndCategory.Length > 1)
                        {
                            obj.Category = Convert.ToInt32(typeAndCategory[1]);
                        }
                    }
                }
            }
        }
    }
}
