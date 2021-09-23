using AlternativeTextures;
using AlternativeTextures.Framework.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace AlternativeTextures.Framework.Patches.GameLocations
{
    internal class DecoratableLocationPatch : PatchTemplate
    {
        private readonly Type _object = typeof(DecoratableLocation);

        internal DecoratableLocationPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "IsFloorableOrWallpaperableTile", new[] { typeof(int), typeof(int), typeof(string) }), postfix: new HarmonyMethod(GetType(), nameof(IsFloorableOrWallpaperableTilePostfix)));
        }

        private static void IsFloorableOrWallpaperableTilePostfix(DecoratableLocation __instance, ref bool __result, int x, int y, string layer_name)
        {
            if (__result)
            {
                return;
            }

            var layer = __instance.map.GetLayer(layer_name);
            if (layer != null && x < layer.LayerWidth && y < layer.LayerHeight && layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet != null && layer.Tiles[x, y].TileSheet.Id.Contains(AlternativeTextures.TEXTURE_TOKEN_HEADER))
            {
                for (int w = 0; w < __instance.getWalls().Count(); w++)
                {
                    if (__instance.modData.ContainsKey($"AlternativeTexture.Wallpaper.Owner_{w}") && __instance.getWalls()[w].Contains(x, y))
                    {
                        __result = true;
                        return;
                    }
                }

                for (int f = 0; f < __instance.getFloors().Count(); f++)
                {
                    if (__instance.modData.ContainsKey($"AlternativeTexture.Floor.Owner_{f}") && __instance.getFloors()[f].Contains(x, y))
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }
    }
}