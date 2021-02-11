using Harmony;
using Microsoft.Xna.Framework;
using MultipleMiniObelisks.Multiplayer;
using MultipleMiniObelisks.Objects;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MultipleMiniObelisks
{
    public class ModEntry : Mod
    {
        // Monitor, Helper, Config
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static ModConfig config;
        internal static IManifest manifest;

        // ModData related
        internal static string ObeliskLocationsKey;
        internal static string ObeliskNameDataKey;

        public override void Entry(IModHelper modHelper)
        {
            // Load the monitor, helper and config
            monitor = this.Monitor;
            helper = modHelper;
            config = helper.ReadConfig<ModConfig>();
            manifest = this.ModManifest;

            // Set the ModData keys we'll be using
            ObeliskLocationsKey = $"{this.ModManifest.UniqueID}/obelisk-locations";
            ObeliskNameDataKey = $"{this.ModManifest.UniqueID}/destination-name";

            // Load our Harmony patches
            try
            {
                var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patch: {e}", LogLevel.Error);
                return;
            }
            // Hook into ObjectListChanged to catch when Mini-Obelisks are placed / removed
            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;

            // Hook into save related events
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                return;
            }

            if (!Game1.player.modData.ContainsKey(ObeliskLocationsKey))
            {
                Game1.player.modData.Add(ObeliskLocationsKey, JsonConvert.SerializeObject(new List<MiniObelisk>()));
            }

            List<MiniObelisk> miniObelisks = JsonConvert.DeserializeObject<List<MiniObelisk>>(Game1.player.modData[ObeliskLocationsKey]);

            // Add any new obelisks
            foreach (var tileToObelisk in e.Added.Where(o => o.Value.ParentSheetIndex == 238 && o.Value.bigCraftable))
            {
                StardewValley.Object obelisk = tileToObelisk.Value;
                if (!obelisk.modData.ContainsKey(ObeliskNameDataKey))
                {
                    obelisk.modData.Add(ObeliskNameDataKey, String.Empty);
                }

                miniObelisks.Add(new MiniObelisk(e.Location.NameOrUniqueName, tileToObelisk.Key, obelisk.modData[ObeliskNameDataKey]));
            }

            // Remove any removed obelisks
            foreach (var tileToObelisk in e.Removed.Where(o => o.Value.ParentSheetIndex == 238 && o.Value.bigCraftable))
            {
                miniObelisks = miniObelisks.Where(o => !(o.LocationName == e.Location.NameOrUniqueName && o.Tile == tileToObelisk.Key)).ToList();
            }

            Game1.player.modData[ObeliskLocationsKey] = JsonConvert.SerializeObject(miniObelisks);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            UpdateObeliskCache();
        }

        internal static void UpdateObeliskCache()
        {
            if (!Context.IsMainPlayer)
            {
                return;
            }

            if (!Game1.player.modData.ContainsKey(ObeliskLocationsKey))
            {
                Game1.player.modData.Add(ObeliskLocationsKey, String.Empty);
            }

            List<MiniObelisk> miniObelisks = new List<MiniObelisk>();
            foreach (GameLocation location in Game1.locations)
            {
                if (location.numberOfObjectsOfType(238, true) > 0)
                {
                    foreach (var tileToObject in location.Objects.Pairs.Where(p => p.Value.ParentSheetIndex == 238 && p.Value.bigCraftable))
                    {
                        StardewValley.Object obelisk = tileToObject.Value;
                        if (!obelisk.modData.ContainsKey(ObeliskNameDataKey))
                        {
                            obelisk.modData.Add(ObeliskNameDataKey, String.Empty);
                        }

                        miniObelisks.Add(new MiniObelisk(location.NameOrUniqueName, tileToObject.Key, obelisk.modData[ObeliskNameDataKey]));
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
                                StardewValley.Object obelisk = tileToObject.Value;
                                if (!obelisk.modData.ContainsKey(ObeliskNameDataKey))
                                {
                                    obelisk.modData.Add(ObeliskNameDataKey, String.Empty);
                                }

                                miniObelisks.Add(new MiniObelisk(indoorLocation.NameOrUniqueName, tileToObject.Key, obelisk.modData[ObeliskNameDataKey]));
                            }
                        }
                    }
                }
            }

            monitor.Log(JsonConvert.SerializeObject(miniObelisks), LogLevel.Trace);
            Game1.player.modData[ObeliskLocationsKey] = JsonConvert.SerializeObject(miniObelisks);
        }
        {
        }
    }
}
