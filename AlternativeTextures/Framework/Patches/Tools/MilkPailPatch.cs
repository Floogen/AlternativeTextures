using AlternativeTextures;
using AlternativeTextures.Framework.Models;
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
            harmony.Patch(AccessTools.PropertyGetter(typeof(Tool), "DisplayName"), postfix: new HarmonyMethod(GetType(), nameof(LoadDisplayNamePostfix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Tool), "description"), postfix: new HarmonyMethod(GetType(), nameof(LoadDescriptionPostfix)));
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
    }
}
