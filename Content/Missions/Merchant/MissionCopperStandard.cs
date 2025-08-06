using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using Reverie.Core.Dialogue;
using static Reverie.Core.Missions.Core.ObjectiveEventTile;

namespace Reverie.Content.Missions.Merchant;

public class MissionCopperStandard : Mission
{
    private enum Objectives
    {
        TalkToMerchant = 0,
        MineCopper = 1,
    }

    public MissionCopperStandard() : base(MissionID.ForgottenAges,
        name: "Copper Standard",

        description: @"""Copper's the backbone of any economy!...""",

        objectiveList:
        [
            [("Talk to Merchant", 1)],
            [("Mine Copper", 50)],
            [("Innovate with Copper", 1), ("Craft Copper Pipes", 10), ("Craft Copper-tipped Arrows", 30)],
            [("Craft Copper Spear", 1)],

        ],

        rewards: [new Item(ItemID.CopperBar, Main.rand.Next(5, 15)), new Item(ItemID.GoldCoin, Main.rand.Next(1, 3))],

        isMainline: false, NPCID.Merchant, xpReward: 80)
    {
        Reverie.Instance.Logger.Info($"[{Name} - {ID}] constructed");
    }

    protected override void RegisterEventHandlers()
    {
        base.RegisterEventHandlers();
        ObjectiveEventNPC.OnNPCChat += OnNPCChatHandler;
        ObjectiveEventItem.OnItemPickup += OnItemPickup;
        DialogueManager.OnDialogueEnd += OnDialogueEndHandler;
        OnTileBreak += OnTileBreakHandler;

    }

    protected override void UnregisterEventHandlers()
    {
        base.UnregisterEventHandlers();
        ObjectiveEventNPC.OnNPCChat -= OnNPCChatHandler;
        ObjectiveEventItem.OnItemPickup -= OnItemPickup;
        DialogueManager.OnDialogueEnd -= OnDialogueEndHandler;
        OnTileBreak -= OnTileBreakHandler;

    }

    public override void OnMissionStart()
    {
        base.OnMissionStart();
        DialogueManager.Instance.StartDialogue("Merchant.CopperStandard", 4, false, false);
    }

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {

        }
    }
    private void OnDialogueEndHandler(string dialogueKey)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.TalkToMerchant:
                if (dialogueKey == "Merchant.CopperStandard")
                    UpdateProgress(objective: 0);
                
                break;

        }
    }
    private void OnTileBreakHandler(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.MineCopper:
                if (type == TileID.Copper)
                    UpdateProgress(objective: 0);
                break;

        }
    }
    private void OnItemPickup(Item item, Player player)
    {
        if (Progress != MissionProgress.Ongoing) return;
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {

        }
    }
}
