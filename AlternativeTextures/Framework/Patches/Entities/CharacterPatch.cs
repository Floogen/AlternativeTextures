using AlternativeTextures;
using AlternativeTextures.Framework.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace AlternativeTextures.Framework.Patches.Entities
{
    internal class CharacterPatch : PatchTemplate
    {
        private readonly Type _entity = typeof(Character);

        internal const string BABY_NAME_PREFIX = "Baby";
        internal const string TODDLER_NAME_PREFIX = "Toddler";

        internal CharacterPatch(IMonitor modMonitor) : base(modMonitor)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_entity, nameof(Character.update), new[] { typeof(GameTime), typeof(GameLocation) }), postfix: new HarmonyMethod(GetType(), nameof(UpdatePostfix)));
            harmony.Patch(AccessTools.Method(_entity, nameof(Character.draw), new[] { typeof(SpriteBatch) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
        }

        private static void UpdatePostfix(Character __instance, GameTime time, GameLocation location)
        {
            if (!__instance.modData.ContainsKey("AlternativeTextureName"))
            {
                return;
            }

            var instanceName = String.Concat(__instance.modData["AlternativeTextureOwner"], ".", $"{AlternativeTextureModel.TextureType.Character}_{GetCharacterName(__instance)}");
            var instanceSeasonName = $"{instanceName}_{Game1.GetSeasonForLocation(__instance.currentLocation)}";
            if (__instance is Child child && child.modData["AlternativeTextureName"].ToLower() != instanceName && child.modData["AlternativeTextureName"].ToLower() != instanceSeasonName)
            {
                child.modData["AlternativeTextureName"] = String.Concat(child.modData["AlternativeTextureOwner"], ".", $"{AlternativeTextureModel.TextureType.Character}_{GetCharacterName(child)}");
                if (child.modData.ContainsKey("AlternativeTextureSeason") && !String.IsNullOrEmpty(__instance.modData["AlternativeTextureSeason"]))
                {
                    child.modData["AlternativeTextureSeason"] = Game1.GetSeasonForLocation(location);
                    child.modData["AlternativeTextureName"] = String.Concat(child.modData["AlternativeTextureName"], "_", child.modData["AlternativeTextureSeason"]);
                }
            }
        }

        private static bool DrawPrefix(Character __instance, SpriteBatch b)
        {
            if (__instance.modData.ContainsKey("AlternativeTextureName"))
            {
                var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(__instance.modData["AlternativeTextureName"]);
                if (textureModel is null)
                {
                    __instance.Sprite.loadedTexture = String.Empty;
                    return true;
                }

                var textureVariation = Int32.Parse(__instance.modData["AlternativeTextureVariation"]);
                if (textureVariation == -1)
                {
                    __instance.Sprite.loadedTexture = String.Empty;
                    return true;
                }
                var textureOffset = textureVariation * textureModel.TextureHeight;

                __instance.Sprite.spriteTexture = textureModel.Texture;
                __instance.Sprite.sourceRect.Y = textureOffset + (__instance.Sprite.currentFrame * __instance.Sprite.SpriteWidth / __instance.Sprite.Texture.Width * __instance.Sprite.SpriteHeight);
            }

            return true;
        }
    }
}
