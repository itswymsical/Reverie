using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using System;
using Terraria.ModLoader;
using Reverie.Content.Terraria.NPCs.WorldNPCs;

namespace Reverie.Core.Missions
{
    public class MissionDataFactory
    {
        private Dictionary<int, Func<MissionData>> missionData;
        public static MissionDataFactory Instance => ModContent.GetInstance<MissionDataFactory>();
        public MissionDataFactory() => InitializeMissionData();

        private void InitializeMissionData()
        {
            missionData = new Dictionary<int, Func<MissionData>>
            {
                #region Sideline Missions

                {MissionID.TestMission, () => new MissionData(
                    MissionID.TestMission,
                    "Test Mission",
                    "This should not be playable. how did you get this, bud?",
                    [
                        [("Collect Dirt", 1)],
                    ],
                    [new Item(ItemID.DirtBlock)],
                    isMainline: false,
                    NPCID.Guide
                )},

                {MissionID.DirtiestBlock, () => new MissionData(
                    MissionID.DirtiestBlock,
                    "Merchant's Resolve",
                    "The merchant has been searchin' for the Dirtiest Block. Help him find it!",
                    [
                        [("Find the Dirtiest Block", 1)],
                    ],
                    [ new Item(ItemID.CopperCoin, 9999), new Item(ItemID.CopperBar, 100)],
                    isMainline: false,
                    npc: NPCID.Merchant,
                    xpReward: 500
                )},

                {MissionID.FoolsGold, () => new MissionData(
                    MissionID.FoolsGold,
                    "Fool's Gold",
                    "'The gold market isn't thriving too well, and the I've got has bills to pay. You're bills, keep in mind.'" +
                    "\nCollect copper and lead to help the merchant cultivate fools gold.",
                    [
                        [("Collect Copper", 38), ("Collect Lead", 24)],
                        [("Check in with the Merchant", 1)]
                    ],
                    [new Item(ItemID.AncientGoldHelmet), new Item(ItemID.GoldBar, Main.rand.Next(9, 18)), new Item(ItemID.SilverCoin, 75)],
                    isMainline: false,
                    NPCID.Merchant,
                    xpReward: 70
                )},

                #endregion

                #region Mainline Missions
                {MissionID.Reawakening, () => new MissionData(
                    MissionID.Reawakening,
                    "Reawakening",
                    "\nYou wake up from comatose greeted by the Guide.",
                    [
                        [("Talk to the Guide", 1)],
                        [("Attack the target dummy", 10), ("Talk to the Guide", 1)],
                        [("Kill slimes", 3), ("Obtain torches", 16)],
                        [("Obtain Iron or Lead ore", 15), ("Obtain an Anvil", 1)],
                        [("Obtain a better pickaxe", 1), ("Obtain a set of armor", 3)],
                        [("Obtain Life Crystals", 3), ("Obtain healing potions", 5)],
                        [("Check in with the Guide", 1)]
                    ],

                    [new Item(ItemID.SilverCoin, Main.rand.Next(7, 19)),
                     new Item(ItemID.CopperCoin, Main.rand.Next(18, 44))],

                    isMainline: true,
                    NPCID.Guide,
                    xpReward: 50,
                    nextMissionID: MissionID.Translocator
                )},

                {MissionID.Translocator, () => new MissionData(
                    MissionID.Translocator,
                    "Realm Reposition",
                    "Create a Translocator with the item's on your task list." +
                    "\nCreate a Translocator with the item's on your task list.",
                    [
                        [("Craft a Realm Crystal", 1), 
                        ("Craft a Coil Array", 1), 
                        ("Craft a Dimensional Tuning Fork", 1)],
                        [("Check in with the Guide", 1)]
                    ],

                    [new Item(ItemID.SilverCoin, Main.rand.Next(11, 63)),
                        new Item(ItemID.CopperCoin, Main.rand.Next(18, 62))],

                    isMainline: true,
                    NPCID.Guide,
                    xpReward: 55
                    //nextMissionID: MissionID.RedEyedRetribution
                )},

                {MissionID.RedEyedRetribution, () => new MissionData(
                    MissionID.RedEyedRetribution,
                    "Red Eyed Retribution",
                    "Prepare to kill the Eye of Cthulhu.",
                    [
                        [("Obtain lens OR Susp. Eye", 6), ("Obtain life crystals", 3)],
                        [("Obtain buff potions", 3), ("Obtain healing potions", 3)],
                        [("Speak to the Guide at night", 1)],

                        [("Down the Eye of Cthulhu", 1)],
                    ],
                    [new Item(ItemID.GoldCoin, Main.rand.Next(2, 4))],
                    isMainline: true,
                    NPCID.Guide,
                    xpReward: 150
                )},
                #endregion
            };
        }

        public MissionData GetMissionData(int missionId)
        {
            if (missionData.TryGetValue(missionId, out var missionDataFactory))
            {
                return missionDataFactory();
            }
            return null;
        }
    }
}