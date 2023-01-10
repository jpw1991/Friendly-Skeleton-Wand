using BepInEx.Configuration;
using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using FriendlySkeletonWand.Minions;

namespace FriendlySkeletonWand
{
    internal class DraugrWand : Wand
    {
        public static ConfigEntry<int> maxDraugr;

        public static ConfigEntry<CraftingTable> craftingStationRequired;
        public static ConfigEntry<int> craftingStationLevel;
        public static ConfigEntry<string> craftingCost;

        public static ConfigEntry<bool> draugrAllowed;

        public static ConfigEntry<float> draugrBaseHealth;
        public static ConfigEntry<float> draugrHealthMultiplier;
        public static ConfigEntry<int> draugrTierOneQuality;
        public static ConfigEntry<int> draugrTierTwoQuality;
        public static ConfigEntry<int> draugrTierTwoLevelReq;
        public static ConfigEntry<int> draugrTierThreeQuality;
        public static ConfigEntry<int> draugrTierThreeLevelReq;
        public static ConfigEntry<float> draugrSetFollowRange;

        private ConfigEntry<float> necromancyLevelIncrease;

        public static ConfigEntry<int> draugrBoneFragmentsRequiredConfig;
        public static ConfigEntry<int> draugrMeatRequiredConfig;

        public override string ItemName { get { return "ChebGonaz_DraugrWand"; } }
        public override string PrefabName { get { return "ChebGonaz_DraugrWand.prefab"; } }

        public override void CreateConfigs(BaseUnityPlugin plugin)
        {
            base.CreateConfigs(plugin);

            draugrSetFollowRange = plugin.Config.Bind("DraugrWand (Client)", "DraugrCommandRange",
            10f, new ConfigDescription("The range from which nearby Draugr will hear your command.", null));

            allowed = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrWandAllowed",
                true, new ConfigDescription("Whether crafting a Draugr Wand is allowed or not.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
 
            craftingStationRequired = plugin.Config.Bind("DraugrWand (Server Synced)", "Draugr Wand Crafting Station",
                CraftingTable.Forge, new ConfigDescription("Crafting station where Draugr Wand is available", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            craftingStationLevel = plugin.Config.Bind("DraugrWand (Server Synced)", "Draugr Wand Crafting Station Level",
                1, new ConfigDescription("Crafting station level required to craft Draugr Wand", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            craftingCost = plugin.Config.Bind("DraugrWand (Server Synced)", "Draugr Wand Crafting Costs",
                "ElderBark:5,FineWood:5,Bronze:5,TrophyDraugr:1", new ConfigDescription("Materials needed to craft Draugr Wand", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
 
            draugrAllowed = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrAllowed",
                true, new ConfigDescription("If false, draugr aren't loaded at all and can't be summoned.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            draugrBaseHealth = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrBaseHealth",
                100f, new ConfigDescription("HP = BaseHealth + NecromancyLevel * HealthMultiplier", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            draugrHealthMultiplier = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrHealthMultiplier",
                5f, new ConfigDescription("HP = BaseHealth + NecromancyLevel * HealthMultiplier", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            draugrTierOneQuality = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrTierOneQuality",
             1, new ConfigDescription("Quality of tier 1 Draugr minions", null,
             new ConfigurationManagerAttributes { IsAdminOnly = true }));

            draugrTierTwoQuality = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrTierTwoQuality",
             2, new ConfigDescription("Quality of tier 2 Draugr minions", null,
             new ConfigurationManagerAttributes { IsAdminOnly = true }));

            draugrTierTwoLevelReq = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrTierTwoLevelReq",
             35, new ConfigDescription("Necromancy skill level required to summon Tier 2 Draugr", null,
             new ConfigurationManagerAttributes { IsAdminOnly = true }));

            draugrTierThreeQuality = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrTierThreeQuality",
             3, new ConfigDescription("Quality of tier 3 Draugr minions", null,
             new ConfigurationManagerAttributes { IsAdminOnly = true }));

            draugrTierThreeLevelReq = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrTierThreeLevelReq",
             70, new ConfigDescription("Necromancy skill level required to summon Tier 3 Draugr", null,
             new ConfigurationManagerAttributes { IsAdminOnly = true }));

            draugrMeatRequiredConfig = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrMeatRequired",
                1, new ConfigDescription("How many pieces of meat it costs to make a Draugr.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            draugrBoneFragmentsRequiredConfig = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrBoneFragmentsRequired",
                3, new ConfigDescription("How many bone fragments it costs to make a Draugr.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            necromancyLevelIncrease = plugin.Config.Bind("DraugrWand (Server Synced)", "DraugrNecromancyLevelIncrease",
                1.5f, new ConfigDescription("How much creating a Draugr contributes to your Necromancy level increasing.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            maxDraugr = plugin.Config.Bind("DraugrWand (Server Synced)", "MaximumDraugr",
                0, new ConfigDescription("The maximum Draugr allowed to be created (0 = unlimited).", null,
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
            config.Name = "$item_friendlyskeletonwand_draugrwand";
            config.Description = "$item_friendlyskeletonwand_draugrwand_desc";

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
                        equippedItem => equippedItem.TokenName().Equals("$item_friendlyskeletonwand_draugrwand")
                        ) != null
                    )
            {
                ExtraResourceConsumptionUnlocked =
                    UnlockExtraResourceConsumptionButton == null
                    || ZInput.GetButton(UnlockExtraResourceConsumptionButton.Name);

                if (CreateMinionButton != null && ZInput.GetButton(CreateMinionButton.Name))
                {
                    SpawnFriendlyDraugr(Player.m_localPlayer,
                        draugrBoneFragmentsRequiredConfig.Value,
                        draugrMeatRequiredConfig.Value,
                        necromancyLevelIncrease.Value,
                        false
                        );
                    return true;
                }
                else if (CreateArcherMinionButton != null && ZInput.GetButton(CreateArcherMinionButton.Name))
                {
                    SpawnFriendlyDraugr(Player.m_localPlayer,
                        draugrBoneFragmentsRequiredConfig.Value,
                        draugrMeatRequiredConfig.Value,
                        necromancyLevelIncrease.Value,
                        true
                        );
                    return true;
                }
                else if (FollowButton != null && ZInput.GetButton(FollowButton.Name))
                {
                    MakeNearbyMinionsFollow(Player.m_localPlayer, draugrSetFollowRange.Value, true);
                    return true;
                }
                else if (WaitButton != null && ZInput.GetButton(WaitButton.Name))
                {
                    MakeNearbyMinionsFollow(Player.m_localPlayer, draugrSetFollowRange.Value, false);
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

        public void SpawnFriendlyDraugr(Player player, int boneFragmentsRequired, int meatRequired, float necromancyLevelIncrease, bool archer)
        {
            if (!draugrAllowed.Value) return;

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
            }

            if (meatRequired > 0)
            {
                List<string> allowedMeatTypes = new List<string>()
                {
                    "$item_meat_rotten",
                    "$item_boar_meat",
                    "$item_necktail",
                    "$item_deer_meat",
                    "$item_loxmeat",
                    "$item_wolf_meat",
                    "$item_serpentmeat",
                    "$item_bug_meat",
                    "$item_chicken_meat",
                    "$item_hare_meat",
                };
                Dictionary<string, int> meatTypesFound = new Dictionary<string, int>();
                int meatInInventory = 0;
                allowedMeatTypes.ForEach(meatTypeStr =>
                {
                    int meatFound = player.GetInventory().CountItems(meatTypeStr);
                    if (meatFound > 0)
                    {
                        meatInInventory += meatFound;
                        meatTypesFound[meatTypeStr] = meatFound;
                    }
                }
                );

                Jotunn.Logger.LogInfo($"Meat in inventory: {meatInInventory}");
                if (meatInInventory < meatRequired)
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "$friendlyskeletonwand_notenoughmeat");
                    return;
                }

                // consume the fragments
                player.GetInventory().RemoveItem("$item_bonefragments", boneFragmentsRequired);

                // consume the meat
                int meatConsumed = 0;
                Stack<Tuple<string, int>> meatToConsume = new Stack<Tuple<string, int>>();
                foreach (string key in meatTypesFound.Keys)
                {
                    if (meatConsumed >= meatRequired) { break; }

                    int meatAvailable = meatTypesFound[key];

                    if (meatAvailable <= meatRequired)
                    {
                        meatToConsume.Push(new Tuple<string, int>(key, meatAvailable));
                        meatConsumed += meatAvailable;
                    }
                    else
                    {
                        meatToConsume.Push(new Tuple<string, int>(key, meatRequired));
                        meatConsumed += meatRequired;
                    }
                }

                while (meatToConsume.Count > 0)
                {
                    Tuple<string, int> keyValue = meatToConsume.Pop();
                    player.GetInventory().RemoveItem(keyValue.Item1, keyValue.Item2);
                }
            }

            // if players have decided to foolishly restrict their power and
            // create a *cough* LIMIT *spits*... check that here
            if (maxDraugr.Value > 0)
            {
                // re-count the current active draugr
                int activeDraugr = CountActiveDraugrMinions();
                Jotunn.Logger.LogInfo($"Draugr count: {activeDraugr}; maxDraugr = {maxDraugr.Value}");
            }

            // scale according to skill
            float playerNecromancyLevel = 1;
            try
            {
                playerNecromancyLevel = player.GetSkillLevel(SkillManager.Instance.GetSkill(BasePlugin.necromancySkillIdentifier).m_skill);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError($"Failed to get player necromancy level: {e}");
            }
            Jotunn.Logger.LogInfo($"Player necromancy level: {playerNecromancyLevel}");

            int quality = draugrTierOneQuality.Value;
            if (playerNecromancyLevel >= draugrTierThreeLevelReq.Value) { quality = draugrTierThreeQuality.Value; }
            else if (playerNecromancyLevel >= draugrTierTwoLevelReq.Value) { quality = draugrTierTwoQuality.Value; }

            // go on to spawn draugr
            string prefabName = archer ? "ChebGonaz_DraugrArcher" : "ChebGonaz_DraugrWarrior";
            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (!prefab)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"{prefabName} does not exist");
                Jotunn.Logger.LogError($"SpawnFriendlyDraugr: spawning {prefabName} failed");
            }

            Jotunn.Logger.LogInfo($"Spawning {prefabName}");
            GameObject spawnedChar = GameObject.Instantiate(prefab, player.transform.position + player.transform.forward * 2f + Vector3.up, Quaternion.identity);
            DraugrMinion minion = spawnedChar.AddComponent<DraugrMinion>();
            Character character = spawnedChar.GetComponent<Character>();
            character.m_faction = Character.Faction.Players;
            character.SetLevel(quality);
            minion.ScaleStats(playerNecromancyLevel);

            try
            {
                player.RaiseSkill(SkillManager.Instance.GetSkill(BasePlugin.necromancySkillIdentifier).m_skill, necromancyLevelIncrease);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError($"Failed to raise player necromancy level: {e}");
            }

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
        }

        public int CountActiveDraugrMinions()
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

                DraugrMinion minion = item.GetComponent<DraugrMinion>();
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
                if (result >= maxDraugr.Value - 1)
                {
                    Tuple<int, Character> tuple = minionsFound[i];
                    tuple.Item2.SetHealth(0);
                    continue;
                }

                result++;
            }

            return result;
        }
    }
}
