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

                b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64)), new Rectangle((sourceRectPosition * Fence.fencePieceWidth % __instance.fenceTexture.Value.Bounds.Width), (textureVariation * textureModel.TextureHeight) + (sourceRectPosition * Fence.fencePieceWidth / __instance.fenceTexture.Value.Bounds.Width * Fence.fencePieceHeight), Fence.fencePieceWidth, Fence.fencePieceHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32) / 10000f);

                return false;
            }
            return true;
        }
    }
}
