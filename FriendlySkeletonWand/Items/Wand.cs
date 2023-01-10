using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FriendlySkeletonWand
{
    internal class Wand : Item
    {
        public ConfigEntry<bool> followByDefault;

        public ConfigEntry<KeyCode> CreateMinionConfig;
        public ConfigEntry<InputManager.GamepadButton> CreateMinionGamepadConfig;
        public ButtonConfig CreateMinionButton;

        public ConfigEntry<KeyCode> FollowConfig;
        public ConfigEntry<InputManager.GamepadButton> FollowGamepadConfig;
        public ButtonConfig FollowButton;

        public ConfigEntry<KeyCode> WaitConfig;
        public ConfigEntry<InputManager.GamepadButton> WaitGamepadConfig;
        public ButtonConfig WaitButton;

        public ConfigEntry<KeyCode> TeleportConfig;
        public ConfigEntry<InputManager.GamepadButton> TeleportGamepadConfig;
        public ButtonConfig TeleportButton;

        public ConfigEntry<KeyCode> AttackTargetConfig;
        public ConfigEntry<InputManager.GamepadButton> AttackTargetGamepadConfig;
        public ButtonConfig AttackTargetButton;

        public ConfigEntry<KeyCode> CreateArcherMinionConfig;
        public ConfigEntry<InputManager.GamepadButton> CreateArcherMinionGamepadConfig;
        public ButtonConfig CreateArcherMinionButton;

        public ConfigEntry<KeyCode> UnlockExtraResourceConsumptionConfig;
        public ButtonConfig UnlockExtraResourceConsumptionButton;

        public bool ExtraResourceConsumptionUnlocked = false;

        private GameObject targetObject;

        public override void CreateConfigs(BaseUnityPlugin plugin)
        {
            followByDefault = plugin.Config.Bind("Wands (Client)", "FollowByDefault",
                false, new ConfigDescription("Whether minions will automatically be set to follow upon being created or not."));

            CreateMinionConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"CreateMinion",
                KeyCode.B, new ConfigDescription("The key to create a warrior minion with."));

            CreateMinionGamepadConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"CreateMinionGamepad",
                InputManager.GamepadButton.ButtonSouth,
                new ConfigDescription("The key to gamepad button to create a warrior minion with."));

            CreateArcherMinionConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"CreateArcher",
                KeyCode.H, new ConfigDescription("The key to create an archer minion with."));

            CreateArcherMinionGamepadConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"CreateArcherGamepad",
                InputManager.GamepadButton.ButtonSouth,
                new ConfigDescription("The key to gamepad button to create an archer minion with."));

            FollowConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"Follow",
                KeyCode.F, new ConfigDescription("The key to tell minions to follow."));

            FollowGamepadConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"FollowGamepad",
                InputManager.GamepadButton.ButtonWest,
                new ConfigDescription("The gamepad button to tell minions to follow."));

            WaitConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"Wait",
                KeyCode.T, new ConfigDescription("The key to tell minions to wait."));

            WaitGamepadConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"WaitGamepad",
                InputManager.GamepadButton.ButtonEast,
                new ConfigDescription("The gamepad button to tell minions to wait."));

            TeleportConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"Teleport",
                KeyCode.G, new ConfigDescription("The key to teleport following minions to you."));

            TeleportGamepadConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"TeleportGamepad",
                InputManager.GamepadButton.SelectButton,
                new ConfigDescription("The gamepad button to teleport following minions to you."));

            AttackTargetConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"Target",
                KeyCode.R, new ConfigDescription("The key to tell minions to go to a specific target."));

            AttackTargetGamepadConfig = plugin.Config.Bind("Keybinds (Client)", ItemName+"TargetGamepad",
                InputManager.GamepadButton.StartButton,
                new ConfigDescription("The gamepad button to tell minions to go to a specific target."));

            UnlockExtraResourceConsumptionConfig = plugin.Config.Bind("Keybinds (Client)", ItemName + "UnlockExtraResourceConsumption",
                KeyCode.LeftShift, new ConfigDescription("The key to permit consumption of additional resources when creating the minion eg. iron to make an armored skeleton."));
        }

        public virtual void CreateButtons()
        {
            if (CreateMinionConfig.Value != KeyCode.None)
            {
                CreateMinionButton = new ButtonConfig
                {
                    Name = ItemName + "CreateMinion",
                    Config = CreateMinionConfig,
                    GamepadConfig = CreateMinionGamepadConfig,
                    HintToken = "$friendlyskeletonwand_create",
                    BlockOtherInputs = true
                };
                InputManager.Instance.AddButton(BasePlugin.PluginGUID, CreateMinionButton);
            }

            if (CreateArcherMinionConfig.Value != KeyCode.None)
            {
                CreateArcherMinionButton = new ButtonConfig
                {
                    Name = ItemName + "CreateArcherMinion",
                    Config = CreateArcherMinionConfig,
                    GamepadConfig = CreateArcherMinionGamepadConfig,
                    HintToken = "$friendlyskeletonwand_create_archer",
                    BlockOtherInputs = true
                };
                InputManager.Instance.AddButton(BasePlugin.PluginGUID, CreateArcherMinionButton);
            }

            if (FollowConfig.Value != KeyCode.None)
            {
                FollowButton = new ButtonConfig
                {
                    Name = ItemName + "Follow",
                    Config = FollowConfig,
                    GamepadConfig = FollowGamepadConfig,
                    HintToken = "$friendlyskeletonwand_follow",
                    BlockOtherInputs = true
                };
                InputManager.Instance.AddButton(BasePlugin.PluginGUID, FollowButton);
            }

            if (WaitConfig.Value != KeyCode.None)
            {
                WaitButton = new ButtonConfig
                {
                    Name = ItemName + "Wait",
                    Config = WaitConfig,
                    GamepadConfig = WaitGamepadConfig,
                    HintToken = "$friendlyskeletonwand_wait",
                    BlockOtherInputs = true
                };
                InputManager.Instance.AddButton(BasePlugin.PluginGUID, WaitButton);
            }

            if (TeleportConfig.Value != KeyCode.None)
            {
                TeleportButton = new ButtonConfig
                {
                    Name = ItemName + "Teleport",
                    Config = TeleportConfig,
                    GamepadConfig = TeleportGamepadConfig,
                    HintToken = "$friendlyskeletonwand_teleport",
                    BlockOtherInputs = true
                };
                InputManager.Instance.AddButton(BasePlugin.PluginGUID, TeleportButton);
            }

            if (AttackTargetConfig.Value != KeyCode.None)
            {
                AttackTargetButton = new ButtonConfig
                {
                    Name = ItemName + "AttackTarget",
                    Config = AttackTargetConfig,
                    GamepadConfig = AttackTargetGamepadConfig,
                    HintToken = "$friendlyskeletonwand_attacktarget",
                    BlockOtherInputs = true
                };
                InputManager.Instance.AddButton(BasePlugin.PluginGUID, AttackTargetButton);
            }

            if (UnlockExtraResourceConsumptionConfig.Value != KeyCode.None)
            {
                UnlockExtraResourceConsumptionButton = new ButtonConfig
                {
                    Name = ItemName + "UnlockExtraResourceConsumption",
                    Config = UnlockExtraResourceConsumptionConfig,
                    //GamepadConfig = AttackTargetGamepadConfig,
                    HintToken = "$friendlyskeletonwand_unlockextraresourceconsumption",
                    BlockOtherInputs = false
                };
                InputManager.Instance.AddButton(BasePlugin.PluginGUID, UnlockExtraResourceConsumptionButton);
            }
        }

        public virtual KeyHintConfig GetKeyHint()
        {
            return null;
        }

        public virtual void AddInputs()
        {

        }

        public virtual bool HandleInputs() { return false; }

        public void MakeNearbyMinionsFollow(Player player, float radius, bool follow)
        {
            // based off BaseAI.FindClosestCreature
            List<Character> allCharacters = Character.GetAllCharacters();
            foreach (Character item in allCharacters)
            {
                if (item.IsDead() || !item.IsOwner())
                {
                    continue;
                }

                UndeadMinion minion = item.GetComponent<UndeadMinion>();
                if (minion != null && minion.canBeCommanded)
                {
                    float distance = Vector3.Distance(item.transform.position, player.transform.position);
                    // if within radius OR it's set to the targetObject so you can recall those you've commanded
                    // to be somewhere that's beyond the radius
                    if (distance < radius
                        || (item.GetComponent<MonsterAI>().GetFollowTarget() == targetObject
                        && item.GetComponent<MonsterAI>().GetFollowTarget() != null
                        ))
                    {
                        MonsterAI monsterAI = item.GetComponent<MonsterAI>();
                        if (monsterAI == null)
                        {
                            Jotunn.Logger.LogError("MakeNearbyMinionsFollow: failed to make skeleton follow -> MonsterAI is null!");
                        }
                        else
                        {
                            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                                follow ? "$friendlyskeletonwand_skeletonfollowing" : "$friendlyskeletonwand_skeletonwaiting");
                            monsterAI.SetFollowTarget(follow ? player.gameObject : null);
                        }
                    }
                }
            }
        }

        public void TeleportFollowingMinionsToPlayer(Player player)
        {
            // based off BaseAI.FindClosestCreature
            List<Character> allCharacters = Character.GetAllCharacters();
            foreach (Character item in allCharacters)
            {
                if (item.IsDead())
                {
                    continue;
                }

                if (item.GetComponent<UndeadMinion>() != null
                    && item.GetComponent<MonsterAI>().GetFollowTarget() == player.gameObject)
                {
                    item.transform.position = player.transform.position;
                }
            }
        }

        public void MakeFollowingMinionsAttackTarget(Player player)
        {
            // make raycast between player and whatever is being looked at
            // we also need to highlight the target somehow (dont know how yet)
            // eg. change its shader
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                                "$friendlyskeletonwand_targetfound");
                targetObject = GameObject.Instantiate(ZNetScene.instance.GetPrefab("Stone"));
                targetObject.transform.position = hit.transform.position;
                targetObject.transform.position += new Vector3(0, 10, 0);
                targetObject.transform.localScale = Vector3.one * 5;
                targetObject.name = "FriendlySkeletonWandTarget";
            }
            else
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                                "$friendlyskeletonwand_notargetfound");
                return;
            }

            // based off BaseAI.FindClosestCreature
            List<Character> allCharacters = Character.GetAllCharacters();
            foreach (Character item in allCharacters)
            {
                if (item.IsDead())
                {
                    continue;
                }

                if (item.GetComponent<UndeadMinion>() != null
                    && item.GetComponent<MonsterAI>().GetFollowTarget() == player.gameObject)
                {
                    item.GetComponent<MonsterAI>().SetFollowTarget(targetObject);
                }
            }
        }

        public virtual CustomItem GetCustomItemFromPrefab(GameObject prefab)
        {
            return null;
        }
    }
}
