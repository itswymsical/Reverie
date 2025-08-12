using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using Reverie.Utilities;
using static Reverie.Core.Missions.Core.ObjectiveEventItem;
using static Reverie.Core.Missions.Core.ObjectiveEventNPC;

namespace Reverie.Content.Missions.Argie;

public class MissionSporeSplinter : Mission
{
    private enum Objectives
    {
        GreetArgie = 0,
        GatherResources = 1,
    }

    public MissionSporeSplinter() : base(MissionID.SporeSplinter,
        name: "Spore & Splinter",
        description: @"""Every spore needs a floor! Help me stake my claim on this flat!.""",
        objectiveList:
        [
            [("Greet Argie", 1)],
            [("Gather Glowing Mushrooms", 30), ("Gather Rope", 30), ("Gather Wood", 75)],
        ],
        rewards: [new Item(ItemID.GoldCoin, Main.rand.Next(4, 6))],
        isMainline: false,
        providerNPC: ModContent.NPCType<NPCs.Special.Argie>(), xpReward: 150)
    {
        Instance.Logger.Info($"[{Name} - {ID}] constructed");
    }

    public override void OnMissionStart()
    {
        base.OnMissionStart();
        DialogueManager.Instance.StartDialogue("Argie.Intro", 9, letterbox: true,
            music: MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}ArgiesTheme"));
    }

    public override void OnMissionComplete(Player player, bool giveRewards = true)
    {
        base.OnMissionComplete(player, giveRewards);
    }

    protected override void RegisterEventHandlers()
    {
        if (eventsRegistered) return;
        base.RegisterEventHandlers();

        OnNPCChat += OnNPCChatHandler;
        OnItemPickup += OnItemPickupHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[{Name} - {ID}] Registered event handlers");
        eventsRegistered = true;
    }

    protected override void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;

        OnNPCChat -= OnNPCChatHandler;
        OnItemPickup -= OnItemPickupHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[{Name} - {ID}] Unregistered event handlers");
        base.UnregisterEventHandlers();
    }

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.GreetArgie:
                if (npc.type == ProviderNPC)
                {
                    UpdateProgress(objective: 0, triggeringPlayer: Main.LocalPlayer);
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
            case Objectives.GatherResources:
                if (item.type == ItemID.GlowingMushroom)
                    UpdateProgress(objective: 0, amount: item.stack, triggeringPlayer: player);
                else if (item.type == ItemID.Rope)
                    UpdateProgress(objective: 1, amount: item.stack, triggeringPlayer: player);
                else if (item.IsWood())
                    UpdateProgress(objective: 2, amount: item.stack, triggeringPlayer: player);
                break;
        }
    }
}