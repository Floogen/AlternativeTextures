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
    internal class FencePatch : PatchTemplate
    {
        private readonly Type _object = typeof(Fence);

        internal FencePatch(IMonitor modMonitor) : base(modMonitor)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Fence.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
        }

        private static bool DrawPrefix(Fence __instance, SpriteBatch b, int x, int y, float alpha = 1f)
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
                int sourceRectPosition = 1;
                if ((float)__instance.health > 1f || __instance.repairQueued.Value)
                {
                    int drawSum = __instance.getDrawSum(Game1.currentLocation);
                    sourceRectPosition = Fence.fenceDrawGuide[drawSum];
                }

                var gateOffset = __instance.isGate ? 128 : 0;
                var textureOffset = textureVariation * textureModel.TextureHeight;
                if ((bool)__instance.isGate)
                {
                    Vector2 offset = new Vector2(0f, 0f);
                    int drawSum = __instance.getDrawSum(Game1.currentLocation);
                    switch (drawSum)
                    {
                        case 10:
                            b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 - 16, y * 64 - 128)), new Rectangle(((int)__instance.gatePosition == 88) ? 24 : 0, textureOffset + (192 - gateOffset), 24, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
                            return false;
                        case 100:
                            b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 - 16, y * 64 - 128)), new Rectangle(((int)__instance.gatePosition == 88) ? 24 : 0, textureOffset + (240 - gateOffset), 24, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
                            return false;
                        case 1000:
                            b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle(((int)__instance.gatePosition == 88) ? 24 : 0, textureOffset + (288 - gateOffset), 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
                            return false;
                        case 500:
                            b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle(((int)__instance.gatePosition == 88) ? 24 : 0, textureOffset + (320 - gateOffset), 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
                            return false;
                        case 110:
                            b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 - 16, y * 64 - 64)), new Rectangle(((int)__instance.gatePosition == 88) ? 24 : 0, textureOffset + (128 - gateOffset), 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
                            return false;
                        case 1500:
                            b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle(((int)__instance.gatePosition == 88) ? 16 : 0, textureOffset + (160 - gateOffset), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
                            b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 + 44)), new Rectangle(((int)__instance.gatePosition == 88) ? 16 : 0, textureOffset + (176 - gateOffset), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
                            return false;
                    }
                    sourceRectPosition = 5;
                }

                b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64)), new Rectangle((sourceRectPosition * Fence.fencePieceWidth % __instance.fenceTexture.Value.Bounds.Width), textureOffset + (sourceRectPosition * Fence.fencePieceWidth / __instance.fenceTexture.Value.Bounds.Width * Fence.fencePieceHeight), Fence.fencePieceWidth, Fence.fencePieceHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32) / 10000f);

                return false;
            }
            return true;
        }
    }
}
