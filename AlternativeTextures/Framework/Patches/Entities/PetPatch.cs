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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace AlternativeTextures.Framework.Patches.Entities
{
    internal class PetPatch : PatchTemplate
    {
        private readonly Type _entity = typeof(Pet);

        internal PetPatch(IMonitor modMonitor) : base(modMonitor)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_entity, nameof(Pet.reloadBreedSprite), null), postfix: new HarmonyMethod(GetType(), nameof(ReloadBreedSpritePostfix)));
            harmony.Patch(AccessTools.Method(_entity, nameof(Pet.update), new[] { typeof(GameTime), typeof(GameLocation) }), postfix: new HarmonyMethod(GetType(), nameof(ReloadBreedSpritePostfix)));

            harmony.Patch(AccessTools.Constructor(typeof(Cat), new[] { typeof(int), typeof(int), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(PetPostfix)));
            harmony.Patch(AccessTools.Constructor(typeof(Dog), new[] { typeof(int), typeof(int), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(PetPostfix)));
        }

        private static void ReloadBreedSpritePostfix(Pet __instance)
        {
            if (__instance.modData.ContainsKey("AlternativeTextureName"))
            {
                var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(__instance.modData["AlternativeTextureName"]);
                if (textureModel is null)
                {
                    __instance.Sprite.LoadTexture(__instance.getPetTextureName());
                    return;
                }

                var textureVariation = Int32.Parse(__instance.modData["AlternativeTextureVariation"]);
                if (textureVariation == -1)
                {
                    __instance.Sprite.LoadTexture(__instance.getPetTextureName());
                    return;
                }
                var textureOffset = textureVariation * textureModel.TextureHeight;

                __instance.Sprite.spriteTexture = textureModel.Texture;
                __instance.Sprite.sourceRect.Y = textureOffset + (__instance.Sprite.currentFrame * __instance.Sprite.SpriteWidth / __instance.Sprite.Texture.Width * __instance.Sprite.SpriteHeight);
            }

            return;
        }

        private static void UpdatePostfix(Pet __instance, GameTime time, GameLocation location)
        {
            if (!__instance.modData.ContainsKey("AlternativeTextureName"))
            {
                return;
            }

            var instanceName = String.Concat(__instance.modData["AlternativeTextureOwner"], ".", $"{AlternativeTextureModel.TextureType.Character}_{GetCharacterName(__instance)}");
            var instanceSeasonName = $"{instanceName}_{Game1.GetSeasonForLocation(__instance.currentLocation)}";
            if (__instance is Pet pet && pet.modData["AlternativeTextureName"].ToLower() != instanceName && pet.modData["AlternativeTextureName"].ToLower() != instanceSeasonName)
            {
                pet.modData["AlternativeTextureName"] = String.Concat(pet.modData["AlternativeTextureOwner"], ".", $"{AlternativeTextureModel.TextureType.Character}_{GetCharacterName(pet)}");
                if (pet.modData.ContainsKey("AlternativeTextureSeason") && !String.IsNullOrEmpty(__instance.modData["AlternativeTextureSeason"]))
                {
                    pet.modData["AlternativeTextureSeason"] = Game1.GetSeasonForLocation(location);
                    pet.modData["AlternativeTextureName"] = String.Concat(pet.modData["AlternativeTextureName"], "_", pet.modData["AlternativeTextureSeason"]);
                }
            }
        }

        private static void PetPostfix(Pet __instance, int xTile, int yTile, int breed)
        {
            var instanceName = $"{AlternativeTextureModel.TextureType.Character}_{GetCharacterName(__instance)}";
            var instanceSeasonName = $"{instanceName}_{Game1.GetSeasonForLocation(__instance.currentLocation)}";

            if (AlternativeTextures.textureManager.DoesObjectHaveAlternativeTexture(instanceName) && AlternativeTextures.textureManager.DoesObjectHaveAlternativeTexture(instanceSeasonName))
            {
                var result = Game1.random.Next(2) > 0 ? AssignModData(__instance, instanceSeasonName, true) : AssignModData(__instance, instanceName, false);
                return;
            }
            else
            {
                if (AlternativeTextures.textureManager.DoesObjectHaveAlternativeTexture(instanceName))
                {
                    AssignModData(__instance, instanceName, false);
                    return;
                }

                if (AlternativeTextures.textureManager.DoesObjectHaveAlternativeTexture(instanceSeasonName))
                {
                    AssignModData(__instance, instanceSeasonName, true);
                    return;
                }
            }

            AssignDefaultModData(__instance, instanceSeasonName, true);
        }
    }
}
