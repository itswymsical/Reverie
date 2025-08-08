using Reverie.Content.Items.Misc;
using Reverie.Content.Projectiles.Misc;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using Reverie.Utilities;
using Terraria.DataStructures;
using static Reverie.Core.Dialogue.DialogueManager;
using static Reverie.Core.Missions.Core.ObjectiveEventItem;
using static Reverie.Core.Missions.Core.ObjectiveEventNPC;
using static Reverie.Core.Missions.Core.ObjectiveEventTile;

namespace Reverie.Content.Missions;

public class MissionJourneysBegin : Mission
{
    private enum Objectives
    {
        TalkToGuide = 0,
        UseMirror = 1,
        ExploreUnderground = 2
    }

    public MissionJourneysBegin() : base(MissionID.JourneysBegin,
        name: "Journey's Begin",

        description: @"""Well, that's one way to make an appearance...""",

        objectiveList:
        [
            [("Talk to Guide", 1)],
            [("Use Magic Mirror", 1)],
            [("Loot chests", 3), ("Mine ore", 20), ("Break pots", 20)],
            [("Build a shelter", 1), ("Check in with Guide", 1)]
        ],

        rewards: [new Item(ItemID.RegenerationPotion), new Item(ItemID.LesserHealingPotion, Main.rand.Next(5, 10)), new Item(ItemID.GoldCoin, Main.rand.Next(1, 2))],

        isMainline: true, NPCID.Guide, xpReward: 50)
    {
        Reverie.Instance.Logger.Info($"[{Name} - {ID}] constructed");
    }

    public override void OnMissionStart()
    {
        base.OnMissionStart();

        DialogueManager.Instance.StartDialogue("JourneysBegin.Tutorial", 7, zoomIn: false, true);
    }

    public override void OnMissionComplete(Player rewardPlayer = null, bool giveRewards = true)
    {
        base.OnMissionComplete();


        MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        player.UnlockMission(MissionID.ForgottenAges);

        DialogueManager.Instance.StartDialogue("JourneysBegin.MissionEnd", 2, zoomIn: false, false);

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
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var currentPlayer = Main.player[i];
            if (currentPlayer?.active != true) continue;

            var missionPlayer = currentPlayer.GetModPlayer<MissionPlayer>();


            if (Progress != MissionProgress.Ongoing) return;

            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.TalkToGuide:
                    if (dialogueKey == "JourneysBegin.Tutorial")
                    {
                        UpdateProgress(objective: 0);
                        currentPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.MagicMirror, 1);
                        DialogueManager.Instance.StartDialogue("JourneysBegin.MirrorGiven", 1, zoomIn: false, letterbox: true);
                    }
                    break;
            }
        }
    }

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {

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

                    UpdateProgress(objective: 0);
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
                    Tile tile = Main.tile[i, j];
                    int originX = i - (tile.TileFrameX / 18);
                    int originY = j - (tile.TileFrameY / 18);

                    var originPos = new Point(originX, originY);

                    if (interactedTiles.Contains(originPos)) return;

                    interactedTiles.Add(originPos);

                    UpdateProgress(objective: 2);

                }
                if (TileID.Sets.Ore[type])
                {
                    UpdateProgress(objective: 1);
                }
                break;
        }
    }
}