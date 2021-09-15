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
        }

        private static void DoSetVisibleFloorPostfix(DecoratableLocation __instance, int whichRoom, int which)
        {
            // TODO: See if we can set the tile's TileSheet texture manually
            _monitor.Log("HERE!", LogLevel.Warn);
            MethodInfo method = __instance.GetType().GetMethod("IsFloorableTile", BindingFlags.Instance | BindingFlags.NonPublic);

            List<Rectangle> rooms = __instance.getFloors();
            int tileSheetIndex = 336 + which % 8 * 2 + which / 8 * 32;
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
                                __instance.setMapTileIndex(x2, y2, __instance.GetFlooringIndex(tileSheetIndex, x2, y2), "Back");
                            }
                            if (r2.Contains(x2 + 1, y2) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x2 + 1, y2, "Back" })))
                            {
                                __instance.setMapTileIndex(x2 + 1, y2, __instance.GetFlooringIndex(tileSheetIndex, x2 + 1, y2), "Back");
                            }
                            if (r2.Contains(x2, y2 + 1) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x2, y2 + 1, "Back" })))
                            {
                                __instance.setMapTileIndex(x2, y2 + 1, __instance.GetFlooringIndex(tileSheetIndex, x2, y2 + 1), "Back");
                            }
                            if (r2.Contains(x2 + 1, y2 + 1) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x2 + 1, y2 + 1, "Back" })))
                            {
                                __instance.setMapTileIndex(x2 + 1, y2 + 1, __instance.GetFlooringIndex(tileSheetIndex, x2 + 1, y2 + 1), "Back");
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
                            __instance.setMapTileIndex(x, y, __instance.GetFlooringIndex(tileSheetIndex, x, y), "Back");
                        }
                        if (r.Contains(x + 1, y) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x + 1, y, "Back" })))
                        {
                            __instance.setMapTileIndex(x + 1, y, __instance.GetFlooringIndex(tileSheetIndex, x + 1, y), "Back");
                        }
                        if (r.Contains(x, y + 1) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x, y + 1, "Back" })))
                        {
                            __instance.setMapTileIndex(x, y + 1, __instance.GetFlooringIndex(tileSheetIndex, x, y + 1), "Back");
                        }
                        if (r.Contains(x + 1, y + 1) && Convert.ToBoolean(method.Invoke(__instance, new object[] { x + 1, y + 1, "Back" })))
                        {
                            __instance.setMapTileIndex(x + 1, y + 1, __instance.GetFlooringIndex(tileSheetIndex, x + 1, y + 1), "Back");
                        }
                    }
                }
            }
            return;
        }
    }
}