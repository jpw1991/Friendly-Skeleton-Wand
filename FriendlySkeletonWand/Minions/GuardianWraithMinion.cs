using BepInEx;
using BepInEx.Configuration;
using Jotunn;
using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendlySkeletonWand
{
    internal class GuardianWraithMinion : UndeadMinion
    {
        public static ConfigEntry<int> guardianWraithLevelRequirement;
        public static ConfigEntry<int> guardianWraithDuration;

        private float createdAt;

        public static void CreateConfigs(BaseUnityPlugin plugin)
        {
            guardianWraithLevelRequirement = plugin.Config.Bind("GuardianWraith (Server Synced)", "GuardianWraithLevelRequirement",
                25, new ConfigDescription("The Necromancy level required to control a Guardian Wraith.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            guardianWraithDuration = plugin.Config.Bind("GuardianWraith (Server Synced)", "GuardianWraithDuration",
                10, new ConfigDescription("The lifetime of a Guardian Wraith.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
        }

        private void Awake()
        {
            createdAt = Time.time;
            canBeCommanded = false;
        }

        private void Update()
        {
            if (Time.time > createdAt + guardianWraithDuration.Value)
            {
                Kill();
            }
            else if (Player.m_localPlayer != null
                && Player.m_localPlayer.IsTeleporting())
            {
                if (TryGetComponent(out Character character))
                {
                    if (character.IsOwner())
                    {
                        Kill();
                        Jotunn.Logger.LogInfo("GuardianWraithMinion: killing because player entered portal.");
                    }
                }
            }
        }

        private void Kill()
        {
            if (TryGetComponent(out Humanoid humanoid))
            {
                humanoid.SetHealth(0);
            }
        }
    }
}
