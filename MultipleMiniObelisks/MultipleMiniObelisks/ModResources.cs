using StardewModdingAPI;
using System.Text.RegularExpressions;

namespace MultipleMiniObelisks
{
    public static class ModResources
    {
        private static IMonitor monitor;
        private static ModConfig config;

        public static void LoadMonitor(IMonitor iMonitor)
        {
            monitor = iMonitor;
        }

        public static IMonitor GetMonitor()
        {
            return monitor;
        }

        public static void LoadConfig(ModConfig modConfig)
        {
            config = modConfig;
        }

        public static ModConfig GetConfig()
        {
            return config;
        }
    }
}
