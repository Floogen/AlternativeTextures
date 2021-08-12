using AlternativeTextures;
using AlternativeTextures.Framework.Models;
using AlternativeTextures.Framework.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace AlternativeTextures.Framework.Patches.Tools
{
    internal class MilkPailPatch : PatchTemplate
    {
        private readonly Type _object = typeof(MilkPail);

        internal MilkPailPatch(IMonitor modMonitor) : base(modMonitor)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.PropertyGetter(typeof(Tool), nameof(Tool.DisplayName)), postfix: new HarmonyMethod(GetType(), nameof(LoadDisplayNamePostfix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Tool), nameof(Tool.description)), postfix: new HarmonyMethod(GetType(), nameof(LoadDescriptionPostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(MilkPail.beginUsing), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(BeginUsingPrefix)));
        }

        private static void LoadDisplayNamePostfix(MilkPail __instance, ref string __result)
        {
            if (__instance.modData.ContainsKey("AlternativeTexturesPaintBucketFlag"))
            {
                __result = "Paint Bucket";
            }
        }

        private static void LoadDescriptionPostfix(MilkPail __instance, ref string __result)
        {
            if (__instance.modData.ContainsKey("AlternativeTexturesPaintBucketFlag"))
            {
                __result = "Allows you to apply different textures to supported objects.";
            }
        }

        private static bool BeginUsingPrefix(MilkPail __instance, ref bool __result, GameLocation location, int x, int y, Farmer who)
        {
            if (!__instance.modData.ContainsKey("AlternativeTexturesPaintBucketFlag"))
            {
                return true;
            }
            __result = true;

            var targetedObject = location.getObjectAt(x, y);
            if (targetedObject != null && targetedObject.modData.ContainsKey("AlternativeTextureName"))
            {
                var modelName = targetedObject.modData["AlternativeTextureName"].Replace($"{targetedObject.modData["AlternativeTextureOwner"]}.", String.Empty);
                if (targetedObject.modData.ContainsKey("AlternativeTextureSeason") && !String.IsNullOrEmpty(targetedObject.modData["AlternativeTextureSeason"]))
                {
                    modelName = modelName.Replace($"_{targetedObject.modData["AlternativeTextureSeason"]}", String.Empty);
                }

                if (AlternativeTextures.textureManager.GetAvailableTextureModels(modelName, Game1.GetSeasonForLocation(Game1.currentLocation)).Count == 0)
                {
                    Game1.addHUDMessage(new HUDMessage($"{modelName} has no alternative textures for this season!", 3));
                    who.CanMove = true;
                    who.UsingTool = false;
                    return false;
                }

                // Display texture menu
                Game1.activeClickableMenu = new PaintBucketMenu(targetedObject, modelName);

                who.CanMove = true;
                who.UsingTool = false;
                return false;
            }

            var targetedTerrain = GetTerrainFeatureAt(location, x, y);
            if (targetedTerrain != null && targetedTerrain.modData.ContainsKey("AlternativeTextureName") && targetedTerrain.modData.ContainsKey("AlternativeTextureSheetId"))
            {
                var modelName = targetedTerrain.modData["AlternativeTextureName"].Replace($"{targetedTerrain.modData["AlternativeTextureOwner"]}.", String.Empty);
                if (targetedTerrain.modData.ContainsKey("AlternativeTextureSeason") && !String.IsNullOrEmpty(targetedTerrain.modData["AlternativeTextureSeason"]))
                {
                    modelName = modelName.Replace($"_{targetedTerrain.modData["AlternativeTextureSeason"]}", String.Empty);
                }

                if (AlternativeTextures.textureManager.GetAvailableTextureModels(modelName, Game1.GetSeasonForLocation(Game1.currentLocation)).Count == 0)
                {
                    Game1.addHUDMessage(new HUDMessage($"{modelName} has no alternative textures for this season!", 3));
                    who.CanMove = true;
                    who.UsingTool = false;
                    return false;
                }

                // Display texture menu
                var terrainObj = new Object(targetedTerrain.currentTileLocation, Int32.Parse(targetedTerrain.modData["AlternativeTextureSheetId"]), 1);
                if (terrainObj != null)
                {
                    foreach (string key in targetedTerrain.modData.Keys)
                    {
                        terrainObj.modData[key] = targetedTerrain.modData[key];
                    }

                    Game1.activeClickableMenu = new PaintBucketMenu(terrainObj, modelName, true);
                }

                who.CanMove = true;
                who.UsingTool = false;
                return false;
            }

            who.CanMove = true;
            who.UsingTool = false;
            return false;
        }
    }
}
