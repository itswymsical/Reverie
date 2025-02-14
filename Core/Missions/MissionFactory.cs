using System.Collections.Generic;

namespace Reverie.Core.Missions;
public static class MissionID
{
    public const int AFallingStar = 1;
}
public class MissionFactory
{
    private Dictionary<int, Func<Mission>> missionData;
    public static MissionFactory Instance => ModContent.GetInstance<MissionFactory>();
    public MissionFactory() => InitializeMissionData();

    private void InitializeMissionData()
    {
        missionData = new Dictionary<int, Func<Mission>>
        {

            {MissionID.AFallingStar, () => new Mission(
                MissionID.AFallingStar,
                "A Falling Star",
                "'Well, that's one way to make an appearance...'" +
                "\nBegin your journey in Terraria, discovering knowledge and power...",
                [
                    [("Talk to Laine", 1)],
                    [("Collect Stone", 25), ("Collect Wood", 50), ("Give Laine resources", 1)],
                    [("Obtain a Helmet", 1), ("Obtain a Chestplate", 1), ("Obtain Leggings", 1), ("Obtain better weapon", 1)],
                    [("Discover Accessories", 3), ("Mine 30 Ore", 30),("Obtain 15 bars of metal", 15)],
                    [("Clear out slimes", 6)],
                    [("Explore the Underground", 1), ("Loot items", 150)],
                    [("Clear out slimes, again", 12)],
                    [("Resume glorius looting", 100)],
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