using Jotunn.Entities;
using BepInEx.Configuration;
using BepInEx;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Jotunn.Configs;
using System.Reflection;
using System.Linq;

namespace FriendlySkeletonWand
{

    public class InternalName : Attribute
    {
        public readonly string internalName;
        public InternalName(string internalName) => this.internalName = internalName;
    }

    public enum CraftingTable
    {
        None,
        [InternalName("piece_workbench")] Workbench,
        [InternalName("piece_cauldron")] Cauldron,
        [InternalName("forge")] Forge,
        [InternalName("piece_artisanstation")] ArtisanTable,
        [InternalName("piece_stonecutter")] StoneCutter
    }

    internal class Item
    {
        public ConfigEntry<bool> allowed;

        public virtual string ItemName { get { return ""; } }
        public virtual string PrefabName { get { return ""; } }

        public virtual void CreateConfigs(BaseUnityPlugin plugin) {}

        //
        // Summary:
        //      Method SetRecipeReqs sets the material requirements needed to craft the item via a recipe.
        public void SetRecipeReqs(
            ItemConfig recipeConfig,
            ConfigEntry<string> craftingCost, 
            ConfigEntry<CraftingTable> craftingStationRequired,
            ConfigEntry<int> craftingStationLevel
            )
        {

            // function to add a single material to the recipe
            void addMaterial(string material)
            {
                string[] materialSplit = material.Split(':');
                string materialName = materialSplit[0];
                int materialAmount = int.Parse(materialSplit[1]);
                recipeConfig.AddRequirement(new RequirementConfig(materialName, materialAmount, materialAmount * 2));
            }

            // set the crafting station to craft it on
            recipeConfig.CraftingStation = ((InternalName)typeof(CraftingTable).GetMember(craftingStationRequired.Value.ToString())[0].GetCustomAttributes(typeof(InternalName)).First()).internalName;

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

            // Set the minimum required crafting station level to craft
            recipeConfig.MinStationLevel = craftingStationLevel.Value;

            return;
        }


        // coroutines cause problems and this is not a monobehavior, but we
        // may still want some stuff to happen during update.
        protected float doOnUpdateDelay;
        public virtual void DoOnUpdate()
        {

        }
    }
}
