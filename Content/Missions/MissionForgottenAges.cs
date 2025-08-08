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

    public override void OnMissionComplete(Player player = null, bool giveRewards = true)
    {
        base.OnMissionComplete();
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
        var eventPlayer = Main.LocalPlayer.GetModPlayer<ObjectiveEventPlayer>();
        eventPlayer.AddTimeRequirement(5 * 60);

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
        OnBiomeEnter -= OnBiomeEnterHandler;
        OnNPCKill -= OnNPCKillHandler;

        var eventPlayer = Main.LocalPlayer.GetModPlayer<ObjectiveEventPlayer>();
        eventPlayer.RemoveTimeRequirement(5 * 60);

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
                if (dialogueKey == "ForgottenAges.FindChronicles")
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

    private void OnBiomeEnterHandler(Player player, BiomeType biome, int timeRequired)
    {
        if (Progress != MissionProgress.Ongoing) return;
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.ExploreJungle:
                if (biome == BiomeType.Jungle && timeRequired == 5 * 60)
                {
                    UpdateProgress(objective: 0);
                }
                break;
        }
    }

    private void OnNPCKillHandler(NPC npc, Player player)
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var currentPlayer = Main.player[i];
            if (currentPlayer?.active != true) continue;

            var missionPlayer = currentPlayer.GetModPlayer<MissionPlayer>();
            player = currentPlayer;

            if (Progress != MissionProgress.Ongoing) return;
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.ExploreUnderground:
                    if (npc.type is NPCID.ManEater or NPCID.JungleSlime or NPCID.SpikedJungleSlime || npc.TypeName.Contains("Hornet"))
                    {
                        UpdateProgress(objective: 2);
                    }
                    break;
            }
        } }

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
        for (int i2 = 0; i2 < Main.maxPlayers; i2++)
        {
            var currentPlayer = Main.player[i2];
            if (currentPlayer?.active != true) continue;

            var missionPlayer = currentPlayer.GetModPlayer<MissionPlayer>();

            if (Progress != MissionProgress.Ongoing) return;

            if (fail) return;

            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.ExploreUnderground:
                    if (type == TileID.Pots && currentPlayer.ZoneJungle)
                    {
                        Tile tile = Main.tile[i, j];
                        int originX = i - (tile.TileFrameX / 18);
                        int originY = j - (tile.TileFrameY / 18);

                        var originPos = new Point(originX, originY);

                        if (interactedTiles.Contains(originPos)) return;

                        interactedTiles.Add(originPos);

                        UpdateProgress(objective: 1);
                    }
                    break;
            }
        }
    }
}