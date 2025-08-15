using Reverie.Content.Items.Mycology;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
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

    public PuffballHunt() : base(MissionID.PuffballHunt,
        name: "Spore & Splinter",
        description: @"""I am quite marvelled by the puffball mushrooms in this area...""",
        objectiveList:
        [
            [("Speak with Argie", 1)],
            [("Search underground for Puffballs", 1)],
            [("Chase down Scrunglepuffs", 1)],
            [("Harvest Puffballs", 5)],
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
        DialogueManager.Instance.StartDialogue("Argie.PuffballHunt", 4, letterbox: true,
            music: MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}ArgiesTheme"));
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);
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

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.Introduction:
                if (dialogueKey == "Argie.PuffballHunt")
                {
                    UpdateProgress(objective: 0);
                }
                break;
        }
    }

    private void OnItemPickupHandler(Item item, Player player)
    {
        if (Progress != MissionProgress.Ongoing) return;
        var objective = (Objectives)CurrentIndex;

        switch (objective)
        {
            case Objectives.HarvestPuffballs:
                if (item.type == ModContent.ItemType<PuffballItem>())
                {
                    UpdateProgress(objective: 0, amount: item.stack);
                }
                break;
        }
    }
}