using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using Terraria.DataStructures;
using static Reverie.Core.Dialogue.DialogueManager;
using static Reverie.Core.Missions.Core.ObjectiveEventItem;
using static Reverie.Core.Missions.Core.ObjectiveEventNPC;
using static Reverie.Core.Missions.Core.ObjectiveEventTile;
using static Reverie.Core.Missions.Core.ObjectiveEventPlayer;
using Reverie.Utilities;
using System;
using Reverie.Content.Projectiles.Misc;
namespace Reverie.Content.Missions;

public class MissionForgottenAges : Mission
{
    private enum Objectives
    {
        TalkToGuide = 0,
        ExploreJungle = 1,
        ExploreUnderground = 2,
        ChronicleSegment = 3
    }

    public MissionForgottenAges() : base(MissionID.ForgottenAges,
        name: "Forgotten Ages",

        description: @"""There's bound to be more of these chronicles around...""",

        objectiveList:
        [
            [("Talk to Guide", 1)],
            [("Explore the Jungle", 1)],
            [("Locate next Chronicle", 1), ("Check inside pots", 10), ("Defeat Jungle Creatures", 10)],
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

        DialogueManager.Instance.StartDialogue("ForgottenAges.FindChronicles", 4, zoomIn: false, true);
    }

    public override void OnMissionComplete(Player rewardPlayer = null, bool giveRewards = true)
    {
        base.OnMissionComplete(rewardPlayer, giveRewards);
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
        OnBiomeEnter += OnBiomeEnterHandler;
        OnNPCKill += OnNPCKillHandler;

        // Add time requirement for any player who has this mission
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true)
            {
                var eventPlayer = player.GetModPlayer<ObjectiveEventPlayer>();
                eventPlayer.AddTimeRequirement(5 * 60);
            }
        }

        ModContent.GetInstance<Reverie>().Logger.Debug($"Mission [Forgotten Ages] Registered event handlers");

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
        OnBiomeEnter -= OnBiomeEnterHandler;
        OnNPCKill -= OnNPCKillHandler;

        // Remove time requirement for all players
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true)
            {
                var eventPlayer = player.GetModPlayer<ObjectiveEventPlayer>();
                eventPlayer.RemoveTimeRequirement(5 * 60);
            }
        }

        ModContent.GetInstance<Reverie>().Logger.Debug($"Mission [Forgotten Ages] Unregistered event handlers");
        base.UnregisterEventHandlers();
    }

    #endregion

    private void OnDialogueEndHandler(string dialogueKey)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.TalkToGuide:
                if (dialogueKey == "ForgottenAges.FindChronicles")
                {
                    MissionUtils.UpdateProgressForPlayers(ID, 0, 1);
                }
                break;
        }
    }

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.ChronicleSegment:
                break;
        }
    }

    private void OnBiomeEnterHandler(Player player, BiomeType biome, int timeRequired)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.ExploreJungle:
                if (biome == BiomeType.Jungle && timeRequired == 5 * 60)
                {
                    MissionUtils.UpdateProgressForPlayers(ID, 0, 1, player);
                }
                break;
        }
    }

    private void OnNPCKillHandler(NPC npc, Player player)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.ExploreUnderground:
                if (npc.type is NPCID.ManEater or NPCID.JungleSlime or NPCID.SpikedJungleSlime || npc.TypeName.Contains("Hornet"))
                {
                    MissionUtils.UpdateProgressForPlayers(ID, 2, 1, player);
                }
                break;
        }
    }

    private void OnItemPickupHandler(Item item, Player player)
    {
    }

    private void OnItemUseHandler(Item item, Player player)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
        }
    }

    private void TileInteractHandler(int i, int j, int type, Player player)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
        }
    }

    private void OnTileBreakHandler(int i, int j, int type, Player player, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.ExploreUnderground:
                if (type == TileID.Pots && player.ZoneJungle)
                {
                    Tile tile = Main.tile[i, j];
                    int originX = i - (tile.TileFrameX / 18);
                    int originY = j - (tile.TileFrameY / 18);

                    var originPos = new Point(originX, originY);

                    if (interactedTiles.Contains(originPos)) return;

                    interactedTiles.Add(originPos);

                    MissionUtils.UpdateProgressForPlayers(ID, 1, 1, player);
                }
                break;
        }
    }
}