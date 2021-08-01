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
    internal class FencePatch
    {
        private static IMonitor _monitor;
        private readonly Type _object = typeof(Fence);

        internal FencePatch(IMonitor modMonitor)
        {
            _monitor = modMonitor;
        }

        internal void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Fence.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Fence.loadFenceTexture), null), prefix: new HarmonyMethod(GetType(), nameof(LoadFenceTexturePrefix)));
        }

        private static bool DrawPrefix(Fence __instance, SpriteBatch b, int x, int y, float alpha = 1f)
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
                int sourceRectPosition = 1;
                if ((float)__instance.health > 1f || __instance.repairQueued.Value)
                {
                    int drawSum = __instance.getDrawSum(Game1.currentLocation);
                    sourceRectPosition = Fence.fenceDrawGuide[drawSum];
                }

                b.Draw(textureModel.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64)), new Rectangle((textureVariation * textureModel.TextureWidth) + (sourceRectPosition * Fence.fencePieceWidth % __instance.fenceTexture.Value.Bounds.Width), sourceRectPosition * Fence.fencePieceWidth / __instance.fenceTexture.Value.Bounds.Width * Fence.fencePieceHeight, Fence.fencePieceWidth, Fence.fencePieceHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32) / 10000f);

                return false;
            }
            return true;
        }

        private static bool LoadFenceTexturePrefix(Fence __instance)
        {
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
