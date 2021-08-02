using AlternativeTextures;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace AlternativeTextures.Framework.Patches
{
    internal class HoeDirtPatch
    {
        private static IMonitor _monitor;
        private readonly Type _object = typeof(HoeDirt);

        internal HoeDirtPatch(IMonitor modMonitor)
        {
            _monitor = modMonitor;
        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(HoeDirt.plant), new[] { typeof(int), typeof(int), typeof(int), typeof(Farmer), typeof(bool), typeof(GameLocation) }), postfix: new HarmonyMethod(GetType(), nameof(PlantPostfix)));
        }

        private static void PlantPostfix(HoeDirt __instance, int index, int tileX, int tileY, Farmer who, bool isFertilizer, GameLocation location)
        {
            var seedName = Game1.objectInformation.ContainsKey(index) ? Game1.objectInformation[index].Split('/')[0] : String.Empty;
            var textureModel = AlternativeTextures.textureManager.GetRandomTextureModel(seedName);
            __instance.modData["AlternativeTextureOwner"] = String.Concat(textureModel.Owner, ".", seedName);

            var selectedVariation = Game1.random.Next(-1, textureModel.Variations);
            __instance.modData["AlternativeTextureVariation"] = selectedVariation.ToString();
        }
    }
}
