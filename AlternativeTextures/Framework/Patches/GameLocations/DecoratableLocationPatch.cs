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
            harmony.Patch(AccessTools.Method(_object, "doSetVisibleWallpaper", new[] { typeof(int), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(DoSetVisibleWallpaperPostfix)));
            harmony.Patch(AccessTools.Method(_object, "doSetVisibleFloor", new[] { typeof(int), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(DoSetVisibleFloorPostfix)));
        }

        private static void ResetWallTiles(DecoratableLocation __instance, int whichRoom)
        {
            if (!__instance.modData.ContainsKey($"AlternativeTexture.Wallpaper.Owner_{whichRoom}") || __instance.modData[$"AlternativeTexture.Wallpaper.Owner_{whichRoom}"] != AlternativeTextures.DEFAULT_OWNER)
            {
                return;
            }

            var walls = __instance.getWalls();
            MethodInfo method = __instance.GetType().GetMethod("IsFloorableOrWallpaperableTile", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (Rectangle r2 in walls)
            {
                if (whichRoom != -1 && walls.ElementAt(whichRoom) != r2)
                {
                    continue;
                }

                for (int x2 = r2.X; x2 < r2.Right; x2++)
                {
                    if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, r2.Y, "Back" })))
                    {
                        ResetWallTiles(__instance, x2, r2.Y, "Back");
                    }
                    if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, r2.Y + 1, "Back" })))
                    {
                        ResetWallTiles(__instance, x2, r2.Y + 1, "Back");
                    }

                    if (r2.Height >= 3)
                    {
                        if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, r2.Y + 2, "Buildings" })))
                        {
                            ResetWallTiles(__instance, x2, r2.Y + 2, "Buildings");
                        }
                        else if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, r2.Y + 2, "Back" })))
                        {
                            ResetWallTiles(__instance, x2, r2.Y + 2, "Back");
                        }
                    }
                }
            }
        }

        private static void ResetWallTiles(DecoratableLocation __instance, int x, int y, string layer_name)
        {
            var layer = __instance.map.GetLayer(layer_name);
            if (layer != null && x < layer.LayerWidth && y < layer.LayerHeight && layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet != null && layer.Tiles[x, y].TileSheet.Id.Contains(AlternativeTextures.TEXTURE_TOKEN_HEADER))
            {
                for (int w = 0; w < __instance.getWalls().Count(); w++)
                {
                    if (__instance.modData.ContainsKey($"AlternativeTexture.Wallpaper.Owner_{w}") && __instance.getWalls()[w].Contains(x, y))
                    {
                        if (__instance.modData[$"AlternativeTexture.Wallpaper.Owner_{w}"] == AlternativeTextures.DEFAULT_OWNER)
                        {
                            layer.Tiles[x, y] = new StaticTile(__instance.map.GetLayer(layer_name), __instance.map.GetTileSheet("walls_and_floors"), BlendMode.Alpha, 0);
                        }
                    }
                }
            }
        }

        private static void ResetFloorTiles(DecoratableLocation __instance, int whichRoom)
        {
            if (!__instance.modData.ContainsKey($"AlternativeTexture.Floor.Owner_{whichRoom}") || __instance.modData[$"AlternativeTexture.Floor.Owner_{whichRoom}"] != AlternativeTextures.DEFAULT_OWNER)
            {
                return;
            }

            var floors = __instance.getFloors();
            MethodInfo method = __instance.GetType().GetMethod("IsFloorableTile", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (Rectangle r2 in floors)
            {
                if (whichRoom != -1 && floors.ElementAt(whichRoom) != r2)
                {
                    continue;
                }

                for (int x2 = r2.X; x2 < r2.Right; x2 += 2)
                {
                    for (int y2 = r2.Y; y2 < r2.Bottom; y2 += 2)
                    {
                        if (r2.Contains(x2, y2) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, y2, "Back" })))
                        {
                            ResetFloorTiles(__instance, x2, y2, "Back");
                        }
                        if (r2.Contains(x2 + 1, y2) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x2 + 1, y2, "Back" })))
                        {
                            ResetFloorTiles(__instance, x2 + 1, y2, "Back");
                        }
                        if (r2.Contains(x2, y2 + 1) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, y2 + 1, "Back" })))
                        {
                            ResetFloorTiles(__instance, x2, y2 + 1, "Back");
                        }
                        if (r2.Contains(x2 + 1, y2 + 1) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x2 + 1, y2 + 1, "Back" })))
                        {
                            ResetFloorTiles(__instance, x2 + 1, y2 + 1, "Back");
                        }
                    }
                }
            }
        }

        private static void ResetFloorTiles(DecoratableLocation __instance, int x, int y, string layer_name)
        {
            var layer = __instance.map.GetLayer(layer_name);
            if (layer != null && x < layer.LayerWidth && y < layer.LayerHeight && layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet != null && layer.Tiles[x, y].TileSheet.Id.Contains(AlternativeTextures.TEXTURE_TOKEN_HEADER))
            {
                for (int f = 0; f < __instance.getFloors().Count(); f++)
                {
                    if (__instance.modData.ContainsKey($"AlternativeTexture.Floor.Owner_{f}") && __instance.getFloors()[f].Contains(x, y))
                    {
                        if (__instance.modData[$"AlternativeTexture.Floor.Owner_{f}"] == AlternativeTextures.DEFAULT_OWNER)
                        {
                            var currentIndex = __instance.getTileIndexAt(x, y, layer_name);
                            int x_offset = currentIndex % 2;
                            int y_offset = currentIndex / 32;
                            layer.Tiles[x, y] = new StaticTile(__instance.map.GetLayer(layer_name), __instance.map.GetTileSheet("walls_and_floors"), BlendMode.Alpha, 336 + x_offset + 16 * y_offset);
                        }
                    }
                }
            }
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

        private static void DoSetVisibleWallpaperPostfix(DecoratableLocation __instance, int whichRoom, int which)
        {
            if (!__instance.modData.ContainsKey($"AlternativeTexture.Wallpaper.Name_{whichRoom}"))
            {
                return;
            }

            var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(__instance.modData[$"AlternativeTexture.Wallpaper.Name_{whichRoom}"]);
            if (textureModel is null || !Int32.TryParse(__instance.modData[$"AlternativeTexture.Wallpaper.Variation_{whichRoom}"], out var textureVariation) || textureVariation == -1)
            {
                ResetWallTiles(__instance, whichRoom);
                return;
            }
            var textureOffset = textureVariation * textureModel.TextureHeight;

            var texturePath = _helper.Content.GetActualAssetKey($"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{textureModel.GetTokenId()}", ContentSource.GameContent);
            var tileSheet = new TileSheet($"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{textureModel.GetTokenId()}", __instance.map, texturePath, new xTile.Dimensions.Size(textureModel.Textures.First().Width, textureModel.Textures.First().Height), new xTile.Dimensions.Size(16));

            // Add the tileSheet, if it is missing from the map
            if (!__instance.map.GetLayer("Back").Map.TileSheets.Contains(tileSheet))
            {
                __instance.map.GetLayer("Back").Map.AddTileSheet(tileSheet);
            }

            // Modified vanilla logic
            List<Rectangle> rooms = __instance.getWalls();
            MethodInfo method = __instance.GetType().GetMethod("IsFloorableOrWallpaperableTile", BindingFlags.Instance | BindingFlags.NonPublic);
            if (whichRoom == -1)
            {
                foreach (Rectangle r2 in rooms)
                {
                    for (int x2 = r2.X; x2 < r2.Right; x2++)
                    {
                        if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, r2.Y, "Back" })))
                        {
                            __instance.map.GetLayer("Back").Tiles[x2, r2.Y] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                        }
                        if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, r2.Y + 1, "Back" })))
                        {
                            __instance.map.GetLayer("Back").Tiles[x2, r2.Y + 1] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset + 16);
                        }
                        if (r2.Height >= 3)
                        {
                            if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, r2.Y + 2, "Buildings" })))
                            {
                                __instance.map.GetLayer("Buildings").Tiles[x2, r2.Y + 2] = new StaticTile(__instance.map.GetLayer("Buildings"), tileSheet, BlendMode.Alpha, textureOffset + 32);
                            }
                            else if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, r2.Y + 2, "Back" })))
                            {
                                __instance.map.GetLayer("Back").Tiles[x2, r2.Y + 2] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset + 32);
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
                for (int x = r.X; x < r.Right; x++)
                {
                    if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x, r.Y, "Back" })))
                    {
                        __instance.map.GetLayer("Back").Tiles[x, r.Y] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                    }
                    if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x, r.Y + 1, "Back" })))
                    {
                        __instance.map.GetLayer("Back").Tiles[x, r.Y + 1] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset + 16);
                    }
                    if (r.Height >= 3)
                    {
                        if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x, r.Y + 2, "Buildings" })))
                        {
                            __instance.map.GetLayer("Buildings").Tiles[x, r.Y + 2] = new StaticTile(__instance.map.GetLayer("Buildings"), tileSheet, BlendMode.Alpha, textureOffset + 32);
                        }
                        else if (Convert.ToBoolean(method.Invoke(__instance, new object[] { x, r.Y + 2, "Back" })))
                        {
                            __instance.map.GetLayer("Back").Tiles[x, r.Y + 2] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset + 32);
                        }
                    }
                }
            }
        }

        private static void DoSetVisibleFloorPostfix(DecoratableLocation __instance, int whichRoom, int which)
        {
            if (!__instance.modData.ContainsKey($"AlternativeTexture.Floor.Name_{whichRoom}"))
            {
                return;
            }

            var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(__instance.modData[$"AlternativeTexture.Floor.Name_{whichRoom}"]);
            if (textureModel is null || !Int32.TryParse(__instance.modData[$"AlternativeTexture.Floor.Variation_{whichRoom}"], out var textureVariation) || textureVariation == -1)
            {
                ResetFloorTiles(__instance, whichRoom);
                return;
            }
            var textureOffset = textureVariation * (textureModel.TextureHeight + textureModel.TextureWidth);

            var texturePath = _helper.Content.GetActualAssetKey($"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{textureModel.GetTokenId()}", ContentSource.GameContent);
            var tileSheet = new TileSheet($"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{textureModel.GetTokenId()}", __instance.map, texturePath, new xTile.Dimensions.Size(textureModel.Textures.First().Width, textureModel.Textures.First().Height), new xTile.Dimensions.Size(16));

            // Add the tileSheet, if it is missing from the map
            if (!__instance.map.GetLayer("Back").Map.TileSheets.Contains(tileSheet))
            {
                __instance.map.GetLayer("Back").Map.AddTileSheet(tileSheet);
            }

            // Modified vanilla logic
            List<Rectangle> rooms = __instance.getFloors();
            MethodInfo method = __instance.GetType().GetMethod("IsFloorableTile", BindingFlags.Instance | BindingFlags.NonPublic);
            if (whichRoom == -1)
            {
                foreach (Rectangle r2 in rooms)
                {
                    for (int x2 = r2.X; x2 < r2.Right; x2 += 2)
                    {
                        for (int y2 = r2.Y; y2 < r2.Bottom; y2 += 2)
                        {
                            if (r2.Contains(x2, y2) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, y2, "Back" })))
                            {
                                __instance.map.GetLayer("Back").Tiles[x2, y2] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                            }
                            if (r2.Contains(x2 + 1, y2) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x2 + 1, y2, "Back" })))
                            {
                                __instance.map.GetLayer("Back").Tiles[x2 + 1, y2] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset + 1);
                            }
                            if (r2.Contains(x2, y2 + 1) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, y2 + 1, "Back" })))
                            {
                                __instance.map.GetLayer("Back").Tiles[x2, y2 + 1] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset + 32);
                            }
                            if (r2.Contains(x2 + 1, y2 + 1) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x2 + 1, y2 + 1, "Back" })))
                            {
                                __instance.map.GetLayer("Back").Tiles[x2 + 1, y2 + 1] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset + 33);
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
                        if (r.Contains(x, y) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x, y, "Back" })))
                        {
                            __instance.map.GetLayer("Back").Tiles[x, y] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset);
                        }
                        if (r.Contains(x + 1, y) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x + 1, y, "Back" })))
                        {
                            __instance.map.GetLayer("Back").Tiles[x + 1, y] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset + 1);
                        }
                        if (r.Contains(x, y + 1) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x, y + 1, "Back" })))
                        {
                            __instance.map.GetLayer("Back").Tiles[x, y + 1] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset + 32);
                        }
                        if (r.Contains(x + 1, y + 1) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x + 1, y + 1, "Back" })))
                        {
                            __instance.map.GetLayer("Back").Tiles[x + 1, y + 1] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, textureOffset + 33);
                        }
                    }
                }
            }
        }
    }
}