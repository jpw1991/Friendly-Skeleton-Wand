using Jotunn.Configs;
using Jotunn.Entities;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace FriendlySkeletonWand
{
    internal class SkeletonHelmetBronze : Item
    {
        // This is a copy of the HelmetBronze item that is scaled slightly larger
        // to accomodate a skeleton's large dome

        public override string ItemName { get { return "ChebGonaz_SkeletonHelmetBronze"; } }
        public override string PrefabName { get { return "ChebGonaz_SkeletonHelmetBronze.prefab"; } }

        public CustomItem GetCustomItemFromPrefab(GameObject prefab)
        {
            ItemConfig config = new ItemConfig();
            config.Name = "ChebGonaz_SkeletonHelmetBronze";
            config.Description = "ChebGonaz_SkeletonHelmetBronze";

            CustomItem customItem = new CustomItem(prefab, false, config);
            if (customItem == null)
            {
                Jotunn.Logger.LogError($"GetCustomItemFromPrefab: {PrefabName}'s CustomItem is null!");
                return null;
            }
            if (customItem.ItemPrefab == null)
            {
                Jotunn.Logger.LogError($"GetCustomItemFromPrefab: {PrefabName}'s ItemPrefab is null!");
                return null;
            }

            return customItem;
        }
    }
}
