using Reverie.Core.Missions;
using System.Collections.Generic;

namespace Reverie.Content.Missions;

public class ExampleMission : Mission
{
    public ExampleMission() : base(
        MissionID.JourneysBegin,
        name: "Journey's Begin",
        description: "Complete basic tasks to get started",
        objectiveList:
        [
            // Set 0: Kill slimes and mine dirt
            new()
            {
                ("Kill 5 slimes", 5),
                ("Mine 10 dirt blocks", 10)
            },
            // Set 1: Craft and place torches
            new()
            {
                ("Craft 20 torches", 20),
                ("Place 10 torches", 10)
            }
        ],
        rewards: new List<Item>
        {
            new(ItemID.IronPickaxe),
            new(ItemID.LesserHealingPotion, 5)
        },
        isMainline: true,
        providerNPC: NPCID.Guide,
        xpReward: 100
    )
    {
    }

    /// <summary>
    /// Declarative event matching - describe what events matter for each objective.
    /// </summary>
    public override bool MatchesEvent(MissionEvent evt, int currentSet, int objectiveIndex)
    {
        // Set 0, Objective 0: Kill 5 slimes
        if (currentSet == 0 && objectiveIndex == 0)
        {
            return evt.Type == MissionEventType.NPCKill
                && evt.NPCType.HasValue
                && (evt.NPCType == NPCID.BlueSlime
                    || evt.NPCType == NPCID.GreenSlime
                    || evt.NPCType == NPCID.RedSlime);
        }

        // Set 0, Objective 1: Mine 10 dirt blocks
        if (currentSet == 0 && objectiveIndex == 1)
        {
            return evt.Type == MissionEventType.TileBreak
                && evt.TileType == TileID.Dirt;
        }

        // Set 1, Objective 0: Craft 20 torches
        if (currentSet == 1 && objectiveIndex == 0)
        {
            return evt.Type == MissionEventType.ItemCraft
                && evt.ItemType == ItemID.Torch;
        }

        // Set 1, Objective 1: Place 10 torches
        if (currentSet == 1 && objectiveIndex == 1)
        {
            return evt.Type == MissionEventType.TilePlace
                && evt.TileType == TileID.Torches;
        }

        return false;
    }

    // Optional: Override OnMatchedEvent for custom behavior
    // Default behavior (increment by evt.Amount) is usually fine
    public override bool OnMatchedEvent(MissionEvent evt, int objectiveIndex)
    {
        // Example: Special handling for crafting torches in bulk
        if (evt.Type == MissionEventType.ItemCraft && evt.ItemType == ItemID.Torch)
        {
            // Torches craft in stacks - use full stack amount
            return UpdateProgress(objectiveIndex, evt.Item.stack);
        }

        // Default behavior for everything else
        return base.OnMatchedEvent(evt, objectiveIndex);
    }
}