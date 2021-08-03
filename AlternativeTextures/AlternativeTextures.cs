using AlternativeTextures.Framework.External.ContentPatcher;
using AlternativeTextures.Framework.Managers;
using AlternativeTextures.Framework.Models;
using AlternativeTextures.Framework.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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

        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        // Managers
        internal static TextureManager textureManager;
        internal static ApiManager apiManager;
        internal static AssetManager assetManager;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor and helper
            monitor = Monitor;
            modHelper = Helper;

            // Setup our managers
            textureManager = new TextureManager(monitor);
            apiManager = new ApiManager(monitor);
            assetManager = new AssetManager();

            // Load the asset manager
            Helper.Content.AssetLoaders.Add(assetManager);

            // Load our Harmony patches
            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply our patches
                new ObjectPatch(monitor).Apply(harmony);
                new FencePatch(monitor).Apply(harmony);
                new HoeDirtPatch(monitor).Apply(harmony);
                new CropPatch(monitor).Apply(harmony);
                new GrassPatch(monitor).Apply(harmony);
                new TreePatch(monitor).Apply(harmony);
                new FruitTreePatch(monitor).Apply(harmony);

                /* 
                 * TODO: Implement support for the following
                 * - Giant Crop
                 * - Special Objects w/ aniamtion (Chest, etc.)
                 */
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

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

        private void LoadContentPacks(bool isReload = false)
        {
            // Load owned content packs
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Loading companions from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Debug);

                var textureFolders = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Textures")).GetDirectories();
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
                        Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a texture.json under {textureFolder.Name}!", LogLevel.Warn);
                        continue;
                    }

                    AlternativeTextureModel textureModel = contentPack.ReadJsonFile<AlternativeTextureModel>(Path.Combine(textureFolder.Parent.Name, textureFolder.Name, "texture.json"));
                    textureModel.Owner = contentPack.Manifest.UniqueID;
                    Monitor.Log(textureModel.ToString(), LogLevel.Trace);

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, "texture.png")))
                    {
                        Monitor.Log($"Unable to add alternative texture for item {textureModel.ItemName} from {contentPack.Manifest.Name}: No associated texture.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    textureModel.TileSheetPath = contentPack.GetActualAssetKey(Path.Combine(textureFolder.Parent.Name, textureFolder.Name, "texture.png"));
                    textureModel.Texture = contentPack.LoadAsset<Texture2D>(textureModel.TileSheetPath);

                    // Track the texture model
                    textureManager.AddAlternativeTexture(textureModel);
                }
            }
        }
    }
}
