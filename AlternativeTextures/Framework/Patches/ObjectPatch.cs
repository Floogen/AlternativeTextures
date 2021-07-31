using AlternativeTextures;
using Harmony;
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
        private static IMonitor monitor;
        private readonly Type _object = typeof(Object);

        internal ObjectPatch(IMonitor modMonitor)
        {
            monitor = modMonitor;
        }

        internal void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Object.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Object.placementAction), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(PlacementActionPrefix)));
        }

        private static bool DrawPrefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (__instance.modData.ContainsKey("AlternativeTextureOwner"))
            {
                var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(String.Concat(__instance.modData["AlternativeTextureOwner"], ":", __instance.parentSheetIndex));
                if (textureModel is null)
                {
                    return true;
                }

                var textureVariation = Int32.Parse(__instance.modData["AlternativeTextureVariation"]);
                if (textureVariation == 0)
                {
                    return true;
                }

                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 4f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                if (__instance.ParentSheetIndex == 105 || __instance.ParentSheetIndex == 264)
                {
                    draw_layer = Math.Max(0f, (float)((y + 1) * 64 + 2) / 10000f) + (float)x / 1000000f;
                }
                if ((int)__instance.parentSheetIndex == 272)
                {
                    spriteBatch.Draw(textureModel.Texture, destination, Object.getSourceRectForBigCraftable(__instance.ParentSheetIndex + 1), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
                    spriteBatch.Draw(textureModel.Texture, position + new Vector2(8.5f, 12f) * 4f, Object.getSourceRectForBigCraftable(__instance.ParentSheetIndex + 2), Color.White * alpha, (float)Game1.currentGameTime.TotalGameTime.TotalSeconds * -1.5f, new Vector2(7.5f, 15.5f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
                    return false;
                }
                spriteBatch.Draw(textureModel.Texture, destination, new Rectangle((textureVariation - 1) * textureModel.TextureWidth, 0, textureModel.TextureWidth, textureModel.TextureHeight), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
                if (__instance.Name.Equals("Loom") && (int)__instance.minutesUntilReady > 0)
                {
                    spriteBatch.Draw(Game1.objectSpriteSheet, __instance.getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16), Color.White * alpha, __instance.scale.X, new Vector2(8f, 8f), 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64) / 10000f + 0.0001f + (float)x * 1E-05f));
                }
                if ((bool)__instance.isLamp && Game1.isDarkOut())
                {
                    spriteBatch.Draw(Game1.mouseCursors, position + new Vector2(-32f, -32f), new Rectangle(88, 1779, 32, 32), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x / 1000000f);
                }
                if ((int)__instance.parentSheetIndex == 126 && (int)__instance.quality != 0)
                {
                    spriteBatch.Draw(FarmerRenderer.hatsTexture, position + new Vector2(-3f, -6f) * 4f, new Rectangle(((int)__instance.quality - 1) * 20 % FarmerRenderer.hatsTexture.Width, ((int)__instance.quality - 1) * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x * 1E-05f);
                }

                return false;
            }
            return true;
        }

        internal static bool PlacementActionPrefix(Object __instance, ref bool __result, GameLocation location, int x, int y, Farmer who = null)
        {
            if (AlternativeTextures.textureManager.DoesObjectHaveAlternativeTexture(__instance.parentSheetIndex))
            {
                var textureModel = AlternativeTextures.textureManager.GetRandomTextureModel(__instance.parentSheetIndex);
                __instance.modData["AlternativeTextureOwner"] = textureModel.Owner;
                __instance.modData["AlternativeTextureVariation"] = Game1.random.Next(0, textureModel.Variations + 1).ToString();
            }

            return true;
        }
    }
}
