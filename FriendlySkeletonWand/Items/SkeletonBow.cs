using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace FriendlySkeletonWand
{
    internal class SkeletonBow : Item
    {
        public override string ItemName { get { return "ChebGonaz_SkeletonBow"; } }
        public override string PrefabName { get { return "ChebGonaz_SkeletonBow.prefab"; } }

        public CustomItem GetCustomItemFromPrefab(GameObject prefab)
        {
            ItemConfig config = new ItemConfig();
            config.Name = "$item_chebgonaz_skeletonbow";
            config.Description = "$item_chebgonaz_skeletonbow_desc";

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
