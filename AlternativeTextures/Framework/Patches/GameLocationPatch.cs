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

namespace AlternativeTextures.Framework.Patches
{
    internal class GameLocationPatch : PatchTemplate
    {
        private readonly Type _object = typeof(GameLocation);

        internal GameLocationPatch(IMonitor modMonitor) : base(modMonitor)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(GameLocation.seasonUpdate), new[] { typeof(string), typeof(bool) }), postfix: new HarmonyMethod(GetType(), nameof(SeasonUpdatePostfix)));
        }

        internal static void SeasonUpdatePostfix(GameLocation __instance, string season, bool onLoad = false)
        {
            for (int k = __instance.objects.Count() - 1; k >= 0; k--)
            {
                var obj = __instance.objects.Pairs.ElementAt(k).Value;
                if (obj.modData.ContainsKey("AlternativeTextureSeason") && !String.IsNullOrEmpty(obj.modData["AlternativeTextureSeason"]) && !String.Equals(obj.modData["AlternativeTextureSeason"], Game1.currentSeason, StringComparison.OrdinalIgnoreCase))
                {
                    obj.modData["AlternativeTextureSeason"] = Game1.currentSeason;
                    obj.modData["AlternativeTextureName"] = String.Concat(obj.modData["AlternativeTextureOwner"], ".", $"{AlternativeTextureModel.TextureType.Craftable}_{obj.name}_{obj.modData["AlternativeTextureSeason"]}");
                }
            }

        }
    }
}