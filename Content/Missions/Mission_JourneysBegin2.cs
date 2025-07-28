using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using Terraria.DataStructures;
using static Reverie.Core.Dialogue.DialogueManager;
using static Reverie.Core.Missions.Core.ObjectiveEventItem;
using static Reverie.Core.Missions.Core.ObjectiveEventNPC;
using static Reverie.Core.Missions.Core.ObjectiveEventTile;

namespace Reverie.Content.Missions;

public class Mission_JourneysBegin2 : Mission
{
    private enum Objectives
    {
        TalkToGuide = 0,
        ExploreJungle = 1,
        ExploreUnderground = 2,
        ChronicleSegment = 3
    }

    public Mission_JourneysBegin2() : base(MissionID.JourneysBegin_Part2,
        name: "Journey's Begin - Continued",

        description: @"""There's bound to be more of these around...""",

        objectiveList:
        [
            [("Talk to Guide", 1)],
            [("Explore the Jungle", 1)],
            [("Locate next Chronicle", 1), ("Break pots", 20), ("Defeat Man Eaters", 20)],
            [("Give Guide Chronicle", 1), ("Listen to Guide", 1)]
        ],

        rewards: [new Item(ItemID.LesserHealingPotion, Main.rand.Next(5, 10)), new Item(ItemID.GoldCoin, Main.rand.Next(2, 5))],

        isMainline: true, NPCID.Guide, xpReward: 50)
    {
        Reverie.Instance.Logger.Info($"[{Name} - {ID}] constructed");
    }

    public override void OnMissionStart()
    {
        base.OnMissionStart();

        DialogueManager.Instance.StartDialogue("JourneysBegin2.FindChronicles", 1, zoomIn: true, false);
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);


        //MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        //player.UnlockMission(MissionID.JourneysBegin_Part2);
    }

    public override void Update()
    {
        base.Update();

        Main.slimeRain = false;
        Main.slimeRainTime = 0;
        Main.bloodMoon = false;
    }

    #region Event Registration
    protected override void RegisterEventHandlers()
    {
        if (eventsRegistered) return;
        base.RegisterEventHandlers();

        OnNPCChat += OnNPCChatHandler;
        OnItemUse += OnItemUseHandler;
        OnItemPickup += OnItemPickupHandler;
        OnTileInteract += TileInteractHandler;
        OnDialogueEnd += OnDialogueEndHandler;
        OnTileBreak += OnTileBreakHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"Mission [Journey's Begin] Registered event handlers");

        eventsRegistered = true;
    }

    protected override void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;

        OnNPCChat -= OnNPCChatHandler;
        OnItemUse -= OnItemUseHandler;
        OnItemPickup -= OnItemPickupHandler;
        OnTileInteract += TileInteractHandler;
        OnDialogueEnd -= OnDialogueEndHandler;
        OnTileBreak -= OnTileBreakHandler;


        ModContent.GetInstance<Reverie>().Logger.Debug($"Mission [Journey's Begin] Unregistered event handlers");
        base.UnregisterEventHandlers();
    }

    #endregion

    private void OnDialogueEndHandler(string dialogueKey)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.TalkToGuide:
                if (dialogueKey == "JourneysBegin2.FindChronicles")
                {
                    UpdateProgress(objective: 0);
                }
                break;
        }
    }

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.ChronicleSegment:

                break;
        }
    }

    private void OnItemPickupHandler(Item item, Player player)
    {

    }

    private void OnItemUseHandler(Item item, Player player)
    {
        if (Progress != MissionProgress.Ongoing) return;
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {

        }
    }

    private void TileInteractHandler(int i, int j, int type)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {

        }
    }

    private void OnTileBreakHandler(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (Progress != MissionProgress.Ongoing) return;

        if (fail) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
  
        }
    }
}