using System;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using Jotunn.Managers;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace FriendlySkeletonWand.Minions
{
    internal class NecroNeckGathererMinion : UndeadMinion
    {
        // for limits checking
        private static int createdOrderIncrementer;
        public int createdOrder;

        private float lastUpdate;

        public static ConfigEntry<bool> allowed;
        public static ConfigEntry<float> updateDelay, lookRadius, pickupRadius, dropoffPointRadius;

        private int autoPickupMask, pieceMask;

        private Container container;

        private Container dropoffTarget;

        public static void CreateConfigs(BaseUnityPlugin plugin)
        {
            allowed = plugin.Config.Bind("NecroNeckGatherer (Server Synced)", "NecroNeckGathererAllowed",
                true, new ConfigDescription("Whether the NecroNeck Gatherer is allowed or not.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            lookRadius = plugin.Config.Bind("NecroNeckGatherer (Server Synced)", "NecroNeckGathererLookRadius",
                50f, new ConfigDescription("The radius in which the NecroNeck Gatherer can see items from.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            pickupRadius = plugin.Config.Bind("NecroNeckGatherer (Server Synced)", "NecroNeckGathererPickupRadius",
                10f, new ConfigDescription("The radius in which the NecroNeck Gatherer can pickup items from.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            dropoffPointRadius = plugin.Config.Bind("NecroNeckGatherer (Server Synced)", "NecroNeckGathererDropoffPointRadius",
                100f, new ConfigDescription("The radius in which the NecroNeck Gatherer looks for a container to store its load in.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            updateDelay = plugin.Config.Bind("NecroNeckGatherer (Server Synced)", "NecroNeckGathererUpdateDelay",
                3f, new ConfigDescription("The delay, in seconds, between item searching & pickup attempts. Attention: small values may impact performance.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
        }

        private void Awake()
        {
            createdOrderIncrementer++;
            createdOrder = createdOrderIncrementer;

            Tameable tameable = GetComponent<Tameable>();
            if (tameable != null)
            {
                // let the minions generate a little necromancy XP for their master
                tameable.m_levelUpOwnerSkill = SkillManager.Instance.GetSkill(BasePlugin.necromancySkillIdentifier).m_skill;
            }

            container = GetComponent<Container>();

            container.m_height = LargeCargoCrate.containerHeight.Value;
            container.m_width = LargeCargoCrate.containerWidth.Value;

            autoPickupMask = LayerMask.GetMask(new string[1] { "item" });
            pieceMask = LayerMask.GetMask(new string[1] { "piece" });

            canBeCommanded = false;
        }

        private void Update()
        {
            if (ZNet.instance != null && Time.time > lastUpdate)
            {
                if (ReturnHome())
                {
                    Chat.instance.SetNpcText(gameObject, Vector3.up * 1.5f, 30f, 2f, "", "Going home!", true);
                    if (dropoffTarget == null)
                    {
                        if (FindNearestDropOffPoint())
                        {
                            Chat.instance.SetNpcText(gameObject, Vector3.up * 1.5f, 30f, 2f, "", $"Moving toward {dropoffTarget.name}", true);
                        }
                        else
                        {
                            Chat.instance.SetNpcText(gameObject, Vector3.up * 1.5f, 30f, 2f, "", "Can't find a container", true);
                        }
                    }
                    else if (CloseToDropoffPoint())
                    {
                        DepositItems();
                    }
                }
                else
                {
                    LookForNearbyItems();
                    PickupNearbyItems();
                }

                lastUpdate = Time.time + updateDelay.Value;
            }
        }

        private void LookForNearbyItems()
        {
            // get all nearby items
            Collider[] hitColliders = Physics.OverlapSphere(transform.position + Vector3.up, lookRadius.Value, autoPickupMask);
            if (hitColliders.Length < 1) return;
            // order items from closest to furthest, then take closest one
            Collider closest = hitColliders
                .OrderBy(collider => Vector3.Distance(transform.position, collider.transform.position))
                .First();
            if (closest != null)
            {
                ItemDrop itemDrop = closest.GetComponentInParent<ItemDrop>();
                if (itemDrop != null)
                {
                    if (TryGetComponent(out MonsterAI monsterAI))
                    {
                        // move toward that item
                        monsterAI.SetFollowTarget(itemDrop.gameObject);
                        return;
                    }
                }
            }
        }

        private void PickupNearbyItems()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position + Vector3.up, pickupRadius.Value, autoPickupMask);
            foreach (var hitCollider in hitColliders)
            {
                ItemDrop itemDrop = hitCollider.GetComponentInParent<ItemDrop>();
                if (itemDrop != null)
                {
                    if (itemDrop.CanPickup())
                    {
                        StoreItem(itemDrop, container);
                    }
                }
            }
        }

        private void StoreItem(ItemDrop itemDrop, Container depositContainer)
        {
            ItemDrop.ItemData itemData = itemDrop.m_itemData;
            if (itemData == null) return;

            if (itemData.m_stack < 1) return;

            while (itemData.m_stack-- > 0 && depositContainer.GetInventory().CanAddItem(itemData, 1))
            {
                ItemDrop.ItemData newItemData = itemData.Clone();
                newItemData.m_stack = 1;
                depositContainer.GetInventory().AddItem(newItemData);
            }

            depositContainer.Save();

            if (itemDrop.GetComponent<ZNetView>() == null)
                DestroyImmediate(itemDrop.gameObject);
            else
                ZNetScene.instance.Destroy(itemDrop.gameObject);
        }

        private bool ReturnHome()
        {
            // return home if no slots found
            return container.GetInventory().GetEmptySlots() < 1;
        }

        private bool FindNearestDropOffPoint()
        {
            // return true if drop off point found

            // doesnt work, dunno why
            //List<Piece> nearbyPieces = new List<Piece>();
            //Piece.GetAllPiecesInRadius(transform.position, dropoffPointRadius.Value, nearbyPieces);
            //
            //if (nearbyPieces.Count < 1) return false;
            Collider[] nearbyPieces = Physics.OverlapSphere(transform.position + Vector3.up, dropoffPointRadius.Value, pieceMask);
            if (nearbyPieces.Length < 1) return false;

            // order piece from closest to furthest, then take closest container
            Container closest = nearbyPieces
                .OrderBy(piece => Vector3.Distance(transform.position, piece.transform.position))
                .Single(piece => piece.GetComponentInParent<Container>() != null
                    && piece.GetComponentInParent<Container>().GetInventory().GetEmptySlots() > 0)
                .GetComponentInParent<Container>();
            if (closest != null)
            {
                if (TryGetComponent(out MonsterAI monsterAI))
                {
                    // move toward that piece
                    monsterAI.SetFollowTarget(closest.gameObject);
                    dropoffTarget = closest;
                    return true;
                }
            }
            return false;
        }

        private bool CloseToDropoffPoint()
        {
            return dropoffTarget != null && Vector3.Distance(transform.position, dropoffTarget.transform.position) < 5;
        }

        private void DepositItems()
        {
            dropoffTarget.GetInventory().MoveAll(container.GetInventory());
        }
    }
}
