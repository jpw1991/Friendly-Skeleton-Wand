using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace FriendlySkeletonWand
{
    internal class SkeletonBow2 : Item
    {
        public override string ItemName { get { return "ChebGonaz_SkeletonBow2"; } }
        public override string PrefabName { get { return "ChebGonaz_SkeletonBow2.prefab"; } }

        public CustomItem GetCustomItemFromPrefab(GameObject prefab)
        {
            ItemConfig config = new ItemConfig();
            config.Name = "$item_chebgonaz_skeletonbow2";
            config.Description = "$item_chebgonaz_skeletonbow2_desc";

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
