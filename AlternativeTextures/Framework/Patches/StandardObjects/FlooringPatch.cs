using AlternativeTextures;
using AlternativeTextures.Framework.Models;
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

namespace AlternativeTextures.Framework.Patches.StandardObjects
{
    internal class FlooringPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Flooring);

        public FlooringPatch(IMonitor modMonitor) : base(modMonitor)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Flooring.draw), new[] { typeof(SpriteBatch), typeof(Vector2) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Flooring.seasonUpdate), new[] { typeof(bool) }), postfix: new HarmonyMethod(GetType(), nameof(SeasonUpdatePostfix)));
        }

        private static bool DrawPrefix(Flooring __instance, byte ___neighborMask, SpriteBatch spriteBatch, Vector2 tileLocation)
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

                if (__instance.cornerDecoratedBorders.Value)
                {
                    int border_size = 6;
                    if ((___neighborMask & 9) == 9 && (___neighborMask & 0x20) == 0)
                    {
                        spriteBatch.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(64 - border_size, (48 - border_size) + textureOffset, border_size, border_size), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
                    }
                    if ((___neighborMask & 3) == 3 && (___neighborMask & 0x10) == 0)
                    {
                        spriteBatch.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 64f - (float)(border_size * 4), tileLocation.Y * 64f)), new Rectangle(16, (48 - border_size) + textureOffset, border_size, border_size), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
                    }
                    if ((___neighborMask & 6) == 6 && (___neighborMask & 0x40) == 0)
                    {
                        spriteBatch.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 64f - (float)(border_size * 4), tileLocation.Y * 64f + 64f - (float)(border_size * 4))), new Rectangle(16, textureOffset, border_size, border_size), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
                    }
                    if ((___neighborMask & 0xC) == 12 && (___neighborMask & 0x80) == 0)
                    {
                        spriteBatch.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f + 64f - (float)(border_size * 4))), new Rectangle(64 - border_size, textureOffset, border_size, border_size), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
                    }
                }
                else if (!__instance.isPathway)
                {
                    if ((___neighborMask & 9) == 9 && (___neighborMask & 0x20) == 0)
                    {
                        spriteBatch.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(60, 44 + textureOffset, 4, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
                    }
                    if ((___neighborMask & 3) == 3 && (___neighborMask & 0x10) == 0)
                    {
                        spriteBatch.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 48f, tileLocation.Y * 64f)), new Rectangle(16, 44 + textureOffset, 4, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
                    }
                    if ((___neighborMask & 6) == 6 && (___neighborMask & 0x40) == 0)
                    {
                        spriteBatch.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 48f, tileLocation.Y * 64f + 48f)), new Rectangle(16, textureOffset, 4, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
                    }
                    if ((___neighborMask & 0xC) == 12 && (___neighborMask & 0x80) == 0)
                    {
                        spriteBatch.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f + 48f)), new Rectangle(60, textureOffset, 4, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
                    }
                    if (!__instance.drawContouredShadow.Value)
                    {
                        spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)(tileLocation.X * 64f) - 4 - Game1.viewport.X, (int)(tileLocation.Y * 64f) + 4 - Game1.viewport.Y, 64, 64), Color.Black * 0.33f);
                    }
                }

                byte drawSum = (byte)(___neighborMask & 0xFu);
                int sourceRectPosition = Flooring.drawGuide[drawSum];
                if ((bool)__instance.isSteppingStone)
                {
                    sourceRectPosition = Flooring.drawGuideList[__instance.whichView.Value];
                }

                if ((bool)__instance.drawContouredShadow)
                {
                    Color shadow_color = Color.Black;
                    shadow_color.A = (byte)((float)(int)shadow_color.A * 0.33f);
                    spriteBatch.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)) + new Vector2(-4f, 4f), new Rectangle(sourceRectPosition * 16 % 256, (sourceRectPosition / 16 * 16) + textureOffset, 16, 16), shadow_color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-10f);
                }

                spriteBatch.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(sourceRectPosition * 16 % 256, (sourceRectPosition / 16 * 16) + textureOffset, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-09f);

                return false;
            }
            return true;
        }

        private static void SeasonUpdatePostfix(Flooring __instance, bool onLoad)
        {
            if (__instance.modData.ContainsKey("AlternativeTextureName") && __instance.modData.ContainsKey("AlternativeTextureSeason") && !String.IsNullOrEmpty(__instance.modData["AlternativeTextureSeason"]))
            {
                __instance.modData["AlternativeTextureSeason"] = Game1.GetSeasonForLocation(__instance.currentLocation);
                __instance.modData["AlternativeTextureName"] = String.Concat(__instance.modData["AlternativeTextureOwner"], ".", $"{AlternativeTextureModel.TextureType.Flooring}_{GetFlooringName(__instance)}_{__instance.modData["AlternativeTextureSeason"]}");
            }
        }
    }
}
