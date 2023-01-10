using Jotunn.Configs;
using Jotunn.Entities;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace FriendlySkeletonWand
{
    internal class SkeletonFireballLevel1 : Item
    {
        public override string ItemName { get { return "ChebGonaz_FireballLevel1"; } }
        public override string PrefabName { get { return "ChebGonaz_FireballLevel1.prefab"; } }

        public CustomItem GetCustomItemFromPrefab(GameObject prefab)
        {
            ItemConfig config = new ItemConfig();
            config.Name = "ChebGonaz_FireballLevel1";
            config.Description = "ChebGonaz_FireballLevel1";

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
