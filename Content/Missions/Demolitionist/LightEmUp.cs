using Reverie.Core.Dialogue;
using Reverie.Core.Missions;

namespace Reverie.Content.Missions.Demolitionist;

public class LightEmUp : Mission
{
    private enum Objectives
    {
        CollectTorches = 0,
        PlaceTorches = 1,
        TGFavor = 2,
        CheckIn = 3
    }

    public LightEmUp() : base(
        id: MissionID.LightEmUp,
        name: "Light 'Em Up",
        description: @"""Bah! Light is supposed to help man, not attack him!""",
        objectiveList: [
            [("Collect torches", 101), ],
            [("Place torches", 101)],
            [("Gain the Torch God's Favor", 1)],
            [("Check in with Demolitionist", 1)]
        ],
        rewards: [new Item(ItemID.GoldCoin, 2), new Item(ItemID.SpelunkerGlowstick, 25)],
        isMainline: false,
        providerNPC: NPCID.Demolitionist,
        xpReward: 50)
    {
        Instance.Logger.Info("[Light 'Em Up] Mission constructed");
    }

    #region Mission Lifecycle
    public override void OnMissionStart()
    {
        base.OnMissionStart(); // By default, this calls RegisterEventHandlers()
        DialogueManager.Instance.StartDialogueByKey(
               NPCManager.DemolitionistData,
               DialogueKeys.Demolitionist.TorchGodStart,
               lineCount: 6);
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);
        DialogueManager.Instance.StartDialogueByKey(
           NPCManager.DemolitionistData,
           DialogueKeys.Demolitionist.TorchGodComplete,
           lineCount: 4);
    }
    #endregion

    #region Event Registration
    protected override void RegisterEventHandlers()
    {
        if (eventsRegistered) return;

        base.RegisterEventHandlers();

        ObjectiveEventItem.OnItemPickup += OnItemPickupHandler;
        ObjectiveEventItem.OnItemUpdate += OnItemUpdateHandler;
        ObjectiveEventTile.OnTilePlace += OnTilePlaceHandler;
        ObjectiveEventNPC.OnNPCChat += OnNPCChatHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[Light 'Em Up] Registered event handlers");

        eventsRegistered = true;
    }

    protected override void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;

        ObjectiveEventItem.OnItemPickup -= OnItemPickupHandler;
        ObjectiveEventItem.OnItemUpdate -= OnItemUpdateHandler;
        ObjectiveEventTile.OnTilePlace -= OnTilePlaceHandler;
        ObjectiveEventNPC.OnNPCChat -= OnNPCChatHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[Light 'Em Up] Unregistered event handlers");

        base.UnregisterEventHandlers();
    }

    #endregion

    #region Event Handlers
    private void OnItemPickupHandler(Item item, Player player)
    {
        if (Progress != MissionProgress.Active) return;

        if (CurrentIndex == (int)Objectives.CollectTorches)
            if (ItemID.Sets.Torches[item.type])
                UpdateProgress(0, item.stack);
    }

    private void OnItemUpdateHandler(Item item, Player player)
    {
        if (Progress != MissionProgress.Active) return;

        if (CurrentIndex == (int)Objectives.TGFavor)
            if (item.type == ItemID.TorchGodsFavor)
                UpdateProgress(0, 1);
    }

    private void OnTilePlaceHandler(int i, int j, int type)
    {
        if (Progress != MissionProgress.Active) return;

        if (CurrentIndex == (int)Objectives.PlaceTorches)
            if (TileID.Sets.Torch[type])
                UpdateProgress(0);

        TorchDialouge();
    }

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        if (Progress != MissionProgress.Active) return;

        if (CurrentIndex == (int)Objectives.CheckIn)
        {
            if (npc.type == ProviderNPC) 
                UpdateProgress(0);
        }
    }
    #endregion

    #region Helpers
    private void TorchDialouge()
    {
        if (Objective[(int)Objectives.PlaceTorches].Objectives[0].CurrentCount == 10)
        {
            DialogueManager.Instance.StartDialogueByKey(
                   NPCManager.DemolitionistData,
                   DialogueKeys.Demolitionist.TorchPlacement_01,
                   lineCount: 1);
        }
        if (Objective[(int)Objectives.PlaceTorches].Objectives[0].CurrentCount == 25)
        {
            DialogueManager.Instance.StartDialogueByKey(
                   NPCManager.DemolitionistData,
                   DialogueKeys.Demolitionist.TorchPlacement_02,
                   lineCount: 1);
        }
        if (Objective[(int)Objectives.PlaceTorches].Objectives[0].CurrentCount == 50)
        {
            DialogueManager.Instance.StartDialogueByKey(
                   NPCManager.DemolitionistData,
                   DialogueKeys.Demolitionist.TorchPlacement_03,
                   lineCount: 1);
        }
        if (Objective[(int)Objectives.PlaceTorches].Objectives[0].CurrentCount == 100)
        {
            DialogueManager.Instance.StartDialogueByKey(
                   NPCManager.DemolitionistData,
                   DialogueKeys.Demolitionist.TorchPlacement_04,
                   lineCount: 1);
        }
    }
    #endregion
}