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

                {MissionID.CrashLanding, () => new Mission(
                    MissionID.CrashLanding,
                    "Falling Star",
                    "Awakened from death by the power of Reverie, you must prepare for the coming darkness.",
                    [
                        [("Talk to Laine", 1)],
                        [("Collect Stone", 25), ("Collect Wood", 50), ("Collect Potions", 8)],
                        [("Give Laine resources", 1)],
                        [("Obtain a Helmet", 1), ("Obtain a Chestplate", 1), ("Obtain Greaves", 1), ("Obtain Better Weapon", 1)],
                        [("Clear Slimes", 6)],
                        [("Explore the Underground", 1), ("Find Loot", 15)],
                        [("Clear Slimes again", 20)],     
                        [("Find More Loot", 15)],
                        [("Return to Laine", 1)],
                        [("Clear Slime Infestation", 50)],
                        [("Defeat the King Slime", 1)]
                    ],

                    [new Item(ItemID.MagicMirror), new Item(ItemID.GoldCoin, Main.rand.Next(3, 4))],
                    isMainline: true,
                    NPCID.Guide,
                    xpReward: 80
                )},
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