using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace GoldChestForAll
{
    [BepInPlugin("com.DestroyedClone.GoldChestForAll", "GoldChestForAll", "1.0.2")]
    public class GCFAPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> CfgCostMultiplier { get; set; }
        public static ConfigEntry<bool> CfgIntent { get; set; }
        public static ConfigEntry<float> CfgCostMultiplierPerPlayerAdditive { get; set; }

        public int ParticipatingPlayerCount
        {
            get
            {
                return Run.instance.participatingPlayerCount;
                //return 4;
            }
        }

        public void Start()
        {
            CfgCostMultiplier = Config.Bind("Gold Chest Cost", "Cost Multiplier", 1.00f, "Multiply the costs of gold chests. Intended for balance, but you can just set it to '1' if you want it unchanged. Applies after \"Additive Cost Multiplier Per Player\". Applies on stage start.");
            CfgIntent = Config.Bind("Default", "Only Abyssal Depths and Sundered Grove", true, "If true, then only the guaranteed chest on Sundered Grove and Abyssal Depths will be affected. Applied on stage start.");
            CfgCostMultiplierPerPlayerAdditive = Config.Bind("Gold Chest Cost", "Additive Cost Multiplier Per Player", 0f, "Ex: 0.05 would be a 5% increase in cost per player. 5% x 4 players = 20%. Applied on stage start.");

            On.RoR2.ChestBehavior.ItemDrop += DuplicateDrops;
            On.RoR2.PurchaseInteraction.Awake += MultiplyChestCost;

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
                ModCompat_RiskOfOptions.Initialize();
        }


        private bool IsValidScene(Transform chestTransform)
        {
            if (CfgIntent.Value)
            {
                bool parentIsRootJungle = chestTransform.parent && chestTransform.parent.name == "HOLDER: Newt Statues and Preplaced Chests";
                bool parentIsDampCaveSimple = chestTransform.parent
                    && chestTransform.parent.parent
                    && chestTransform.parent.parent.name == "GROUP: Large Treasure Chests";
                return parentIsDampCaveSimple || parentIsRootJungle;
            }
            return true;
        }

        private void MultiplyChestCost(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            if (NetworkServer.active && IsValidScene(self.transform))
            {
                if (self.TryGetComponent(out ChestBehavior chest) && ChestIsLegendary(chest))
                {
                    float costMultPerPlayer = 1;
                    if (CfgCostMultiplierPerPlayerAdditive.Value != 0)
                    {
                        costMultPerPlayer += ParticipatingPlayerCount * CfgCostMultiplierPerPlayerAdditive.Value;
                    }
                    float multiplier = costMultPerPlayer * CfgCostMultiplier.Value;
                    self.Networkcost = (int)Mathf.Ceil(self.cost * multiplier);
                }
            }
            orig(self);
        }

        private void DuplicateDrops(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            if (!NetworkServer.active)
                goto EarlyReturn;
            if (!IsValidScene(self.transform))
                goto EarlyReturn;
            if (!ChestIsLegendary(self))
                goto EarlyReturn;
            if (ParticipatingPlayerCount < 2)
                goto EarlyReturn;

            self.dropCount += ParticipatingPlayerCount - 1;
        EarlyReturn:
            orig(self);
        }

        private static bool ChestIsLegendary(ChestBehavior self)
        {
            return self.dropPickup != null && self.dropPickup.pickupDef.itemTier == ItemTier.Tier3;
        }
    }
}