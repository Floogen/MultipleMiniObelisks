﻿using Harmony;
using StardewValley;
using System.Reflection;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using System.Linq;
using System.Collections.Generic;
using MultipleMiniObelisks.UI;

namespace MultipleMiniObelisks.Patches
{
    [HarmonyPatch]
    public class ObjectCheckForActionPatch
    {
        private static IMonitor monitor = ModResources.GetMonitor();

        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.checkForAction));
        }

        internal static bool Prefix(Object __instance, ref bool __result, Farmer who, bool justCheckingForActivity = false)
        {
            // 238 is Mini-Obelisks
            if (__instance.ParentSheetIndex == 238)
            {
				if (justCheckingForActivity)
				{
					return false;
				}

                List<Object> miniObelisks = new List<Object>();
                foreach (GameLocation location in Game1.locations.Where(l => l.numberOfObjectsOfType(238, true) > 0))
                {
                    foreach (var tileToObject in location.Objects.Pairs.Where(p => p.Value.ParentSheetIndex == 238))
                    {
                        miniObelisks.Add(tileToObject.Value);
                    }
                }
                Game1.activeClickableMenu = new TeleportMenu(miniObelisks);

                __result = true;
                return false;
			}

            return true;
        }
    }
}
