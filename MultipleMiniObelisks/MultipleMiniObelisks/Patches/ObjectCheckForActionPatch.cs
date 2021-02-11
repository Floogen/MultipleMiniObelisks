using Harmony;
using StardewValley;
using System.Reflection;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using System.Linq;
using System.Collections.Generic;
using MultipleMiniObelisks.UI;
using StardewValley.Locations;
using StardewValley.Buildings;
using Newtonsoft.Json;

namespace MultipleMiniObelisks.Patches
{
    [HarmonyPatch]
    public class ObjectCheckForActionPatch
    {
        private static IMonitor monitor = ModEntry.monitor;

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

                Dictionary<Object, GameLocation> miniObeliskToLocation = new Dictionary<Object, GameLocation>();
                foreach (GameLocation location in Game1.locations)
                {
                    if (location.numberOfObjectsOfType(238, true) > 0)
                    {
                        foreach (var tileToObject in location.Objects.Pairs.Where(p => p.Value.ParentSheetIndex == 238 && p.Value.bigCraftable))
                        {
                            miniObeliskToLocation.Add(tileToObject.Value, location);
                        }
                    }

                    if (location is BuildableGameLocation)
                    {
                        foreach (Building b2 in (location as BuildableGameLocation).buildings)
                        {
                            GameLocation indoorLocation = b2.indoors.Value;
                            if (indoorLocation is null)
                            {
                                continue;
                            }

                            if (indoorLocation.numberOfObjectsOfType(238, true) > 0)
                            {
                                foreach (var tileToObject in indoorLocation.Objects.Pairs.Where(p => p.Value.ParentSheetIndex == 238 && p.Value.bigCraftable))
                                {
                                    miniObeliskToLocation.Add(tileToObject.Value, indoorLocation);
                                }
                            }
                        }
                    }
                }
                Game1.activeClickableMenu = new TeleportMenu(__instance, miniObeliskToLocation);

                __result = true;
                return false;
            }

            return true;
        }
    }
}
