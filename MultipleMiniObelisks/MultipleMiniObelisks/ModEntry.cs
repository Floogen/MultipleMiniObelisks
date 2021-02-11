using Harmony;
using StardewModdingAPI;
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
        }
    }
}
