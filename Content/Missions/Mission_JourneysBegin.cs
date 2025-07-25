using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using Terraria;
using Terraria.DataStructures;
using static Reverie.Core.Dialogue.DialogueManager;
using static Reverie.Core.Missions.Core.ObjectiveEventItem;
using static Reverie.Core.Missions.Core.ObjectiveEventNPC;
using static Reverie.Core.Missions.Core.ObjectiveEventTile;

namespace Reverie.Content.Missions;

public class Mission_JourneysBegin : Mission
{
    private enum Objectives
    {
        TalkToGuide = 0,
        UseMirror = 1,
        ExploreUnderground = 2,
        ReturnToGuide = 3
    }

    public Mission_JourneysBegin() : base(MissionID.JourneysBegin,
        name: "Journey's Begin",

        description: @"""Well, that's one way to make an appearance...""",

        objectiveList: 
        [ 
            [("Talk to Guide", 1)],
            [("Use Magic Mirror", 1)],
            [("Break pots", 20), ("Loot chests", 5)],
            [("Return to Guide", 1)]
        ],

        rewards: [new Item(ItemID.RegenerationPotion), new Item(ItemID.IronskinPotion), new Item(ItemID.GoldCoin, Main.rand.Next(4, 6))],

        isMainline: true, NPCID.Guide, xpReward: 100)
    {
        Reverie.Instance.Logger.Info($"[{Name} - {ID}] constructed");
    }

    public override void OnMissionStart()
    {
        base.OnMissionStart();

        DialogueManager.Instance.StartDialogue("JourneysBegin.Tutorial", 6, zoomIn: true, false);
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);

        //MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        //player.StartNextMission(...);
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
                if (dialogueKey == "JourneysBegin.Tutorial")
                {
                    UpdateProgress(0);
                    player.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.MagicMirror, 1);
                    DialogueManager.Instance.StartDialogue("JourneysBegin.MirrorGiven", 1, zoomIn: false, letterbox: false);
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
            case Objectives.ReturnToGuide:
                if (npc.type == NPCID.Guide)
                {
                    UpdateProgress(0);
                    //DialogueManager.Instance.StartDialogue("JourneysBegin.Tutorial", 7, zoomIn: true);
                }
                break;
        }
    }

    private void OnItemPickupHandler(Item item, Player player)
    {
        //if (Progress != MissionProgress.Ongoing) return;
        //var objective = (Objectives)CurrentIndex;
        //switch (objective)
        //{


        //}
    }

    private void OnItemUseHandler(Item item, Player player)
    {
        if (Progress != MissionProgress.Ongoing) return;
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.UseMirror:
                if (item.type == ItemID.MagicMirror)
                {
                    UpdateProgress(0);
                    DialogueManager.Instance.StartDialogue("JourneysBegin.Mirror", 6);
                }
                break;
        }
    }

    private void TileInteractHandler(int i, int j, int type)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.ExploreUnderground:
                if (type == TileID.Containers || type == TileID.Containers2)
                {
                    // Find the top-left origin of the 2x2 container
                    Tile tile = Main.tile[i, j];
                    int originX = i - (tile.TileFrameX / 18);
                    int originY = j - (tile.TileFrameY / 18);

                    var originPos = new Point(originX, originY);

                    // Skip if this container was already interacted with
                    if (interactedTiles.Contains(originPos)) return;

                    // Mark all 4 tiles of the 2x2 container as interacted
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            interactedTiles.Add(new Point(originX + x, originY + y));
                        }
                    }

                    UpdateProgress(1);
                }
                break;
        }
    }

    private void OnTileBreakHandler(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (Progress != MissionProgress.Ongoing) return;

        if (fail) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.ExploreUnderground:
                if (type == TileID.Pots)
                {
                    // Find the top-left origin of the 2x2 pot
                    Tile tile = Main.tile[i, j];
                    int originX = i - (tile.TileFrameX / 18);
                    int originY = j - (tile.TileFrameY / 18);

                    var originPos = new Point(originX, originY);

                    // Skip if this pot was already counted
                    if (interactedTiles.Contains(originPos)) return;

                    // Mark this pot as counted (prevents 4x counting from 2x2 break)
                    interactedTiles.Add(originPos);

                    UpdateProgress(0); // First objective in the set (Break pots)
                }
                break;
        }
    }
}
