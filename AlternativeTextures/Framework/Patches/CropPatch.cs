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
    internal class CropPatch
    {
        private static IMonitor _monitor;
        private readonly Type _object = typeof(Crop);

        internal CropPatch(IMonitor modMonitor)
        {
            _monitor = modMonitor;
        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Crop.draw), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(Color), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
            //harmony.Patch(AccessTools.Constructor(typeof(Crop)), postfix: new HarmonyMethod(GetType(), nameof(CropPostfix)));
            // TODO: Harmony patch the Crop class to apply textures
        }

        private static bool DrawPrefix(Crop __instance, Vector2 ___origin, Vector2 ___drawPosition, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
        {
            var hoeDirt = Game1.currentLocation.terrainFeatures[tileLocation] as HoeDirt;
            if (hoeDirt != null && hoeDirt.modData.ContainsKey("AlternativeTextureOwner"))
            {
                var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(hoeDirt.modData["AlternativeTextureOwner"]);
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

                b.Draw(textureModel.Texture, position, new Rectangle(sourceX, (textureVariation * textureModel.TextureHeight), 16, 32), toTint, rotation, ___origin, 4f, effect, layerDepth);

                return false;
            }
            return true;
        }
    }
}
