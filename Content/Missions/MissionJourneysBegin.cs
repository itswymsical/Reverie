using Reverie.Content.Cutscenes;
using Reverie.Content.NPCs.Enemies.Assailants;
using Reverie.Core.Cinematics;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Utilities;
using Terraria.DataStructures;
using static Reverie.Core.Dialogue.DialogueManager;
using static Reverie.Core.Missions.ObjectiveEventItem;
using static Reverie.Core.Missions.ObjectiveEventNPC;
using static Reverie.Core.Missions.ObjectiveEventPlayer;
using static Reverie.Core.Missions.ObjectiveEventTile;

namespace Reverie.Content.Missions;

public class MissionJourneysBegin : Mission
{
    private bool died = false;
    private bool survived = false;
    private bool hasPlayedOpeningCutscene = false;
    private bool hasSpawnedAssailants = false;
    private bool hasPlayedChronicleEscape = false;
    private Vector2 corpsePosition;

    private enum Objectives
    {
        TalkToGuide = 0,
        Basics = 1,
        FirstNight = 2,
        TheSurge = 3,
        DefendYourself = 4,
        ReturnToGuide = 5

    }

    #region Setup
    public MissionJourneysBegin() : base(MissionID.JourneysBegin,
        name: "Journey's Begin",

        description: @"""Well, that's one way to make an appearance...""",

        objectiveList:
        [
            // 0 "Talk to Guide"
            [("Talk to Guide", 1)],
            // 1 "Basics" (Tutorial)
            [("Gather Wood", 20), ("Craft a Work Bench", 1), ("Craft a Wooden Sword", 1)],
            // 2 "First Night"
            [("Survive until dawn", 1), ("Defeat Enemies", 5), ("Break Pots", 20)],
            // 3 "The Surge"
            [("Explore the Forest", 1)],
            // 4 "Defend Yourself" Encounter Assailant Scouts (cutscene)
            [("Defeat Grunts", 3)],
            [("Return to Guide", 1)]
        ],

        rewards: [new Item(ItemID.RegenerationPotion), new Item(ItemID.LesserHealingPotion, Main.rand.Next(5, 10)), new Item(ItemID.GoldCoin, Main.rand.Next(1, 2))],

        isMainline: true, NPCID.Guide, xpReward: 50)
    {
        Reverie.Instance.Logger.Info($"[{Name} - {ID}] constructed");
        this.PauseWorldEvents = true;

    }

    public override void OnMissionStart()
    {
        base.OnMissionStart();
        Main.time = 14400;
        DialogueManager.Instance.StartDialogue("JourneysBegin.Tutorial", 3, zoomIn: false, true, music: MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}GuidesTheme"));
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);

        //MissionWorld.Instance.UnlockMission(MissionID.ForgottenAges);

        DialogueManager.Instance.StartDialogue("JourneysBegin.MissionEnd", 2, zoomIn: false, false);
    }

    protected override void RegisterEventHandlers()
    {
        if (eventsRegistered) return;

        base.RegisterEventHandlers();

        OnItemCreated += OnCraftItem;
        OnItemUse += UseItem;
        OnItemPickup += PickupItem;

        OnTileBreak += BreakPots;
        OnNPCKill += OnKill;

        OnDialogueEnd += OnDialogueFinished;
        OnBiomeEnter += WhileInBiome;
        var eventPlayer = Main.LocalPlayer.GetModPlayer<ObjectiveEventPlayer>();
        eventPlayer.AddTimeRequirement(300);

        ModContent.GetInstance<Reverie>().Logger.Debug($"Mission [Journey's Begin] Registered event handlers");

        eventsRegistered = true;
    }


    protected override void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;

        OnItemCreated -= OnCraftItem;
        OnItemUse -= UseItem;
        OnItemPickup -= PickupItem;

        OnTileBreak -= BreakPots;
        OnNPCKill -= OnKill;

        OnDialogueEnd -= OnDialogueFinished;
        OnBiomeEnter -= WhileInBiome;

        ModContent.GetInstance<Reverie>().Logger.Debug($"Mission [Journey's Begin] Unregistered event handlers");

        base.UnregisterEventHandlers();
    }
    #endregion

    public override void Update()
    {
        base.Update();

        if (Main.LocalPlayer.ZoneForest)
        {
            if (Main.dayTime && !Main.raining && !Main.slimeRain && !Main.LocalPlayer.ShoppingZone_AnyBiome)
            {
                Main.musicBox2 = MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}GuidesTheme");
            }
            else
            {
                Main.musicBox2 = default;
            }
        }
        if (Main.LocalPlayer.dead && !died)
        {
            died = true;
            ObjectiveList[CurrentList].Objective[0].Description = "...make it to the next day."; // lol
        }
        
        UpdateTime();
        if (Main.LocalPlayer.isNearNPC(NPCID.Guide, 60f))
        {
            if ((Objectives)CurrentList == Objectives.ReturnToGuide)
            {
                UpdateProgress(objective: 0);
            }
        }
    }

    /// <summary>
    /// Check day-night cycle for objectives
    /// </summary>
    private void UpdateTime()
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentList;
        switch (objective)
        {
            case Objectives.FirstNight:
                if (Main.time == 1 && !survived)
                {
                    UpdateProgress(objective: 0);
                    survived = true;
                }
                break;
        }
    }

    protected override void OnObjectiveIndexComplete(int setIndex, ObjectiveList completedSet)
    {
        base.OnObjectiveIndexComplete(setIndex, completedSet);

        if (setIndex == (int)Objectives.Basics)
            DialogueManager.Instance.StartDialogue("JourneysBegin.BasicsDone", 8, zoomIn: false, letterbox: false, music: MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}GuidesTheme"));
    }

    private void WhileInBiome(Player player, BiomeType biome, int ticksSpent)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentList;
        switch (objective)
        {
            case Objectives.TheSurge:
                if (biome == BiomeType.Forest)
                {
                    UpdateProgress(objective: 0);
                    
                    CutsceneSystem.PlayCutscene<AmbushCutscene>();
                }
                break;
        }
    }

    private void OnCraftItem(Item item, ItemCreationContext context)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentList;
        switch (objective)
        {
            case Objectives.Basics:
                if (item.type == ItemID.WorkBench)
                {
                    UpdateProgress(objective: 1);
                }
                if (item.type == ItemID.WoodenSword)
                {
                    UpdateProgress(objective: 2);
                }
                break;
        }
    }

    private void OnKill(NPC npc)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentList;
        switch (objective)
        {
            case Objectives.FirstNight:
                if (!npc.friendly || !npc.CountsAsACritter || !npc.townNPC)
                {
                    UpdateProgress(objective: 1);
                }
                break;
            case Objectives.DefendYourself:
                if (npc.type == ModContent.NPCType<NullHerald>())
                {
                    UpdateProgress(objective: 0);
                }
                break;
        }
    }

    private void OnDialogueFinished(string dialogueKey)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentList;
        switch (objective)
        {
            case Objectives.TalkToGuide:
                if (dialogueKey == "JourneysBegin.Tutorial")
                {
                    UpdateProgress(objective: 0);

                    Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.MagicMirror, 1);
                    DialogueManager.Instance.StartDialogue("JourneysBegin.Basics", 2, zoomIn: false, letterbox: true, music: MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}GuidesTheme"));
                }
                break;
        }
    }

    private void PickupItem(Item item, Player player)
    {
        if (Progress != MissionProgress.Ongoing) return;
        var objective = (Objectives)CurrentList;
        switch (objective)
        {
            case Objectives.Basics:
                if (item.type == ItemID.Wood)
                {
                    UpdateProgress(objective: 0, item.stack);
                }
                break;
        }
    }

    private void UseItem(Item item, Player player)
    {
        if (Progress != MissionProgress.Ongoing) return;
        var objective = (Objectives)CurrentList;
        switch (objective)
        {
            case Objectives.Basics:
                // to be implemented
                break;
        }
    }

    private void BreakPots(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (Progress != MissionProgress.Ongoing) return;

        if (fail) return;

        var objective = (Objectives)CurrentList;
        switch (objective)
        {
            case Objectives.FirstNight:
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

    // use this for the corpse tile

    //private void TileInteractHandler(int i, int j, int type)
    //{
    //    if (Progress != MissionProgress.Ongoing) return;

    //    var objective = (Objectives)CurrentList;
    //    switch (objective)
    //    {
    //        case Objectives.ExploreUnderground:
    //            if (type == TileID.Containers || type == TileID.Containers2)
    //            {
    //                // Find the top-left origin of the 2x2 container
    //                Tile tile = Main.tile[i, j];
    //                int originX = i - (tile.TileFrameX / 18);
    //                int originY = j - (tile.TileFrameY / 18);

    //                var originPos = new Point(originX, originY);

    //                // Skip if this container was already interacted with
    //                if (interactedTiles.Contains(originPos)) return;

    //                // Mark all 4 tiles of the 2x2 container as interacted
    //                for (int x = 0; x < 2; x++)
    //                {
    //                    for (int y = 0; y < 2; y++)
    //                    {
    //                        interactedTiles.Add(new Point(originX + x, originY + y));
    //                    }
    //                }

    //                UpdateProgress(objective: 0);
    //            }
    //            break;
    //    }
    //}
}