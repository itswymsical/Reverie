using Reverie.Common.Items.Components;
using Reverie.Content.Items.Mycology;
using Reverie.Content.NPCs.Enemies.Underground;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Terraria;
using static Reverie.Core.Missions.ObjectiveEventItem;

namespace Reverie.Content.Missions.Argie;

public class PuffballHunt : Mission
{
    private enum Objectives
    {
        Introduction = 0,
        SearchForPuffball = 1,
        ChaseScrunglepuff = 2,
        HarvestPuffballs = 3
    }

    public static bool scrungleFound = false;
    public static bool puffFound = false;

    public PuffballHunt() : base(MissionID.PuffballHunt,
        name: "Huff n' Puff",
        description: @"""I am quite marvelled by the puffball mushrooms in this area...""",
        objectiveList:
        [
            [("Speak with Argie", 1)],
            [("Search underground for Puffballs", 1)],
            [("Chase down Scrunglepuffs", 1)],
            [("Harvest Puffballs", 8)]
        ],
        rewards: [new Item(ItemID.GoldCoin, 3), new Item(ItemID.SpelunkerPotion, 3), new Item(ItemID.HerbBag, 5)],
        isMainline: false,
        providerNPC: ModContent.NPCType<NPCs.Special.Argie>(), xpReward: 100)
    {
        Instance.Logger.Info($"[{Name} - {ID}] constructed");
    }

    public override void OnMissionStart()
    {
        base.OnMissionStart();
        scrungleFound = false;
        puffFound = false;
        DialogueManager.Instance.StartDialogue("Argie.PuffballHunt", 4, letterbox: true,
            music: MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}ArgiesTheme"));
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);
        DialogueManager.Instance.StartDialogue("Argie.PuffballHuntEnd", 2, letterbox: true);
        MissionUtils.RetrieveItemsFromPlayer(Main.LocalPlayer, ModContent.ItemType<PuffballItem>(), 8);
    }

    public override void Update()
    {
        base.Update();
        if (Progress == MissionProgress.Ongoing)
        {
            var objective = (Objectives)CurrentList;
            switch (objective)
            {
                case Objectives.SearchForPuffball:
                    CheckForActiveScrunglepuff();
                    break;

                case Objectives.ChaseScrunglepuff:
                    CheckProximityToScrunglepuff();
                    break;
            }
        }
    }

    private void CheckForActiveScrunglepuff()
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && npc.type == ModContent.NPCType<Scrunglepuff>())
            {
                if (npc.ai[0] == 1f && !scrungleFound)
                {
                    DialogueManager.Instance.StartDialogue("Argie.Scrunglepuff", 2);
                    scrungleFound = true;
                    return;
                }
                Player player = Main.LocalPlayer;
                const float TRIG_DIST = 500f;
                float distance = Vector2.Distance(player.Center, npc.Center);

                if (distance < TRIG_DIST && !puffFound)
                {
                    DialogueManager.Instance.StartDialogue("Argie.Puffball", 1);
                    puffFound = true;
                    return;
                }
            }
        }
    }

    private void CheckProximityToScrunglepuff()
    {
        Player player = Main.LocalPlayer;
        const float CHASE_DISTANCE = 150f;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && npc.type == ModContent.NPCType<Scrunglepuff>())
            {
                if (npc.ai[0] == 1f)
                {
                    float distance = Vector2.Distance(player.Center, npc.Center);
                    if (distance < CHASE_DISTANCE)
                    {
                        UpdateProgress(objective: 0);
                        return;
                    }
                }
            }
        }
    }

    protected override void RegisterEventHandlers()
    {
        if (eventsRegistered) return;
        base.RegisterEventHandlers();

        DialogueManager.OnDialogueEnd += OnDialogueEndHandler;
        OnItemPickup += OnItemPickupHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[{Name} - {ID}] Registered event handlers");
        eventsRegistered = true;
    }

    protected override void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;

        OnItemPickup -= OnItemPickupHandler;
        DialogueManager.OnDialogueEnd -= OnDialogueEndHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[{Name} - {ID}] Unregistered event handlers");
        base.UnregisterEventHandlers();
    }

    private void OnDialogueEndHandler(string dialogueKey)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentList;
        switch (objective)
        {
            case Objectives.Introduction:
                if (dialogueKey == "Argie.PuffballHunt")
                {
                    UpdateProgress(objective: 0);
                }
                break;
            case Objectives.SearchForPuffball:
                if (dialogueKey == "Argie.Scrunglepuff")
                {
                    UpdateProgress(objective: 0);
                }
                break;
        }
    }

    private void OnItemPickupHandler(Item item, Player player)
    {
        if (Progress != MissionProgress.Ongoing) return;
        var objective = (Objectives)CurrentList;

        switch (objective)
        {
            case Objectives.HarvestPuffballs:
                if (item.type == ModContent.ItemType<PuffballItem>())
                {
                    UpdateProgress(objective: 0, amount: item.stack);
                    if (!ObjectiveList[CurrentList].Objective[0].IsCompleted)
                    {
                        MissionItemComponent.MarkForMission(item);
                    }

                    if (ObjectiveList[CurrentList].Objective[0].Count == 1)
                    {
                        DialogueManager.Instance.StartDialogue("Argie.PuffballCollected", 1);
                    }
                }
                break;
        }
    }
}