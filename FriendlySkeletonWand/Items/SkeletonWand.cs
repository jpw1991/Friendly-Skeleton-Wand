using BepInEx;
using BepInEx.Configuration;
using FriendlySkeletonWand.Minions;
using JetBrains.Annotations;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FriendlySkeletonWand.Minions.SkeletonMinion;
using static System.Collections.Specialized.BitVector32;

namespace FriendlySkeletonWand
{
    internal class SkeletonWand : Wand
    {
        public const string SkeletonWarriorPrefabName = "ChebGonaz_SkeletonWarrior";
        public const string SkeletonWarriorTier2PrefabName = "ChebGonaz_SkeletonWarriorTier2";
        public const string SkeletonWarriorTier3PrefabName = "ChebGonaz_SkeletonWarriorTier3";

        public const string SkeletonArcherPrefabName = "ChebGonaz_SkeletonArcher";
        public const string SkeletonArcherTier2PrefabName = "ChebGonaz_SkeletonArcherTier2";
        public const string SkeletonArcherTier3PrefabName = "ChebGonaz_SkeletonArcherTier3";

        public const string SkeletonMagePrefabName = "ChebGonaz_SkeletonMage";
        public const string SkeletonMageTier2PrefabName = "ChebGonaz_SkeletonMageTier2";
        public const string SkeletonMageTier3PrefabName = "ChebGonaz_SkeletonMageTier3";

        public const string PoisonSkeletonPrefabName = "ChebGonaz_PoisonSkeleton";

        public static ConfigEntry<CraftingTable> craftingStationRequired;
        public static ConfigEntry<int> craftingStationLevel;
        public static ConfigEntry<string> craftingCost;

        public static ConfigEntry<bool> skeletonsAllowed;

        public static ConfigEntry<int> maxSkeletons;

        public static ConfigEntry<float> skeletonBaseHealth;
        public static ConfigEntry<float> skeletonHealthMultiplier;
        public static ConfigEntry<float> skeletonSetFollowRange;

        private static ConfigEntry<float> necromancyLevelIncrease;

        public static ConfigEntry<int> boneFragmentsRequiredConfig;
        public static ConfigEntry<int> boneFragmentsDroppedAmountMin;
        public static ConfigEntry<int> boneFragmentsDroppedAmountMax;

        public static ConfigEntry<int> armorLeatherScrapsRequiredConfig;
        public static ConfigEntry<int> armorBronzeRequiredConfig;
        public static ConfigEntry<int> armorIronRequiredConfig;
        public static ConfigEntry<int> armorBlackIronRequiredConfig;
        public static ConfigEntry<int> surtlingCoresRequiredConfig;

        public static ConfigEntry<int> poisonSkeletonLevelRequirementConfig;
        public static ConfigEntry<float> poisonSkeletonBaseHealth;
        public static ConfigEntry<int> poisonSkeletonGuckRequiredConfig;
        public static ConfigEntry<float> poisonSkeletonNecromancyLevelIncrease;
        public static ConfigEntry<float> skeletonArmorValueMultiplier;

        public override string ItemName { get { return "ChebGonaz_SkeletonWand"; } }
        public override string PrefabName { get { return "ChebGonaz_SkeletonWand.prefab"; } }

        public override void CreateConfigs(BaseUnityPlugin plugin)
        {
            base.CreateConfigs(plugin);

            skeletonSetFollowRange = plugin.Config.Bind("SkeletonWand (Client)", "SkeletonCommandRange",
            20f, new ConfigDescription("The distance which nearby skeletons will hear your commands."));

            allowed = plugin.Config.Bind("SkeletonWand (Server Synced)", "SkeletonWandAllowed",
                true, new ConfigDescription("Whether crafting a Skeleton Wand is allowed or not.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            craftingStationRequired = plugin.Config.Bind("SkeletonWand (Server Synced)", "Skeleton Wand Crafting Station",
                CraftingTable.Workbench, new ConfigDescription("Crafting station where Skeleton Wand is available", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            craftingStationLevel = plugin.Config.Bind("SkeletonWand (Server Synced)", "Skeleton Wand Crafting Station Level",
                1, new ConfigDescription("Crafting station level required to craft Skeleton Wand", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            craftingCost = plugin.Config.Bind("SkeletonWand (Server Synced)", "Skeleton Wand Crafting Costs",
                "Wood:5", new ConfigDescription("Materials needed to craft Skeleton Wand", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            skeletonsAllowed = plugin.Config.Bind("SkeletonWand (Server Synced)", "SkeletonsAllowed",
                true, new ConfigDescription("If false, skeletons aren't loaded at all and can't be summoned.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            skeletonBaseHealth = plugin.Config.Bind("SkeletonWand (Server Synced)", "SkeletonBaseHealth",
                20f, new ConfigDescription("HP = BaseHealth + NecromancyLevel * HealthMultiplier", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            skeletonHealthMultiplier = plugin.Config.Bind("SkeletonWand (Server Synced)", "SkeletonHealthMultiplier",
                2.5f, new ConfigDescription("HP = BaseHealth + NecromancyLevel * HealthMultiplier", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            boneFragmentsRequiredConfig = plugin.Config.Bind("SkeletonWand (Server Synced)", "BoneFragmentsRequired",
                3, new ConfigDescription("The amount of Bone Fragments required to craft a skeleton.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            boneFragmentsDroppedAmountMin = plugin.Config.Bind("SkeletonWand (Server Synced)", "BoneFragmentsDroppedAmountMin",
                1, new ConfigDescription("The minimum amount of bones dropped by creatures.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            boneFragmentsDroppedAmountMax = plugin.Config.Bind("SkeletonWand (Server Synced)", "BoneFragmentsDroppedAmountMax",
                3, new ConfigDescription("The maximum amount of bones dropped by creautres.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            necromancyLevelIncrease = plugin.Config.Bind("SkeletonWand (Server Synced)", "NecromancyLevelIncrease",
                1f, new ConfigDescription("How much crafting a skeleton contributes to your Necromancy level increasing.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            maxSkeletons = plugin.Config.Bind("SkeletonWand (Server Synced)", "MaximumSkeletons",
                0, new ConfigDescription("The maximum amount of skeletons that can be made (0 = unlimited).", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            armorLeatherScrapsRequiredConfig = plugin.Config.Bind("SkeletonWand (Server Synced)", "ArmoredSkeletonLeatherScrapsRequired",
                5, new ConfigDescription("The amount of LeatherScraps required to craft a skeleton in leather armor.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            armorBronzeRequiredConfig = plugin.Config.Bind("SkeletonWand (Server Synced)", "ArmoredSkeletonBronzeRequired",
                1, new ConfigDescription("The amount of Bronze required to craft a skeleton in bronze armor.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            armorIronRequiredConfig = plugin.Config.Bind("SkeletonWand (Server Synced)", "ArmoredSkeletonIronRequired",
                1, new ConfigDescription("The amount of Iron required to craft a skeleton in iron armor.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            surtlingCoresRequiredConfig = plugin.Config.Bind("SkeletonWand (Server Synced)", "SkeletonMageSurtlingCoresRequired",
                1, new ConfigDescription("The amount of surtling cores required to craft a skeleton mage.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            armorBlackIronRequiredConfig = plugin.Config.Bind("SkeletonWand (Server Synced)", "ArmoredSkeletonBlackIronRequired",
                1, new ConfigDescription("The amount of Black Metal required to craft a skeleton in black iron armor.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            poisonSkeletonBaseHealth = plugin.Config.Bind("SkeletonWand (Server Synced)", "PoisonSkeletonBaseHealth",
                100f, new ConfigDescription("HP = BaseHealth + NecromancyLevel * HealthMultiplier", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            poisonSkeletonLevelRequirementConfig = plugin.Config.Bind("SkeletonWand (Server Synced)", "PoisonSkeletonLevelRequired",
                50, new ConfigDescription("The Necromancy level needed to summon a Poison Skeleton.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            poisonSkeletonGuckRequiredConfig = plugin.Config.Bind("SkeletonWand (Server Synced)", "PoisonSkeletonGuckRequired",
                1, new ConfigDescription("The amount of Guck required to craft a Poison Skeleton.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            poisonSkeletonNecromancyLevelIncrease = plugin.Config.Bind("SkeletonWand (Server Synced)", "PoisonSkeletonNecromancyLevelIncrease",
                3f, new ConfigDescription("How much crafting a Poison Skeleton contributes to your Necromancy level increasing.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            skeletonArmorValueMultiplier = plugin.Config.Bind("SkeletonWand (Server Synced)", "SkeletonArmorValueMultiplier",
                1f, new ConfigDescription("If you find the armor value for skeletons to be too low, you can multiply it here. By default, a skeleton wearing iron armor will have an armor value of 42 (14+14+14). A multiplier of 1.5 will cause this armor value to increase to 63.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
        }

        public override void CreateButtons()
        {
            // call the base to add the basic generic buttons -> create, attack, follow, wait, etc.
            base.CreateButtons();

            // add any extra buttons
        }

        public override CustomItem GetCustomItemFromPrefab(GameObject prefab)
        {
            ItemConfig config = new ItemConfig();
            config.Name = "$item_friendlyskeletonwand";
            config.Description = "$item_friendlyskeletonwand_desc";

            if (allowed.Value)
            {
                // set recipe requirements
                this.SetRecipeReqs(
                    config,
                    craftingCost,
                    craftingStationRequired,
                    craftingStationLevel
                );
            } else
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
            // make sure the set effect is applied
            customItem.ItemDrop.m_itemData.m_shared.m_setStatusEffect = BasePlugin.setEffectNecromancyArmor;

            return customItem;
        }

        public override KeyHintConfig GetKeyHint()
        {
            List<ButtonConfig> buttonConfigs = new List<ButtonConfig>();

            if (CreateMinionButton != null) buttonConfigs.Add(CreateMinionButton);
            if (CreateArcherMinionButton != null) buttonConfigs.Add(CreateArcherMinionButton);
            if (FollowButton != null) buttonConfigs.Add(FollowButton);
            if (WaitButton != null) buttonConfigs.Add(WaitButton);
            if (TeleportButton != null) buttonConfigs.Add(TeleportButton);
            if (AttackTargetButton != null) buttonConfigs.Add(AttackTargetButton);

            return new KeyHintConfig
            {
                Item = ItemName,
                ButtonConfigs = buttonConfigs.ToArray()
            };
        }

        public override bool HandleInputs()
        {
            if (MessageHud.instance != null
                    && Player.m_localPlayer != null
                    && Player.m_localPlayer.GetInventory().GetEquipedtems().Find(
                        equippedItem => equippedItem.TokenName().Equals("$item_friendlyskeletonwand")
                        ) != null
                    )
            {
                ExtraResourceConsumptionUnlocked =
                    UnlockExtraResourceConsumptionButton == null
                    || ZInput.GetButton(UnlockExtraResourceConsumptionButton.Name);

                if (CreateMinionButton != null && ZInput.GetButton(CreateMinionButton.Name))
                {
                    SpawnFriendlySkeleton(Player.m_localPlayer,
                        boneFragmentsRequiredConfig.Value,
                        necromancyLevelIncrease.Value,
                        false
                        );
                    return true;
                }
                else if (CreateArcherMinionButton != null && ZInput.GetButton(CreateArcherMinionButton.Name))
                {
                    SpawnFriendlySkeleton(Player.m_localPlayer,
                        boneFragmentsRequiredConfig.Value,
                        necromancyLevelIncrease.Value,
                        true
                        );
                    return true;
                }
                else if (FollowButton != null && ZInput.GetButton(FollowButton.Name))
                {
                    MakeNearbyMinionsFollow(Player.m_localPlayer, skeletonSetFollowRange.Value, true);
                    return true;
                }
                else if (WaitButton != null && ZInput.GetButton(WaitButton.Name))
                {
                    MakeNearbyMinionsFollow(Player.m_localPlayer, skeletonSetFollowRange.Value, false);
                    return true;
                }
                else if (TeleportButton != null && ZInput.GetButton(TeleportButton.Name))
                {
                    TeleportFollowingMinionsToPlayer(Player.m_localPlayer);
                    return true;
                }
                else if (AttackTargetButton != null && ZInput.GetButton(AttackTargetButton.Name))
                {
                    MakeFollowingMinionsAttackTarget(Player.m_localPlayer);
                    return true;
                }
            }
            return false;
        }

        public int CountActiveSkeletonMinions()
        {
            int result = 0;
            // based off BaseAI.FindClosestCreature
            List<Character> allCharacters = Character.GetAllCharacters();
            List<Tuple<int, Character>> minionsFound = new List<Tuple<int, Character>>();

            foreach (Character item in allCharacters)
            {
                if (item.IsDead())
                {
                    continue;
                }

                SkeletonMinion minion = item.GetComponent<SkeletonMinion>();
                if (minion != null)
                {
                    minionsFound.Add(new Tuple<int, Character>(minion.createdOrder, item));
                }
            }

            // reverse so that we get newest first, oldest last. This means
            // when we kill off surplus, the oldest things are getting killed
            // not the newest things
            minionsFound = minionsFound.OrderByDescending((arg) => arg.Item1).ToList();

            for (int i = 0; i < minionsFound.Count; i++)
            {
                // kill off surplus
                if (result >= maxSkeletons.Value - 1)
                {
                    Tuple<int, Character> tuple = minionsFound[i];
                    tuple.Item2.SetHealth(0);
                    continue;
                }

                result++;
            }

            return result;
        }

        private bool ConsumeGuckIfAvailable(Player player)
        {
            if (!ExtraResourceConsumptionUnlocked) return false;

            // return true if guck is available and got consumed
            int guckInInventory = player.GetInventory().CountItems("$item_guck");
            if (guckInInventory >= poisonSkeletonGuckRequiredConfig.Value)
            {
                player.GetInventory().RemoveItem("$item_guck", poisonSkeletonGuckRequiredConfig.Value);
                return true;
            }
            return false;
        }

        private string PrefabFromNecromancyLevel(float necromancyLevel, SkeletonType skeletonType)
        {
            string result = "";
            switch (skeletonType)
            {
                case SkeletonType.Archer:
                    if (necromancyLevel >= 75)
                    {
                        result = SkeletonArcherTier3PrefabName;
                    }
                    else if (necromancyLevel >= 35)
                    {
                        result = SkeletonArcherTier2PrefabName;
                    }
                    else
                    {
                        result = SkeletonArcherPrefabName;
                    }
                    break;
                case SkeletonType.Mage:
                    if (necromancyLevel >= 75)
                    {
                        result = SkeletonMageTier3PrefabName;
                    }
                    else if (necromancyLevel >= 35)
                    {
                        result = SkeletonMageTier2PrefabName;
                    }
                    else
                    {
                        result = SkeletonMagePrefabName;
                    }
                    break;
                case SkeletonType.Poison:
                    result = PoisonSkeletonPrefabName;
                    break;
                default:
                    if (necromancyLevel >= 60)
                    {
                        result = SkeletonWarriorTier3PrefabName;
                    }
                    else if (necromancyLevel >= 30)
                    {
                        result = SkeletonWarriorTier2PrefabName;
                    }
                    else
                    {
                        result = SkeletonWarriorPrefabName;
                    }
                    break;
            }
            return result;
        }

        private void InstantiateSkeleton(Player player, int quality, float playerNecromancyLevel, SkeletonType skeletonType, bool leatherArmor, bool bronzeArmor, bool ironArmor, bool blackIronArmor)
        {
            string prefabName = PrefabFromNecromancyLevel(playerNecromancyLevel, skeletonType);
            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (!prefab)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"{prefabName} does not exist");
                Jotunn.Logger.LogError($"InstantiateSkeleton: spawning {prefabName} failed");
            }

            GameObject spawnedChar = GameObject.Instantiate(prefab, player.transform.position + player.transform.forward * 2f + Vector3.up, Quaternion.identity);
            Character character = spawnedChar.GetComponent<Character>();
            character.SetLevel(quality);

            SkeletonMinion minion = skeletonType == SkeletonType.Poison 
                ? spawnedChar.AddComponent<PoisonSkeletonMinion>() 
                : spawnedChar.AddComponent<SkeletonMinion>();
            minion.ScaleEquipment(playerNecromancyLevel, skeletonType, leatherArmor, bronzeArmor, ironArmor, blackIronArmor);
            minion.ScaleStats(playerNecromancyLevel);

            if (followByDefault.Value)
            {
                spawnedChar.GetComponent<MonsterAI>().SetFollowTarget(player.gameObject);
            }

            try
            {
                spawnedChar.GetComponent<ZNetView>().GetZDO().SetOwner(
                    ZDOMan.instance.GetMyID()
                    );
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError($"Failed to set minion owner to player: {e}");
            }

            player.RaiseSkill(SkillManager.Instance.GetSkill(BasePlugin.necromancySkillIdentifier).m_skill, necromancyLevelIncrease.Value);
        }

        public void SpawnFriendlySkeleton(Player player, int boneFragmentsRequired, float necromancyLevelIncrease, bool archer)
        {
            if (!skeletonsAllowed.Value) return;

            // check player inventory for requirements
            if (boneFragmentsRequired > 0)
            {
                int boneFragmentsInInventory = player.GetInventory().CountItems("$item_bonefragments");

                Jotunn.Logger.LogInfo($"BoneFragments in inventory: {boneFragmentsInInventory}");
                if (boneFragmentsInInventory < boneFragmentsRequired)
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "$friendlyskeletonwand_notenoughbones");
                    return;
                }

                // consume the fragments
                player.GetInventory().RemoveItem("$item_bonefragments", boneFragmentsRequired);
            }

            bool createArmoredLeather = false;
            if (ExtraResourceConsumptionUnlocked && armorLeatherScrapsRequiredConfig.Value > 0)
            {
                int leatherScrapsInInventory = player.GetInventory().CountItems("$item_leatherscraps");
                Jotunn.Logger.LogInfo($"LeatherScraps in inventory: {leatherScrapsInInventory}");
                if (leatherScrapsInInventory >= armorLeatherScrapsRequiredConfig.Value)
                {
                    createArmoredLeather = true;
                    player.GetInventory().RemoveItem("$item_leatherscraps", armorLeatherScrapsRequiredConfig.Value);
                }
                else
                {
                    // no leather scraps? Try some deer hide
                    int deerHideInInventory = player.GetInventory().CountItems("$item_deerhide");
                    Jotunn.Logger.LogInfo($"DeerHide in inventory: {deerHideInInventory}");
                    if (deerHideInInventory >= armorLeatherScrapsRequiredConfig.Value)
                    {
                        createArmoredLeather = true;
                        player.GetInventory().RemoveItem("$item_deerhide", armorLeatherScrapsRequiredConfig.Value);
                    }
                }
            }

            bool createArmoredBronze = false;
            if (ExtraResourceConsumptionUnlocked && !createArmoredLeather && armorBronzeRequiredConfig.Value > 0)
            {
                int bronzeInInventory = player.GetInventory().CountItems("$item_bronze");
                Jotunn.Logger.LogInfo($"Bronze in inventory: {bronzeInInventory}");
                if (bronzeInInventory >= armorBronzeRequiredConfig.Value)
                {
                    createArmoredBronze = true;
                    player.GetInventory().RemoveItem("$item_bronze", armorBronzeRequiredConfig.Value);
                }
            }

            bool createArmoredIron = false;
            if (ExtraResourceConsumptionUnlocked && !createArmoredLeather && !createArmoredBronze && armorIronRequiredConfig.Value > 0)
            {
                int ironInInventory = player.GetInventory().CountItems("$item_iron");
                Jotunn.Logger.LogInfo($"Iron in inventory: {ironInInventory}");
                if (ironInInventory >= armorIronRequiredConfig.Value)
                {
                    createArmoredIron = true;
                    player.GetInventory().RemoveItem("$item_iron", armorIronRequiredConfig.Value);
                }
            }

            bool createArmoredBlackIron = false;
            if (ExtraResourceConsumptionUnlocked 
                && !createArmoredLeather 
                && !createArmoredBronze 
                && !createArmoredIron
                && armorBlackIronRequiredConfig.Value > 0)
            {
                int blackIronInInventory = player.GetInventory().CountItems("$item_blackmetal");
                Jotunn.Logger.LogInfo($"Black metal in inventory: {blackIronInInventory}");
                if (blackIronInInventory >= armorBlackIronRequiredConfig.Value)
                {
                    createArmoredBlackIron = true;
                    player.GetInventory().RemoveItem("$item_blackmetal", armorBlackIronRequiredConfig.Value);
                }
            }

            bool createMage = false;
            if (ExtraResourceConsumptionUnlocked
                && !archer
                && surtlingCoresRequiredConfig.Value > 0)
            {
                int surtlingCoresInInventory = player.GetInventory().CountItems("$item_surtlingcore");
                Jotunn.Logger.LogInfo($"Surtling cores in inventory: {surtlingCoresInInventory}");
                if (surtlingCoresInInventory >= surtlingCoresRequiredConfig.Value)
                {
                    createMage = true;
                    player.GetInventory().RemoveItem("$item_surtlingcore", surtlingCoresRequiredConfig.Value);
                }
            }

            // if players have decided to foolishly restrict their power and
            // create a *cough* LIMIT *spits*... check that here
            if (maxSkeletons.Value > 0)
            {
                // re-count the current active skeletons
                int activeSkeletons = CountActiveSkeletonMinions();
                Jotunn.Logger.LogInfo($"Skeleton count: {activeSkeletons}; maxSkeletons = {maxSkeletons.Value}");
            }

            // scale according to skill
            float playerNecromancyLevel = player.GetSkillLevel(SkillManager.Instance.GetSkill(BasePlugin.necromancySkillIdentifier).m_skill);
            Jotunn.Logger.LogInfo($"Player necromancy level: {playerNecromancyLevel}");

            int quality = 1;
            if (playerNecromancyLevel >= 70) { quality = 3; }
            else if (playerNecromancyLevel >= 35) { quality = 2; }

            SkeletonType skeletonType = SkeletonType.Warrior;
            if (archer) { skeletonType = SkeletonType.Archer; }
            else if (createMage) { skeletonType = SkeletonType.Mage; }
            else if (playerNecromancyLevel >= poisonSkeletonLevelRequirementConfig.Value
                && !createArmoredBlackIron
                && !createArmoredIron
                && !createArmoredBronze
                && !createArmoredLeather
                && ConsumeGuckIfAvailable(player))
            {
                skeletonType = SkeletonType.Poison;
            }

            InstantiateSkeleton(player, quality, playerNecromancyLevel, 
                skeletonType, 
                createArmoredLeather, createArmoredBronze, 
                createArmoredIron, createArmoredBlackIron);
        }
    }
}
