using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using Object = StardewValley.Object;

namespace AlternativeTextures.Framework.Patches
{
    internal class PatchTemplate
    {
        internal static IMonitor _monitor;

        internal PatchTemplate(IMonitor modMonitor)
        {
            _monitor = modMonitor;
        }

        internal static void AssignObjectModData(Object obj, string textureModelName, bool trackSeason = false)
        {
            var textureModel = AlternativeTextures.textureManager.GetRandomTextureModel(textureModelName);

            obj.modData["AlternativeTextureOwner"] = textureModel.Owner;
            obj.modData["AlternativeTextureName"] = String.Concat(textureModel.Owner, ".", textureModelName);

            if (trackSeason)
            {
                obj.modData["AlternativeTextureSeason"] = Game1.currentSeason;
            }

            var selectedVariation = Game1.random.Next(-1, textureModel.Variations);
            obj.modData["AlternativeTextureVariation"] = selectedVariation.ToString();
        }

        internal static void AssignTerrainFeatureModData(TerrainFeature terrain, string textureModelName, bool trackSeason = false)
        {
            var textureModel = AlternativeTextures.textureManager.GetRandomTextureModel(textureModelName);

            terrain.modData["AlternativeTextureOwner"] = textureModel.Owner;
            terrain.modData["AlternativeTextureName"] = String.Concat(textureModel.Owner, ".", textureModelName);

            if (trackSeason)
            {
                terrain.modData["AlternativeTextureSeason"] = Game1.currentSeason;
            }

            var selectedVariation = Game1.random.Next(-1, textureModel.Variations);
            terrain.modData["AlternativeTextureVariation"] = selectedVariation.ToString();
        }
    }
}
