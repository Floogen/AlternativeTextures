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
            if (targetedObject != null)
            {
                // Assign default data if none exists
                if (!targetedObject.modData.ContainsKey("AlternativeTextureName"))
                {
                    var instanceSeasonName = $"{AlternativeTextureModel.TextureType.Craftable}_{targetedObject.name}_{Game1.currentSeason}";
                    AssignDefaultModData(targetedObject, instanceSeasonName, true);
                }

                var modelName = targetedObject.modData["AlternativeTextureName"].Replace($"{targetedObject.modData["AlternativeTextureOwner"]}.", String.Empty);
                if (targetedObject.modData.ContainsKey("AlternativeTextureSeason") && !String.IsNullOrEmpty(targetedObject.modData["AlternativeTextureSeason"]))
                {
                    modelName = modelName.Replace($"_{targetedObject.modData["AlternativeTextureSeason"]}", String.Empty);
                }

                if (AlternativeTextures.textureManager.GetAvailableTextureModels(modelName, Game1.GetSeasonForLocation(Game1.currentLocation)).Count == 0)
                {
                    Game1.addHUDMessage(new HUDMessage($"{modelName} has no alternative textures for this season!", 3));
                    return CancelUsing(who);
                }

                // Display texture menu
                Game1.activeClickableMenu = new PaintBucketMenu(targetedObject, modelName);

                return CancelUsing(who);
            }

            var targetedTerrain = GetTerrainFeatureAt(location, x, y);
            if (targetedTerrain != null)
            {
                if (targetedTerrain is HoeDirt || targetedTerrain is GiantCrop || targetedTerrain is Tree || targetedTerrain is FruitTree || targetedTerrain is Grass)
                {
                    Game1.addHUDMessage(new HUDMessage($"You can't put paint on that!", 3));
                    return CancelUsing(who);
                }

                if (!targetedTerrain.modData.ContainsKey("AlternativeTextureName"))
                {
                    if (targetedTerrain is Flooring flooring)
                    {
                        if (GetFloorSheetId(flooring) == -1)
                        {
                            return CancelUsing(who);
                        }

                        var instanceSeasonName = $"{AlternativeTextureModel.TextureType.Flooring}_{GetFlooringName(flooring)}_{Game1.GetSeasonForLocation(Game1.currentLocation)}";
                        targetedTerrain.modData["AlternativeTextureSheetId"] = GetFloorSheetId(flooring).ToString();
                        AssignDefaultModData(targetedTerrain, instanceSeasonName, true);
                    }
                    else
                    {
                        return CancelUsing(who);
                    }
                }

                var modelName = targetedTerrain.modData["AlternativeTextureName"].Replace($"{targetedTerrain.modData["AlternativeTextureOwner"]}.", String.Empty);
                if (targetedTerrain.modData.ContainsKey("AlternativeTextureSeason") && !String.IsNullOrEmpty(targetedTerrain.modData["AlternativeTextureSeason"]))
                {
                    modelName = modelName.Replace($"_{targetedTerrain.modData["AlternativeTextureSeason"]}", String.Empty);
                }

                if (AlternativeTextures.textureManager.GetAvailableTextureModels(modelName, Game1.GetSeasonForLocation(Game1.currentLocation)).Count == 0 || !targetedTerrain.modData.ContainsKey("AlternativeTextureSheetId"))
                {
                    Game1.addHUDMessage(new HUDMessage($"{modelName} has no alternative textures for this season!", 3));
                    return CancelUsing(who);
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

                return CancelUsing(who);
            }

            return CancelUsing(who);
        }

        private static bool CancelUsing(Farmer who)
        {
            who.CanMove = true;
            who.UsingTool = false;
            return false;
        }
    }
}
