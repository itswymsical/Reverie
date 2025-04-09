using Reverie.Core.Missions;
using System.Collections.Generic;

namespace Reverie.Content.Missions;

/// <summary>
/// Template mission demonstrating standard patterns for mission implementation.
/// </summary>
public class TemplateMission : Mission
{
    private const int MISSION_ID = 999;
    private const int REQUIRED_ITEM_TYPE = ItemID.IronOre;
    private const int REQUIRED_NPC_TYPE = NPCID.BlueSlime;


    public TemplateMission() : base(
        id: MISSION_ID,
        name: "Template Mission",
        description: "This is a template mission showing how to create missions.",
        objectiveSetData:
        [
            // First objective set - collect items
            [
                ("Collect Iron Ore", 10),
                ("Craft Iron Bar", 3)
            ],
            // Second objective set - defeat NPCs
            [
                ("Defeat Blue Slimes", 5)
            ]
        ],
        rewards:
        [
            new Item(ItemID.GoldCoin, 2),
            new Item(ItemID.SilverCoin, 50)
        ],
        isMainline: false,
        providerNPC: NPCID.Guide,
        nextMissionID: -1, // No follow-up mission
        xpReward: 100)
    {
    }
    internal enum Objectives // Useful for tracking progress w/ switch statements
    {
        Objective1 = 0,
        Objective2 = 1,
        Objective3 = 2
    }

    #region Event Registration
    // Set this before implementing event logic
    protected override void RegisterEventHandlers()
    {
        if (eventsRegistered) return;

        ObjectiveEventItem.OnItemPickup += OnItemPickup;
        ObjectiveEventNPC.OnNPCKill += OnNPCKill;

        eventsRegistered = true;
    }

    // Override the event unregistration method to clean up
    protected override void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;

        ObjectiveEventItem.OnItemPickup -= OnItemPickup;
        ObjectiveEventNPC.OnNPCKill -= OnNPCKill;

        ModContent.GetInstance<Reverie>().Logger.Info($"Mission {Name} unregistered event handlers");

        eventsRegistered = false;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Event handlers, check <see cref="'ObjectiveEventHandlers.cs'" /> for all event handler delegates
    /// </summary>
    /// <param name="item"></param>
    /// <param name="player"></param>
    private void OnItemPickup(Item item, Player player)
    {
        if (Progress != MissionProgress.Active) return;

        // First objective set - checking if it's the right item types
        if (CurrentIndex == 0) // I prefer to use switch case statements but if statements work too
        {
            if (item.type == REQUIRED_ITEM_TYPE)
            {
                UpdateProgress(0, item.stack);
            }
            else if (item.type == ItemID.IronBar)
            {
                UpdateProgress(1, item.stack);
            }
        }
    }

    // Event handler for NPC kills
    private void OnNPCKill(NPC npc)
    {
        if (Progress != MissionProgress.Active) return;

        if (CurrentIndex == 1)
        {
            // First objective in the set (Defeat Blue Slimes)
            if (npc.type == REQUIRED_NPC_TYPE)
            {
                // Update progress for the NPC kill objective
                UpdateProgress(0, 1);
            }
        }
    }

    #endregion

    #region Mission Lifecycle

    // Override this to add custom logic when the mission starts
    public override void OnMissionStart()
    {
        base.OnMissionStart(); // By default, this calls RegisterEventHandlers()
    }

    // Override this to add custom logic when the mission completes
    public override void OnMissionComplete(bool giveRewards = true)
    {
        // Call base to give rewards and show notification
        // By default, this calls UnregisterEventHandlers()
        base.OnMissionComplete(giveRewards);

        // You can add custom logic after completion
        // For example, unlocking a follow-up mission if NextMissionID is valid
        if (NextMissionID > 0)
        {
            var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            missionPlayer.UnlockMission(NextMissionID);
        }
    }

    #endregion

    #region Objective Completion Events

    // Override this to add custom logic when an objective set completes
    protected override void OnObjectiveIndexComplete(int setIndex, ObjectiveSet set)
    {

    }

    // Override this to add custom logic when a specific objective completes
    protected override void OnObjectiveComplete(int objectiveIndex)
    {

    }

    #endregion

}