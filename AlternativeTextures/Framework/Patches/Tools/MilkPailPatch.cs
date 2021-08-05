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
            if (targetedObject is null || !targetedObject.modData.ContainsKey("AlternativeTextureName"))
            {
                who.CanMove = true;
                who.UsingTool = false;
                return false;
            }

            var modelName = targetedObject.modData["AlternativeTextureName"].Replace($"{targetedObject.modData["AlternativeTextureOwner"]}.", String.Empty);
            if (AlternativeTextures.textureManager.GetAvailableTextureModels(modelName).Count == 0)
            {
                Game1.addHUDMessage(new HUDMessage($"{targetedObject.Name} has no alternative textures available!", 3));
                who.CanMove = true;
                who.UsingTool = false;
                return false;
            }

            // Display texture menu
            Game1.activeClickableMenu = new PaintBucketMenu(targetedObject);

            who.CanMove = true;
            who.UsingTool = false;
            return false;
        }
    }
}
