using Jotunn.Configs;
using Jotunn.Entities;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace FriendlySkeletonWand
{
    internal class LargeCargoCrate
    {
        // large cargo crate is dropped by NecroNeckGathererMinions
        public const string PrefabName = "ChebGonaz_LargeCargoCrate.prefab";

        public static ConfigEntry<bool> allowed;
        public static ConfigEntry<int> containerHeight, containerWidth;

        public static void CreateConfigs(BaseUnityPlugin plugin)
        {

            allowed = plugin.Config.Bind("NecroNeckGatherer (Server Synced)", "LargeCargoCrateAllowed",
                true, new ConfigDescription("Disallowing this will cause the ChebGonaz_LargeCargoCrate to not be loaded. Attention: it is essential for the correct functioning of the NecroNeck Gatherer minion.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            containerHeight = plugin.Config.Bind("NecroNeckGatherer (Server Synced)", "LargeCargoCrateHeight",
                5, new ConfigDescription("Container slots = containerHeight * containerWidth = 5*5 = 25", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            containerWidth = plugin.Config.Bind("NecroNeckGatherer (Server Synced)", "LargeCargoCrateWidth",
                5, new ConfigDescription("Container slots = containerHeight * containerWidth = 5*5 = 25", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
        }
    }
}
