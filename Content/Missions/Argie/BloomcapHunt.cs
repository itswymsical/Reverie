using Reverie.Content.Items;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;

namespace Reverie.Content.Missions.Argie;

public class BloomcapHunt : Mission
{
    public BloomcapHunt() : base(MissionID.BloomcapHunt,
      "Bloomcap Hunt",
      @"""GIMME BLOOMCAPS! My stump will look so fancy!.""",
      [
        [("Collect Bloomcaps", 12)],
        [("Return to Argie", 1)]
      ],

      [new Item(ItemID.SilverCoin, Main.rand.Next(50, 75)), new Item(ItemID.HealingPotion, 3), new Item(ItemID.PlatinumBar, 8)],
      isMainline: false,
      ModContent.NPCType<NPCs.WorldNPCs.Argie>(),
      xpReward: 100)
    {
        ModContent.GetInstance<Reverie>().Logger.Info("[BloomcapHunt Hunt] Mission constructed");
    }

    internal enum Objectives
    {
        CollectBloomcaps = 0,
        Return = 1,
    }

    #region Event Registration

    protected override void RegisterEventHandlers()
    {
        if (eventsRegistered) return;

        base.RegisterEventHandlers();

        // Register only the events this mission cares about
        ObjectiveEventItem.OnItemPickup += OnItemPickupHandler;
        ObjectiveEventNPC.OnNPCChat += OnNPCChatHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[BloomcapHunt] Registered event handlers");
        eventsRegistered = true;
    }

    protected override void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;

        // Unregister all event handlers
        ObjectiveEventItem.OnItemPickup -= OnItemPickupHandler;
        ObjectiveEventNPC.OnNPCChat -= OnNPCChatHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[BloomcapHunt] Unregistered event handlers");
        base.UnregisterEventHandlers();
    }

    #endregion

    #region Event Handlers
    private void OnItemPickupHandler(Item item, Player player)
    {
        if (Progress != MissionProgress.Active) return;

        if ((Objectives)CurrentIndex == Objectives.CollectBloomcaps
            && item.type == ModContent.ItemType<BloomcapItem>())
        {
            UpdateProgress(0, item.stack);

            if (Objective[CurrentIndex].Objectives[0].CurrentCount == 1)
            {
                DialogueManager.Instance.StartDialogueByKey(
                    NPCManager.Default,
                    DialogueKeys.Argie.BloomcapCollected,
                    lineCount: 2,
                    zoomIn: true);
            }

            if (Objective[CurrentIndex].Objectives[0].CurrentCount == 4)
            {
                DialogueManager.Instance.StartDialogueByKey(
                    NPCManager.Default,
                    DialogueKeys.Argie.BloomcapCollectedHalf,
                    lineCount: 1,
                    zoomIn: true);
            }
        }
    }

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        if (Progress != MissionProgress.Active) return;

        if ((Objectives)CurrentIndex == Objectives.Return
            && npc.type == ModContent.NPCType<NPCs.WorldNPCs.Argie>())
        {
            UpdateProgress(0);
        }
    }

    #endregion

    public override void OnMissionStart()
    {
        base.OnMissionStart();

        DialogueManager.Instance.StartDialogueByKey(
            NPCManager.Default,
            DialogueKeys.Argie.BloomcapIntro,
            lineCount: 5,
            zoomIn: true);
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);

        DialogueManager.Instance.StartDialogueByKey(
            NPCManager.Default,
            DialogueKeys.Argie.BloomcapComplete,
            lineCount: 4,
            zoomIn: true);
    }

    protected override void OnObjectiveComplete(int objectiveIndex)
    {
        base.OnObjectiveComplete(objectiveIndex);
        if (objectiveIndex == 0)
        {
            DialogueManager.Instance.StartDialogueByKey(
                NPCManager.Default,
                DialogueKeys.Argie.BloomcapCollectedAll,
                lineCount: 2,
                zoomIn: true);
        }
    }
}