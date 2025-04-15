using Reverie.Core.Dialogue;
using Reverie.Core.Missions;

namespace Reverie.Content.Missions.Demolitionist;

public class LightEmUp : Mission
{
    internal enum Objectives
    {
        CollectTorches = 0,
        PlaceTorches = 1,
        DefeatTG = 2,
    }
    public LightEmUp() : base(
        id: MissionID.LightEmUp,
        name: "Light 'Em Up",
        description: @"""I need you to take care of a flaming freak, one with too many eyes and not enough respect.""",
        objectiveList: [
            [("Collect torches", 101), ],
            [("Place torches", 101)],
            [("Defeat the Torch God", 1)]
        ],
        rewards: [new Item(ItemID.GoldCoin, 5), new Item(ItemID.SpelunkerGlowstick, 25)],
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
        ObjectiveEventNPC.OnNPCKill += OnNPCKillHandler;
        ObjectiveEventTile.OnTilePlace += OnTilePlaceHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[Light 'Em Up] Registered event handlers");

        eventsRegistered = true;
    }

    protected override void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;

        ObjectiveEventItem.OnItemPickup -= OnItemPickupHandler;
        ObjectiveEventNPC.OnNPCKill -= OnNPCKillHandler;
        ObjectiveEventTile.OnTilePlace -= OnTilePlaceHandler;

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

    private void OnTilePlaceHandler(int i, int j, int type)
    {
        if (Progress != MissionProgress.Active) return;

        if (CurrentIndex == (int)Objectives.PlaceTorches)
            if (TileID.Sets.Torch[type])
                UpdateProgress(0);

        TorchDialouge();
    }

    private void OnNPCKillHandler(NPC npc)
    {
        if (Progress != MissionProgress.Active) return;

        if (CurrentIndex == (int)Objectives.DefeatTG)
            if (npc.type == NPCID.TorchGod)
                UpdateProgress(1);
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