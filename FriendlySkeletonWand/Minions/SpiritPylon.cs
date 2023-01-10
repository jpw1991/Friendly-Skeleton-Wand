using BepInEx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jotunn.Configs;
using Jotunn.Managers;
using BepInEx.Configuration;
using System.Linq;
using System;
using FriendlySkeletonWand;
using Jotunn.Entities;

namespace FriendlySkeletonWand
{
    internal class SpiritPylon : MonoBehaviour
    {
        public static ConfigEntry<bool> allowed;
        public static ConfigEntry<string> craftingCost;
        public static ConfigEntry<float> sightRadius;
        public static ConfigEntry<float> ghostDuration;
        public static ConfigEntry<float> delayBetweenGhosts;
        public static ConfigEntry<int> maxGhosts;

        public static string PrefabName = "ChebGonaz_SpiritPylon";
        public static string PieceTable = "Hammer";
        public static string IconName = "chebgonaz_spiritpylon_icon.png";
        protected List<GameObject> spawnedGhosts = new List<GameObject>();

        private float ghostLastSpawnedAt;

        //Jotunn.Logger.LogInfo($"Loading {SpiritPylon.PrefabName}...");
         //           if (spiritPylonPrefab == null)
          //          {
            //            Jotunn.Logger.LogError($"AddCustomStructures: {SpiritPylon.PrefabName} is null!");
              //          return;
                //    }

    //spiritPylonPrefab.AddComponent<SpiritPylon>();

      //              PieceConfig spiritPylon = new PieceConfig();
    // spiritPylon.PieceTable = "";
    //SpiritPylon.SetRequirements(spiritPylon);
      //              spiritPylon.Icon = chebgonazAssetBundle.LoadAsset<Sprite>(SpiritPylon.IconName);

                    
        //        }

        public CustomPiece GetCustomPieceFromPrefab(GameObject prefab, Sprite icon)
        {
            PieceConfig config = new PieceConfig();
            config.Name = "ChebGonaz_SpiritPylon";
            config.Description = "Spirit Pylon";

            if (allowed.Value)
            {
                // set recipe requirements
                SetRecipeReqs(config, craftingCost);
            }
            else
            {
                config.Enabled = false;
            }

            config.Icon = icon;
            config.PieceTable = "_HammerPieceTable";
            config.Category = "Misc";

            CustomPiece customPiece = new CustomPiece(prefab, false, config);
            if (customPiece == null)
            {
                Jotunn.Logger.LogError($"AddCustomPieces: {PrefabName}'s CustomPiece is null!");
                return null;
            }
            if (customPiece.PiecePrefab == null)
            {
                Jotunn.Logger.LogError($"AddCustomPieces: {PrefabName}'s PiecePrefab is null!");
                return null;
            }

            return customPiece;
        }


        public void SetRecipeReqs(PieceConfig config, ConfigEntry<string> craftingCost)
        {
            // function to add a single material to the recipe
            void addMaterial(string material)
            {
                string[] materialSplit = material.Split(':');
                string materialName = materialSplit[0];
                int materialAmount = int.Parse(materialSplit[1]);
                config.AddRequirement(new RequirementConfig(materialName, materialAmount, 0, true));
            }

            // build the recipe. material config format ex: Wood:5,Stone:1,Resin:1
            if (craftingCost.Value.Contains(','))
            {
                string[] materialList = craftingCost.Value.Split(',');

                foreach (string material in materialList)
                {
                    addMaterial(material);
                }
            }
            else
            {
                addMaterial(craftingCost.Value);
            }
        }

        public static void CreateConfigs(BaseUnityPlugin plugin)
        {
            allowed = plugin.Config.Bind("SpiritPylon (Server Synced)", "SpiritPylonAllowed",
                true, new ConfigDescription("Whether making a Spirit Pylon is allowed or not.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            craftingCost = plugin.Config.Bind("SpiritPylon (Server Synced)", "Spirit Pylon Build Costs",
                "Stone:15,Wood:15,BoneFragments:15,SurtlingCore:1", new ConfigDescription("Materials needed to build Spirit Pylon", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            sightRadius = plugin.Config.Bind("SpiritPylon (Server Synced)", "SpiritPylonSightRadius",
                30f, new ConfigDescription("How far a Spirit Pylon can see enemies.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            ghostDuration = plugin.Config.Bind("SpiritPylon (Server Synced)", "SpiritPylonGhostDuration",
                30f, new ConfigDescription("How long a Spirit Pylon's ghost persists.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            delayBetweenGhosts = plugin.Config.Bind("SpiritPylon (Server Synced)", "SpiritPylonDelayBetweenGhosts",
                5f, new ConfigDescription("How long a Spirit Pylon must wait before being able to spawn another ghost.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            maxGhosts = plugin.Config.Bind("SpiritPylon (Server Synced)", "SpiritPylonMaxGhosts",
                3, new ConfigDescription("The maximum number of ghosts that a Spirit Pylon can spawn.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
        }

        private void Awake()
        {
            StartCoroutine(LookForEnemies());
        }

        IEnumerator LookForEnemies()
        {
            while (ZInput.instance == null)
            {
                yield return new WaitForSeconds(2);
            }

            // prevent coroutine from doing its thing while the pylon isn't
            // yet constructed
            Piece piece = GetComponent<Piece>();
            while (!piece.IsPlacedByPlayer())
            {
                //Jotunn.Logger.LogInfo("Waiting for player to place pylon...");
                yield return new WaitForSeconds(2);
            }

            while (true)
            {
                yield return new WaitForSeconds(2);

                // clear out any dead/destroyed ghosts
                for (int i=spawnedGhosts.Count-1; i>=0; i--)
                {
                    if (spawnedGhosts[i] == null)
                    {
                        spawnedGhosts.RemoveAt(i);
                    }
                }

                if (Player.m_localPlayer != null)
                {
                    if (EnemiesNearby(out Character characterInRange))
                    {
                        // spawn ghosts up until the limit
                        if (spawnedGhosts.Count < maxGhosts.Value)
                        {
                            if (Time.time > ghostLastSpawnedAt + delayBetweenGhosts.Value)
                            {
                                ghostLastSpawnedAt = Time.time;

                                GameObject friendlyGhost = SpawnFriendlyGhost();
                                friendlyGhost.GetComponent<MonsterAI>().SetTarget(characterInRange);
                                spawnedGhosts.Add(friendlyGhost);
                            }
                        }
                    }  
                }
            }
        }

        protected bool EnemiesNearby(out Character characterInRange)
        {
            List<Character> charactersInRange = new List<Character>();
            Character.GetCharactersInRange(
                transform.position,
                sightRadius.Value,
                charactersInRange
                );
            foreach (Character character in charactersInRange)
            {
                if (character != null && character.m_faction != Character.Faction.Players)
                {
                    characterInRange = character;
                    return true;
                }
            }
            characterInRange = null;
            return false;
        }

        protected GameObject SpawnFriendlyGhost()
        {
            int quality = 1;

            string prefabName = "ChebGonaz_SpiritPylonGhost";
            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (!prefab)
            {
                Jotunn.Logger.LogError($"SpawnFriendlyGhost: spawning {prefabName} failed!");
                return null;
            }

            GameObject spawnedChar = Instantiate(
                prefab,
                transform.position + transform.forward * 2f + Vector3.up,
                Quaternion.identity);

            Character character = spawnedChar.GetComponent<Character>();
            character.SetLevel(quality);

            return spawnedChar;
        }
    }
}
