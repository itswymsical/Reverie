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

    public override void OnMissionComplete(Player rewardPlayer = null, bool giveRewards = true)
    {
        base.OnMissionComplete(rewardPlayer, giveRewards);
    }

    public override void Update()
    {
        base.Update(); //prevent events from messing up the flow
    }

    #region Event Registration
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

    #endregion

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.GreetArgie:
                if (npc.type == ProviderNPC)
                {
                    var chattingPlayer = GetPlayerNearNPC(npc);
                    if (chattingPlayer != null)
                    {
                        MissionUtils.UpdateProgressForPlayers(ID, 0, 1, chattingPlayer);
                    }
                }
                break;
        }
    }

    private void OnItemPickupHandler(Item item, Player player)
    {
        var objective = (Objectives)CurrentIndex;

        switch (objective)
        {
            case Objectives.GatherResources:
                if (item.type == ItemID.GlowingMushroom)
                {
                    MissionUtils.UpdateProgressForPlayers(ID, 0, item.stack, player);
                }
                else if (item.type == ItemID.Rope)
                {
                    MissionUtils.UpdateProgressForPlayers(ID, 1, item.stack, player);
                }
                else if (item.IsWood())
                {
                    MissionUtils.UpdateProgressForPlayers(ID, 2, item.stack, player);
                }
                break;
        }
    }

    private Player GetPlayerNearNPC(NPC npc)
    {
        float closestDistance = float.MaxValue;
        Player closestPlayer = null;

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true)
            {
                float distance = Vector2.Distance(player.Center, npc.Center);
                if (distance < closestDistance && distance < 200f) // Within chat range
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
        }

        return closestPlayer;
    }
}