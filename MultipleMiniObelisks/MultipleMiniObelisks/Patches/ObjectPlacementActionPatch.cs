﻿using Harmony;
using StardewValley;
using System.Reflection;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using System.Linq;

namespace MultipleMiniObelisks.Patches
{
    [HarmonyPatch]
    public class ObjectPlacementActionPatch
    {
        private static IMonitor monitor = ModResources.GetMonitor();

        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction));
        }

        internal static bool Prefix(Object __instance, ref bool __result, GameLocation location, int x, int y, Farmer who = null)
        {
			// 238 is Mini-Obelisks
			if (__instance.ParentSheetIndex == 238)
            {
                Vector2 placementTile = new Vector2(x / 64, y / 64);

                if (!(location is Farm))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:OnlyPlaceOnFarm"));
				}

                Object toPlace = (Object)__instance.getOne();
                location.objects.Add(placementTile, toPlace);
                toPlace.initializeLightSource(placementTile);
                location.playSound("woodyStep");

                __result = true;
                return false;
			}

            return true;
        }
    }
}
