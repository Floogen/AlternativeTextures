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

        internal static void AssignModData<T>(T type, string textureModelName, bool trackSeason = false)
        {
            var textureModel = AlternativeTextures.textureManager.GetRandomTextureModel(textureModelName);
            var selectedVariation = Game1.random.Next(-1, textureModel.Variations);

            switch (type)
            {
                case Object obj:
                    AssignObjectModData(type as Object, textureModel, selectedVariation, trackSeason);
                    return;
                case TerrainFeature terrain:
                    AssignTerrainFeatureModData(type as TerrainFeature, textureModel, selectedVariation, trackSeason);
                    return;
            }
        }

        internal static void AssignObjectModData(Object obj, AlternativeTextureModel textureModel, int variation, bool trackSeason = false)
        {
            obj.modData["AlternativeTextureOwner"] = textureModel.Owner;
            obj.modData["AlternativeTextureName"] = String.Concat(textureModel.Owner, ".", textureModel.ItemName);

            if (trackSeason)
            {
                obj.modData["AlternativeTextureSeason"] = Game1.currentSeason;
            }

            obj.modData["AlternativeTextureVariation"] = variation.ToString();
        }

        internal static void AssignTerrainFeatureModData(TerrainFeature terrain, AlternativeTextureModel textureModel, int variation, bool trackSeason = false)
        {
            terrain.modData["AlternativeTextureOwner"] = textureModel.Owner;
            terrain.modData["AlternativeTextureName"] = String.Concat(textureModel.Owner, ".", textureModel.ItemName);

            if (trackSeason)
            {
                terrain.modData["AlternativeTextureSeason"] = Game1.currentSeason;
            }

            terrain.modData["AlternativeTextureVariation"] = variation.ToString();
        }
    }
}
