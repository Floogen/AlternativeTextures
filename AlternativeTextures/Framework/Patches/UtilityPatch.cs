using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace AlternativeTextures.Framework.Patches
{
    internal class UtilityPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Utility);

        internal UtilityPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Utility.getCarpenterStock), null), postfix: new HarmonyMethod(GetType(), nameof(GetCarpenterStockPostFix)));
        }

        private static void GetCarpenterStockPostFix(Utility __instance, ref Dictionary<ISalable, int[]> __result)
        {
            __result.Add(GetPaintBucketTool(), new int[2] { 500, 1 });
            __result.Add(GetScissorsTool(), new int[2] { 500, 1 });
            __result.Add(GetPaintBrushTool(), new int[2] { 500, 1 });
            __result.Add(GetSprayCanTool(), new int[2] { 500, 1 });
        }
    }
}
