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

namespace AlternativeTextures.Framework.Patches
{
    internal class UtilityPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Utility);

        internal UtilityPatch(IMonitor modMonitor) : base(modMonitor)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Utility.getCarpenterStock), null), postfix: new HarmonyMethod(GetType(), nameof(GetCarpenterStockPostFix)));
        }

        private static void GetCarpenterStockPostFix(Utility __instance, ref Dictionary<ISalable, int[]> __result)
        {
            var paintBucket = new GenericTool("Paint Bucket", "Allows you to apply different textures to supported objects.", -1, 6, 6);
            paintBucket.modData["AlternativeTexturesPaintBucketFlag"] = true.ToString();

            __result.Add(paintBucket, new int[2] { 500, 1 });
        }
    }
}
