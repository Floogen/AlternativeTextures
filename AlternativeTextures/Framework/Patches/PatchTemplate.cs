using AlternativeTextures.Framework.Models;
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

        internal static void AssignModData<T>(T type, string modelName, bool trackSeason = false)
        {
            var textureModel = AlternativeTextures.textureManager.GetRandomTextureModel(modelName);
            var selectedVariation = Game1.random.Next(-1, textureModel.Variations);

            switch (type)
            {
                case Object obj:
                    AssignObjectModData(type as Object, modelName, textureModel, selectedVariation, trackSeason);
                    return;
                case TerrainFeature terrain:
                    AssignTerrainFeatureModData(type as TerrainFeature, modelName, textureModel, selectedVariation, trackSeason);
                    return;
            }
        }

        private static void AssignObjectModData(Object obj, string modelName, AlternativeTextureModel textureModel, int variation, bool trackSeason = false)
        {
            obj.modData["AlternativeTextureOwner"] = textureModel.Owner;
            obj.modData["AlternativeTextureName"] = String.Concat(textureModel.Owner, ".", modelName);

            if (trackSeason && !String.IsNullOrEmpty(textureModel.Season))
            {
                obj.modData["AlternativeTextureSeason"] = Game1.currentSeason;
            }

            obj.modData["AlternativeTextureVariation"] = variation.ToString();
        }

        private static void AssignTerrainFeatureModData(TerrainFeature terrain, string modelName, AlternativeTextureModel textureModel, int variation, bool trackSeason = false)
        {
            terrain.modData["AlternativeTextureOwner"] = textureModel.Owner;
            terrain.modData["AlternativeTextureName"] = String.Concat(textureModel.Owner, ".", modelName);

            if (trackSeason && !String.IsNullOrEmpty(textureModel.Season))
            {
                terrain.modData["AlternativeTextureSeason"] = Game1.GetSeasonForLocation(terrain.currentLocation);
            }

            terrain.modData["AlternativeTextureVariation"] = variation.ToString();
        }
    }
}
