﻿using BepInEx.Configuration;
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

namespace FriendlySkeletonWand
{
    internal class DraugrWand : Wand
    {
        public static List<GameObject> draugr = new List<GameObject>();
        public static ConfigEntry<int> maxDraugr;

        public static ConfigEntry<float> draugrBaseHealth;
        public static ConfigEntry<float> draugrHealthMultiplier;
        public static ConfigEntry<float> draugrSetFollowRange;

        private ConfigEntry<float> necromancyLevelIncrease;

        public static ConfigEntry<int> draugrBoneFragmentsRequiredConfig;

        public DraugrWand()
        {
            ItemName = "FriendlySkeletonWand_DraugrWand";
        }

        public override void CreateConfigs(BaseUnityPlugin plugin)
        {
            base.CreateConfigs(plugin);

            draugrBaseHealth = plugin.Config.Bind("Client config", "DraugrBaseHealth",
                100f, new ConfigDescription("$friendlyskeletonwand_config_draugrbasehealth_desc"));

            draugrHealthMultiplier = plugin.Config.Bind("Client config", "DraugrHealthMultiplier",
                5f, new ConfigDescription("$friendlyskeletonwand_config_draugrhealthmultiplier_desc"));

            draugrSetFollowRange = plugin.Config.Bind("Client config", "DraugrSetFollowRange",
                10f, new ConfigDescription("$friendlyskeletonwand_config_draugrsetfollowrange_desc"));

            draugrBoneFragmentsRequiredConfig = plugin.Config.Bind("Client config", "DraugrBoneFragmentsRequired",
                3, new ConfigDescription("$friendlyskeletonwand_config_draugrbonefragmentsrequired_desc"));

            necromancyLevelIncrease = plugin.Config.Bind("Client config", "DraugrNecromancyLevelIncrease",
                1.5f, new ConfigDescription("$friendlyskeletonwand_config_necromancylevelincrease_desc"));

            maxDraugr = plugin.Config.Bind("Client config", "MaximumDraugr",
                0, new ConfigDescription("$friendlyskeletonwand_config_maxdraugr_desc"));
        }

        public override void CreateButtons()
        {
            // call the base to add the basic generic buttons -> create, attack, follow, wait, etc.
            base.CreateButtons();

            // add any extra buttons
            
        }

        public override CustomItem GetCustomItem()
        {
            // Create and add a custom item based on Club
            ItemConfig draugrWandConfig = new ItemConfig();
            draugrWandConfig.Name = "$item_friendlyskeletonwand_draugrwand";
            draugrWandConfig.Description = "$item_friendlyskeletonwand_draugrwand_desc";
            draugrWandConfig.CraftingStation = "piece_workbench";
            draugrWandConfig.AddRequirement(new RequirementConfig("ElderBark", 5));
            draugrWandConfig.AddRequirement(new RequirementConfig("FineWood", 5));
            draugrWandConfig.AddRequirement(new RequirementConfig("Bronze", 5));
            draugrWandConfig.AddRequirement(new RequirementConfig("TrophyDraugr", 1));

            return new CustomItem(ItemName, "Club", draugrWandConfig);
        }

        public override KeyHintConfig GetKeyHint()
        {
            return new KeyHintConfig
            {
                Item = ItemName,
                ButtonConfigs = new[]
                {
                    //new ButtonConfig { Name = "Attack", HintToken = "$friendlyskeletonwand_attack" },
                    CreateMinionButton,
                    CreateArcherMinionButton,
                    FollowButton,
                    WaitButton,
                    TeleportButton,
                    AttackTargetButton,
                }
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
                if (CreateMinionButton != null && ZInput.GetButton(CreateMinionButton.Name))
                {
                    SpawnFriendlyDraugr(Player.m_localPlayer,
                        draugrBoneFragmentsRequiredConfig.Value,
                        necromancyLevelIncrease.Value,
                        false
                        );
                    return true;
                }
                else if (CreateArcherMinionButton != null && ZInput.GetButton(CreateArcherMinionButton.Name))
                {
                    SpawnFriendlyDraugr(Player.m_localPlayer,
                        draugrBoneFragmentsRequiredConfig.Value,
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

        public static void AdjustDraugrStatsToNecromancyLevel(GameObject draugrInstance, float necromancyLevel)
        {
            Character character = draugrInstance.GetComponent<Character>();
            if (character == null)
            {
                Jotunn.Logger.LogError("AdjustDraugrStatsToNecromancyLevel: error -> failed to scale minion to player necromancy level -> Character component is null!");
                return;
            }
            float health = draugrBaseHealth.Value + necromancyLevel * draugrHealthMultiplier.Value;
            character.SetMaxHealth(health);
            character.SetHealth(health);
        }

        public void SpawnFriendlyDraugr(Player player, int boneFragmentsRequired, float necromancyLevelIncrease, bool archer)
        {
            // if players have decided to foolishly restrict their power and
            // create a *cough* LIMIT *spits*... check that here
            if (maxDraugr.Value > 0)
            {
                // re-count the current active draugr
                for (int i = draugr.Count - 1; i >= 0; i--) { if (draugr[i] == null) { draugr.RemoveAt(i); } }
                if (draugr.Count >= maxDraugr.Value)
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "$friendlyskeletonwand_limitexceeded");
                    return;
                }
            }

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

            int quality = 1;
            if (playerNecromancyLevel >= 70) { quality = 3; }
            else if (playerNecromancyLevel >= 35) { quality = 2; }

            // go on to spawn skeleton
            string prefabName = archer ? "ChebGonaz_DraugrArcher" : "ChebGonaz_DraugrWarrior";
            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (!prefab)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"{prefabName} does not exist");
                Jotunn.Logger.LogError($"SpawnFriendlyDraugr: spawning {prefabName} failed");
            }

            Jotunn.Logger.LogInfo($"Spawning {prefabName}");
            GameObject spawnedChar = GameObject.Instantiate(prefab, player.transform.position + player.transform.forward * 2f + Vector3.up, Quaternion.identity);
            spawnedChar.AddComponent<UndeadMinion>();
            Character character = spawnedChar.GetComponent<Character>();
            character.m_faction = Character.Faction.Players;
            character.SetLevel(quality);
            AdjustDraugrStatsToNecromancyLevel(spawnedChar, playerNecromancyLevel);

            try
            {
                player.RaiseSkill(SkillManager.Instance.GetSkill(BasePlugin.necromancySkillIdentifier).m_skill, necromancyLevelIncrease);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError($"Failed to raise player necromancy level: {e}");
            }
        }
    }
}
