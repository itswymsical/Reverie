using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using System;
using Terraria.ModLoader;

namespace Reverie.Core.Missions
{
    public class MissionFactory
    {
        private Dictionary<int, Func<Mission>> missionData;
        public static MissionFactory Instance => ModContent.GetInstance<MissionFactory>();
        public MissionFactory() => InitializeMissionData();

        private void InitializeMissionData()
        {
            missionData = new Dictionary<int, Func<Mission>>
            {
                #region Sideline Missions

                {MissionID.TestMission, () => new Mission(
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

                {MissionID.LostBrother, () => new Mission(
                    MissionID.LostBrother,
                    "The Merchant's Lost Brother",
                    "The Merchant opens up about his missing brother, a fellow trader who vanished years ago. " +
                    "His last known whereabouts were in these lands, trading rare goods in dangerous territories.",
                    [
                        [("Find Old Trading Receipts", 3)],
                        [("Talk to the Merchant", 1)],
                        [("Explore the Corruption/Crimson", 1)],
                        [("Talk to the Merchant's Brother", 1)],
                        [("Return to the Merchant", 1)]
                    ],
                    [new Item(ItemID.LuckyCoin), new Item(ItemID.GoldCoin, 3)],
                    isMainline: false,
                    NPCID.Merchant,
                    xpReward: 200
                )},

                {MissionID.FoolsGold, () => new Mission(
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
                    {MissionID.Reawakening, () => new Mission(
                    MissionID.Reawakening,
                    "Reawakening",
                    "Awakened from death by the power of Reverie, you must prepare for the coming darkness.",
                    [
                        // 0, Opening sequence
                        [("Talk to the Guide", 1)],
                        // 1, EOC cutscene
                        [("Prepare Yourself...", 1)],
                        // 2, Basic resource gathering
                        [("Gather Wood", 50), ("Craft/Obtain Torches", 20)],  
                        // 3, Underground preparation
                        [("Mine Iron/Lead Ore", 25), ("Craft an Anvil", 1)],
                        // 4, Combat preparation
                        [("Craft Armor Pieces", 3), ("Obtain Healing Potions", 5)],     
                        // 5, Life Crystal gathering
                        [("Find Life Crystals", 3)],     
                        // 6, Return for Mirror
                        [("Speak with the Guide", 1)],       
                        // 7, Final preparation and Eye encounter
                        [("Defeat the Eye of Cthulhu", 1)]
                    ],

                    [new Item(ItemID.SilverCoin, Main.rand.Next(38, 66)),
                     new Item(ItemID.GoldCoin, Main.rand.Next(3, 4))],
                    isMainline: true,
                    NPCID.Guide,
                    xpReward: 150
                )},
                #endregion
            };
        }

        public Mission GetMissionData(int missionId)
        {
            if (missionData.TryGetValue(missionId, out var missionDataFactory))
            {
                return missionDataFactory();
            }
            return null;
        }
    }
}