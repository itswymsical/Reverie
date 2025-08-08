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
        base.OnMissionComplete(rewardPlayer, giveRewards);

        // For mainline missions, the next mission unlock is now handled globally!
        // Just call UnlockMission on ANY MissionPlayer - it will unlock for ALL players automatically
        var anyMissionPlayer = GetAnyActiveMissionPlayer();
        if (anyMissionPlayer != null)
        {
            anyMissionPlayer.UnlockMission(MissionID.ForgottenAges, broadcast: true);
        }

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
        OnTileInteract -= TileInteractHandler;
        OnDialogueEnd -= OnDialogueEndHandler;
        OnTileBreak -= OnTileBreakHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"Mission [Journey's Begin] Unregistered event handlers");
        base.UnregisterEventHandlers();
    }

    #endregion

    private void OnDialogueEndHandler(string dialogueKey)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.TalkToGuide:
                if (dialogueKey == "JourneysBegin.Tutorial")
                {
                    // Use new progress system - mainline mission so all players get progress
                    MissionUtils.UpdateMissionProgressForPlayers(ID, 0, 1);

                    // Give magic mirror to all players (mainline mission reward)
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        var player = Main.player[i];
                        if (player?.active == true)
                        {
                            player.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.MagicMirror, 1);
                        }
                    }

                    DialogueManager.Instance.StartDialogue("JourneysBegin.MirrorGiven", 1, zoomIn: false, letterbox: true);
                }
                break;
        }
    }

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            // Add NPC chat logic if needed
        }
    }

    private void OnItemPickupHandler(Item item, Player player)
    {
        // Handle item pickup logic if needed
    }

    private void OnItemUseHandler(Item item, Player player)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.UseMirror:
                if (item.type == ItemID.MagicMirror)
                {
                    // Use new progress system - mainline mission so all players get progress
                    MissionUtils.UpdateMissionProgressForPlayers(ID, 0, 1, player);
                    DialogueManager.Instance.StartDialogue("JourneysBegin.Mirror", 6);
                }
                break;
        }
    }

    private void TileInteractHandler(int i, int j, int type, Player player)
    {
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

                    // Use new progress system - mainline mission so all players get progress
                    MissionUtils.UpdateMissionProgressForPlayers(ID, 0, 1, player);
                }
                break;
        }
    }

    private void OnTileBreakHandler(int i, int j, int type, Player player, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
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

                    // Use new progress system - mainline mission so all players get progress
                    MissionUtils.UpdateMissionProgressForPlayers(ID, 2, 1, player);
                }
                if (TileID.Sets.Ore[type])
                {
                    // Use new progress system - mainline mission so all players get progress
                    MissionUtils.UpdateMissionProgressForPlayers(ID, 1, 1, player);
                }
                break;
        }
    }

    /// <summary>
    /// Helper method to get any active mission player for global operations
    /// </summary>
    private MissionPlayer GetAnyActiveMissionPlayer()
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true)
            {
                return player.GetModPlayer<MissionPlayer>();
            }
        }
        return null;
    }
}