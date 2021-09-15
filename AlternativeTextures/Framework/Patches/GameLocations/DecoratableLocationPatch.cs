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
            harmony.Patch(AccessTools.Method(_object, "doSetVisibleFloor", new[] { typeof(int), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(DoSetVisibleFloorPostfix)));
            harmony.Patch(AccessTools.Method(_object, "IsFloorableOrWallpaperableTile", new[] { typeof(int), typeof(int), typeof(string) }), postfix: new HarmonyMethod(GetType(), nameof(IsFloorableOrWallpaperableTilePostfix)));
        }

        private static void IsFloorableOrWallpaperableTilePostfix(DecoratableLocation __instance, ref bool __result, int x, int y, string layer_name)
        {
            if (__result)
            {
                return;
            }

            var layer = __instance.map.GetLayer(layer_name);
            if (layer != null && x < layer.LayerWidth && y < layer.LayerHeight && layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet != null && layer.Tiles[x, y].TileSheet.Id != "walls_and_floors")
            {
                if (__instance.modData.ContainsKey("AlternativeTexture.Floor.Owner") && __instance.modData["AlternativeTexture.Floor.Owner"] == AlternativeTextures.DEFAULT_OWNER)
                {
                    if (Int32.TryParse(__instance.modData["AlternativeTexture.Floor.RoomIndex"], out int room) && __instance.getFloors()[room].Contains(x, y))
                    {
                        layer.Tiles[x, y] = new StaticTile(__instance.map.GetLayer(layer_name), __instance.map.GetTileSheet("walls_and_floors"), BlendMode.Alpha, 336);
                        __result = true;
                    }
                }
            }
        }

        private static void DoSetVisibleFloorPostfix(DecoratableLocation __instance, int whichRoom, int which)
        {
            List<Rectangle> rooms = __instance.getFloors();
            if (!__instance.modData.ContainsKey("AlternativeTexture.Floor.Name"))
            {
                return;
            }

            if (!__instance.modData.ContainsKey("AlternativeTexture.Floor.RoomIndex") || !Int32.TryParse(__instance.modData["AlternativeTexture.Floor.RoomIndex"], out int room) || room != whichRoom)
            {
                return;
            }

            var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(__instance.modData["AlternativeTexture.Floor.Name"]);
            if (textureModel is null || !Int32.TryParse(__instance.modData["AlternativeTexture.Floor.Variation"], out var textureVariation) || textureVariation == -1)
            {
                return;
            }
            var textureOffset = textureVariation * (textureModel.TextureHeight + textureModel.TextureWidth);

            var texturePath = _helper.Content.GetActualAssetKey($"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{textureModel.GetTokenId()}", ContentSource.GameContent);
            var tileSheet = new TileSheet(__instance.map, texturePath, new xTile.Dimensions.Size(textureModel.Textures.First().Width, textureModel.Textures.First().Height), new xTile.Dimensions.Size(16));

            // Add the tileSheet, if it is missing from the map
            if (!__instance.map.GetLayer("Back").Map.TileSheets.Contains(tileSheet))
            {
                __instance.map.GetLayer("Back").Map.AddTileSheet(tileSheet);
            }

            // Modified vanilla logic
            if (whichRoom == -1)
            {
                foreach (Rectangle r2 in rooms)
                {
                    for (int x2 = r2.X; x2 < r2.Right; x2 += 2)
                    {
                        for (int y2 = r2.Y; y2 < r2.Bottom; y2 += 2)
                        {
                            if (r2.Contains(x2, y2))
                            {
                                __instance.map.GetLayer("Back").Tiles[x2, y2] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                            }
                            if (r2.Contains(x2 + 1, y2))
                            {
                                __instance.map.GetLayer("Back").Tiles[x2 + 1, y2] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                            }
                            if (r2.Contains(x2, y2 + 1))
                            {
                                __instance.map.GetLayer("Back").Tiles[x2, y2 + 1] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                            }
                            if (r2.Contains(x2 + 1, y2 + 1))
                            {
                                __instance.map.GetLayer("Back").Tiles[x2 + 1, y2 + 1] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                            }
                        }
                    }
                }
            }
            else
            {
                if (rooms.Count <= whichRoom)
                {
                    return;
                }

                Rectangle r = rooms[whichRoom];
                for (int x = r.X; x < r.Right; x += 2)
                {
                    for (int y = r.Y; y < r.Bottom; y += 2)
                    {
                        if (r.Contains(x, y))
                        {
                            __instance.map.GetLayer("Back").Tiles[x, y] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                        }
                        if (r.Contains(x + 1, y))
                        {
                            __instance.map.GetLayer("Back").Tiles[x + 1, y] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                        }
                        if (r.Contains(x, y + 1))
                        {
                            __instance.map.GetLayer("Back").Tiles[x, y + 1] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                        }
                        if (r.Contains(x + 1, y + 1))
                        {
                            __instance.map.GetLayer("Back").Tiles[x + 1, y + 1] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                        }
                    }
                }
            }
        }
    }
}