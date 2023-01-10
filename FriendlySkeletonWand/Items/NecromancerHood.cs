using Jotunn.Configs;
using Jotunn.Entities;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using Jotunn.Managers;

namespace FriendlySkeletonWand
{
    internal class NecromancerHood : Item
    {
        public override string ItemName { get { return "ChebGonaz_NecromancerHood"; } }
        public override string PrefabName { get { return "ChebGonaz_NecromancerHood.prefab"; } }

        public static ConfigEntry<int> necromancySkillBonus;

        public static ConfigEntry<CraftingTable> craftingStationRequired;
        public static ConfigEntry<int> craftingStationLevel;
        public static ConfigEntry<string> craftingCost;

        public override void CreateConfigs(BaseUnityPlugin plugin)
        {
            base.CreateConfigs(plugin);

            allowed = plugin.Config.Bind("NecromancerHood (Server Synced)", "NecromancerHoodAllowed",
                true, new ConfigDescription("Whether crafting a Necromancer's Hood is allowed or not.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            craftingStationRequired = plugin.Config.Bind("NecromancerHood (Server Synced)", "Necromancer Hood Crafting Station",
                CraftingTable.Workbench, new ConfigDescription("Crafting station where Necromancer Hood is available", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            craftingStationLevel = plugin.Config.Bind("NecromancerHood (Server Synced)", "Necromancer Hood Crafting Station Level",
                1, new ConfigDescription("Crafting station level required to craft Necromancer Hood", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            craftingCost = plugin.Config.Bind("NecromancerHood (Server Synced)", "Necromancer Hood Crafting Costs",
                "WitheredBone:2,TrollHide:5", new ConfigDescription("Materials needed to craft Necromancer Hood", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            necromancySkillBonus = plugin.Config.Bind("NecromancerHood (Server Synced)", "NecromancerHoodSkillBonus",
                10, new ConfigDescription("How much wearing the item should raise the Necromancy level (set to 0 to have no set effect at all).", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
        }

        public CustomItem GetCustomItemFromPrefab(GameObject prefab)
        {
            ItemConfig config = new ItemConfig();
            config.Name = "$item_chebgonaz_necromancerhood";
            config.Description = "$item_chebgonaz_necromancerhood_desc";

            if (allowed.Value)
            {
                // set recipe requirements
                this.SetRecipeReqs(
                    config,
                    craftingCost,
                    craftingStationRequired,
                    craftingStationLevel
                );
            }
            else
            {
                config.Enabled = false;
            }

            CustomItem customItem = new CustomItem(prefab, false, config);
            if (customItem == null)
            {
                Jotunn.Logger.LogError($"AddCustomItems: {PrefabName}'s CustomItem is null!");
                return null;
            }
            if (customItem.ItemPrefab == null)
            {
                Jotunn.Logger.LogError($"AddCustomItems: {PrefabName}'s ItemPrefab is null!");
                return null;
            }
            // make sure the set effect is applied or removed according
            // to config values
            customItem.ItemDrop.m_itemData.m_shared.m_setStatusEffect =
                necromancySkillBonus.Value > 0 ?
                BasePlugin.setEffectNecromancyArmor2 : null;
            customItem.ItemDrop.m_itemData.m_shared.m_equipStatusEffect =
                necromancySkillBonus.Value > 0 ?
                BasePlugin.setEffectNecromancyArmor2 : null;

            return customItem;
        }
    }
}
