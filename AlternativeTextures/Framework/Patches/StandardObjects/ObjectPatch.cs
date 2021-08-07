using AlternativeTextures;
using AlternativeTextures.Framework.Models;
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

namespace AlternativeTextures.Framework.Patches.StandardObjects
{
    internal class ObjectPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Object);

        internal ObjectPatch(IMonitor modMonitor) : base(modMonitor)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Object.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Object.drawPlacementBounds), new[] { typeof(SpriteBatch), typeof(GameLocation) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPlacementBoundsPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Object.placementAction), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer) }), postfix: new HarmonyMethod(GetType(), nameof(PlacementActionPostfix)));
        }

        private static bool DrawPrefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (__instance.modData.ContainsKey("AlternativeTextureName"))
            {
                var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(__instance.modData["AlternativeTextureName"]);
                if (textureModel is null)
                {
                    return true;
                }

                var textureVariation = Int32.Parse(__instance.modData["AlternativeTextureVariation"]);
                if (textureVariation == -1)
                {
                    return true;
                }
                var textureOffset = textureVariation * textureModel.TextureHeight;

                // Get the current X index for the source tile
                var xTileOffset = __instance.modData.ContainsKey("AlternativeTextureSheetId") ? __instance.ParentSheetIndex - Int32.Parse(__instance.modData["AlternativeTextureSheetId"]) : 0;
                if (__instance.showNextIndex)
                {
                    xTileOffset += 1;
                }
                xTileOffset *= textureModel.TextureWidth;

                // Perform base game draw logic
                Vector2 scaleFactor = __instance.getScale() * 4f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;

                spriteBatch.Draw(textureModel.Texture, destination, new Rectangle(xTileOffset, textureOffset, textureModel.TextureWidth, textureModel.TextureHeight), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);

                // Replicate the extra draw logic from the base game
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

        internal static bool DrawPlacementBoundsPrefix(Object __instance, SpriteBatch spriteBatch, GameLocation location)
        {
            if (__instance.modData.ContainsKey("AlternativeTextureName"))
            {
                __instance.modData.Remove("AlternativeTextureName");
            }
            return true;
        }

        internal static void PlacementActionPostfix(Object __instance, bool __result, GameLocation location, int x, int y, Farmer who = null)
        {
            if (!__result)
            {
                return;
            }

            // Used for most objects, except for those whom are converted upon placement (such as Fences)
            var placedObject = location.getObjectAt(x, y);
            if (placedObject is null)
            {
                return;
            }

            var instanceName = $"{AlternativeTextureModel.TextureType.Craftable}_{placedObject.name}";
            if (AlternativeTextures.textureManager.DoesObjectHaveAlternativeTexture(instanceName))
            {
                AssignModData(placedObject, instanceName, false, placedObject.bigCraftable);
                return;
            }

            instanceName = $"{instanceName}_{Game1.currentSeason}";
            if (AlternativeTextures.textureManager.DoesObjectHaveAlternativeTexture(instanceName))
            {
                AssignModData(placedObject, instanceName, true, placedObject.bigCraftable);
                return;
            }
        }
    }
}
