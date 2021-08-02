using AlternativeTextures;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace AlternativeTextures.Framework.Patches
{
    internal class ObjectPatch
    {
        private static IMonitor _monitor;
        private readonly Type _object = typeof(Object);

        internal ObjectPatch(IMonitor modMonitor)
        {
            _monitor = modMonitor;
        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Object.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Object.drawPlacementBounds), new[] { typeof(SpriteBatch), typeof(GameLocation) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPlacementBoundsPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Object.placementAction), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(PlacementActionPrefix)));
        }

        private static bool DrawPrefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (__instance.modData.ContainsKey("AlternativeTextureOwner"))
            {
                var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(String.Concat(__instance.modData["AlternativeTextureOwner"], ".", __instance.name));
                if (textureModel is null)
                {
                    return true;
                }

                var textureVariation = Int32.Parse(__instance.modData["AlternativeTextureVariation"]);
                if (textureVariation == -1)
                {
                    return true;
                }

                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 4f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                spriteBatch.Draw(textureModel.Texture, destination, new Rectangle(textureVariation * textureModel.TextureWidth, 0, textureModel.TextureWidth, textureModel.TextureHeight), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);

                // TODO: Add draw handling for machines (such as loom, furnace, etc.)

                return false;
            }
            return true;
        }

        internal static bool DrawPlacementBoundsPrefix(Object __instance, SpriteBatch spriteBatch, GameLocation location)
        {
            if (__instance.modData.ContainsKey("AlternativeTextureOwner"))
            {
                // TODO: Implement showing what variation will be placed?
                __instance.modData.Remove("AlternativeTextureOwner");
            }
            return true;
        }

        internal static bool PlacementActionPrefix(Object __instance, ref bool __result, GameLocation location, int x, int y, Farmer who = null)
        {
            // Used for most objects, except for those whom are converted upon placement (such as Fences)
            if (AlternativeTextures.textureManager.DoesObjectHaveAlternativeTexture(__instance.name))
            {
                var textureModel = AlternativeTextures.textureManager.GetRandomTextureModel(__instance.name);
                __instance.modData["AlternativeTextureOwner"] = textureModel.Owner;

                var selectedVariation = Game1.random.Next(-1, textureModel.Variations);
                __instance.modData["AlternativeTextureVariation"] = selectedVariation.ToString();
            }

            return true;
        }
    }
}
