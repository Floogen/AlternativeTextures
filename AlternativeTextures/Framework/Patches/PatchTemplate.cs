using AlternativeTextures.Framework.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Linq;
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

        internal static bool AssignDefaultModData<T>(T type, string modelName, bool trackSeason = false, bool trackSheetId = false)
        {
            var textureModel = new AlternativeTextureModel() { Owner = AlternativeTextures.DEFAULT_OWNER, Season = trackSeason ? Game1.currentSeason : String.Empty };
            switch (type)
            {
                case Object obj:
                    AssignObjectModData(obj, modelName, textureModel, -1, trackSeason, trackSheetId);
                    return true;
                case TerrainFeature terrain:
                    AssignTerrainFeatureModData(terrain, modelName, textureModel, -1, trackSeason);
                    return true;
            }

            return false;
        }

        internal static bool AssignModData<T>(T type, string modelName, bool trackSeason = false, bool trackSheetId = false)
        {
            var textureModel = AlternativeTextures.textureManager.GetRandomTextureModel(modelName);

            var selectedVariation = Game1.random.Next(-1, textureModel.Variations);
            if (textureModel.ManualVariations.Count() > 0)
            {
                var weightedSelection = textureModel.ManualVariations.Where(v => v.ChanceWeight >= Game1.random.NextDouble()).ToList();
                if (weightedSelection.Count > 0)
                {
                    var randomWeightedSelection = Game1.random.Next(!textureModel.ManualVariations.Any(v => v.Id == -1) ? -1 : 0, weightedSelection.Count());
                    selectedVariation = randomWeightedSelection == -1 ? -1 : weightedSelection[randomWeightedSelection].Id;
                }
            }

            switch (type)
            {
                case Object obj:
                    AssignObjectModData(obj, modelName, textureModel, selectedVariation, trackSeason, trackSheetId);
                    return true;
                case TerrainFeature terrain:
                    AssignTerrainFeatureModData(terrain, modelName, textureModel, selectedVariation, trackSeason);
                    return true;
            }

            return false;
        }

        private static void AssignObjectModData(Object obj, string modelName, AlternativeTextureModel textureModel, int variation, bool trackSeason = false, bool trackSheetId = false)
        {
            obj.modData["AlternativeTextureOwner"] = textureModel.Owner;
            obj.modData["AlternativeTextureName"] = String.Concat(textureModel.Owner, ".", modelName);

            if (trackSeason && !String.IsNullOrEmpty(textureModel.Season))
            {
                obj.modData["AlternativeTextureSeason"] = Game1.currentSeason;
            }

            if (trackSheetId)
            {
                obj.modData["AlternativeTextureSheetId"] = obj.ParentSheetIndex.ToString();
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
