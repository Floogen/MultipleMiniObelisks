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
        public override void Entry(IModHelper helper)
        {
            // Load the monitor
            ModResources.LoadMonitor(this.Monitor);

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
