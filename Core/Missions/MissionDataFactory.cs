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
                    "The merchant has been searchin' for decades, he's almost given up" +
                    "\n on finding ye' old's Dirtiest Block." +
                    "\nWill you make this old fart's dream come true?",
                    [
                        [("Find the Dirtiest Block", 1)],
                    ],
                    [new Item(ItemID.DirtiestBlock)],
                    isMainline: false,
                    npc: NPCID.Merchant,
                    xpReward: 500
                )},

                {MissionID.FoolsGold, () => new MissionData(
                    MissionID.FoolsGold,
                    "Fool's Gold",
                    "'The gold market isn't thriving too well, and the I've got has bills to pay. You're bills keep in mind.'" +
                    "\nCollect copper and lead to help the merchant cultivate fools gold.",
                    [
                        [("Collect Copper", 38), ("Collect Lead", 24)],
                        [("Check in with the Merchant", 1)]
                    ],
                    [new Item(ItemID.AncientGoldHelmet), new Item(ItemID.GoldBar, Main.rand.Next(6, 9)), new Item(ItemID.SilverCoin, 75)],
                    isMainline: false,
                    NPCID.Merchant,
                    xpReward: 70
                )},

                {MissionID.ArgiesHunt, () => new MissionData(
                    MissionID.ArgiesHunt,
                    "Argie's Hunt",
                    "Ms. Argie is looking to decorate her home. Collect her favorite fungi and bring them to her",
                    [
                        [("Collect mushrooms", 10), ("Collect glowing mushrooms", 10), ("Collect amanita fungus", 3)],
                    ],
                    [new Item(ItemID.GoldCoin, 8)],
                    isMainline: false,
                    ModContent.NPCType<Argie>()
                )},
                #endregion

                #region Mainline Missions
                {MissionID.Reawakening, () => new MissionData(
                    MissionID.Reawakening,
                    "Reawakening",
                    "Speak to the guide and gather materials." +
                    "\nYou wake up from comatose greeted by the handsome and charming guide. (wait did he write that?)" +
                    "\nYou are the one appointed 'HERO', and you must do HERO things...",
                    [
                        [("Talk to the Guide", 1)],
                        [("Choose a combat path", 1)],
                        [("Attack the target dummy", 10), ("Talk to the Guide", 1)],
                        [("Kill slimes", 3), ("Obtain torches", 16)],
                        [("Craft a better pickaxe", 1), ("Obtain Iron or Lead ore", 15), ("Check in with the Guide", 1)]
                    ],

                    [new Item(ItemID.SilverBar, Main.rand.Next(5, 8)),
                     new Item(ItemID.GoldCoin, Main.rand.Next(2, 4)),
                     new Item(ItemID.SilverCoin, Main.rand.Next(18, 36))],

                    isMainline: true,
                    NPCID.Guide,
                    xpReward: 50,
                    nextMissionID: MissionID.RedEyedRetribution
                )},

                {MissionID.RedEyedRetribution, () => new MissionData(
                    MissionID.RedEyedRetribution,
                    "Red Eyed Retribution",
                    "Prepare to kill the Eye of Cthulhu.",
                    [
                        [("Collect lenses OR Suspicious Eye", 6), ("Collect life crystals", 3)],
                        [("Obtain 3 buff potions", 3), ("Collect healing potions", 3)],
                        [("Speak to the Guide at night", 1)],

                        [("Slay the Eye of Cthulhu", 1)],
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