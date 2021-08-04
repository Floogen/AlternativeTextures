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
    internal class CropPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Crop);

        internal CropPatch(IMonitor modMonitor) : base(modMonitor)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Crop.draw), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(Color), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Crop.drawWithOffset), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(Color), typeof(float), typeof(Vector2) }), prefix: new HarmonyMethod(GetType(), nameof(DrawWithOffsetPrefix)));
        }

        private static bool DrawPrefix(Crop __instance, Vector2 ___origin, Vector2 ___drawPosition, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
        {
            var hoeDirt = Game1.currentLocation.terrainFeatures[tileLocation] as HoeDirt;
            if (hoeDirt != null && hoeDirt.modData.ContainsKey("AlternativeTextureName"))
            {
                var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(hoeDirt.modData["AlternativeTextureName"]);
                if (textureModel is null)
                {
                    return true;
                }

                var textureVariation = Int32.Parse(hoeDirt.modData["AlternativeTextureVariation"]);
                if (textureVariation == -1)
                {
                    return true;
                }

                Vector2 position = Game1.GlobalToLocal(Game1.viewport, ___drawPosition);
                SpriteEffects effect = (__instance.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                var layerDepth = (tileLocation.Y * 64f + 32f + ((!__instance.shouldDrawDarkWhenWatered() || (int)__instance.currentPhase >= __instance.phaseDays.Count - 1) ? 0f : ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f))) / 10000f / (((int)__instance.currentPhase == 0 && __instance.shouldDrawDarkWhenWatered()) ? 2f : 1f);
                var sourceX = ((!__instance.fullyGrown) ? ((int)(((int)__instance.phaseToShow != -1) ? __instance.phaseToShow : __instance.currentPhase) + (((int)(((int)__instance.phaseToShow != -1) ? __instance.phaseToShow : __instance.currentPhase) == 0 && ((int)tileLocation.X * 7 + (int)tileLocation.Y * 11) % 2 == 0) ? (-1) : 0) + 1) : (((int)__instance.dayOfCurrentPhase <= 0) ? 6 : 7)) * 16;

                // TODO: Handle color overlays
                b.Draw(textureModel.Texture, position, new Rectangle(sourceX, (textureVariation * textureModel.TextureHeight), 16, 32), toTint, rotation, ___origin, 4f, effect, layerDepth);

                return false;
            }
            return true;
        }

        private static bool DrawWithOffsetPrefix(Crop __instance, Vector2 ___origin, Vector2 ___drawPosition, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation, Vector2 offset)
        {
            var gardenPot = Game1.currentLocation.getObjectAtTile((int)tileLocation.X, (int)tileLocation.Y) as IndoorPot;
            if (gardenPot is null)
            {
                return true;
            }

            var hoeDirt = gardenPot.hoeDirt.Value;
            if (hoeDirt != null && hoeDirt.modData.ContainsKey("AlternativeTextureName"))
            {
                var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(hoeDirt.modData["AlternativeTextureName"]);
                if (textureModel is null)
                {
                    return true;
                }

                var textureVariation = Int32.Parse(hoeDirt.modData["AlternativeTextureVariation"]);
                if (textureVariation == -1)
                {
                    return true;
                }

                var sourceX = ((!__instance.fullyGrown) ? ((int)(((int)__instance.phaseToShow != -1) ? __instance.phaseToShow : __instance.currentPhase) + (((int)(((int)__instance.phaseToShow != -1) ? __instance.phaseToShow : __instance.currentPhase) == 0 && ((int)tileLocation.X * 7 + (int)tileLocation.Y * 11) % 2 == 0) ? (-1) : 0) + 1) : (((int)__instance.dayOfCurrentPhase <= 0) ? 6 : 7)) * 16;
                if ((bool)__instance.forageCrop)
                {
                    b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(sourceX, (textureVariation * textureModel.TextureHeight), 16, 32), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
                    return false;
                }

                b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(sourceX, (textureVariation * textureModel.TextureHeight), 16, 32), toTint, rotation, new Vector2(8f, 24f), 4f, __instance.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
                if (!__instance.tintColor.Equals(Color.White) && (int)__instance.currentPhase == __instance.phaseDays.Count - 1 && !__instance.dead)
                {
                    // TODO: Handle color overlays
                    //b.Draw(Game1.cropSpriteSheet, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(sourceX, (textureVariation * textureModel.TextureHeight), 16, 32), __instance.tintColor, rotation, new Vector2(8f, 24f), 4f, __instance.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.67f) * 64f / 10000f + tileLocation.X * 1E-05f);
                }

                return false;
            }
            return true;
        }
    }
}
