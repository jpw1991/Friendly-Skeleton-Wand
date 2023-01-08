using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace FriendlySkeletonWand
{
    internal class SkeletonCrossbow : Item
    {
        public override string ItemName { get { return "ChebGonaz_SkeletonCrossbow"; } }
        public override string PrefabName { get { return "ChebGonaz_SkeletonCrossbow.prefab"; } }

        public override CustomItem GetCustomItem(Sprite icon = null)
        {
            Jotunn.Logger.LogError("I shouldn't be called");
            return null;
        }

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
