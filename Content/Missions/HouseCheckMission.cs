using Reverie.Core.Missions;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;

namespace Reverie.Content.Missions;

public class HousingMission : Mission
{
    public HousingMission() : base(
        id: MissionID.BUILD_VALID_HOUSE,
        name: "Home Sweet Home",
        description: "Build a valid house for NPCs to live in.",
        objectiveSetData: new List<List<(string, int)>>
        {
                new List<(string, int)> { ("Build a valid house for NPCs.", 1) }
        },
        rewards: new List<Item>
        {
                new Item(ItemID.WoodenTable, 1),
                new Item(ItemID.WoodenChair, 1),
                new Item(ItemID.Torch, 10)
        },
        isMainline: true,
        npc: NPCID.Guide,
        nextMissionID: -1,
        xpReward: 50)
    { }

    // Override to handle the housing found event
    public override void OnValidHousingFound()
    {
        if (Progress == MissionProgress.Active)
        {
            // Complete the objective
            UpdateProgress(0);

            // Notify the player
            Main.NewText("You've built a valid house! Mission complete.", Color.LightGreen);

            ModContent.GetInstance<Reverie>().Logger.Info("Housing mission completed - valid house found");
        }
    }
}