using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using static Reverie.Core.Missions.Core.ObjectiveEventNPC;

namespace Reverie.Content.Missions;

public class Mission_JourneysBegin : Mission
{
    private enum Objectives
    {
        TalkToGuide = 0,
        GatherResources = 1,
    }

    public Mission_JourneysBegin() : base(MissionID.JourneysBegin,
        name: "Journey's Begin",

        description: @"""Well, that's one way to make an appearance...""",

        objectiveList: 
        [ 
            [("Talk to Guide", 1)],
        ],

        rewards: [new Item(ItemID.RegenerationPotion), new Item(ItemID.IronskinPotion), new Item(ItemID.GoldCoin, Main.rand.Next(4, 6))],

        isMainline: true, NPCID.Guide, xpReward: 100)
    {
        Instance.Logger.Info($"[{Name} - {ID}] constructed");
    }

    public override void OnMissionStart()
    {
        base.OnMissionStart(); // This calls RegisterEventHandlers()
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);

        //MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        //player.StartNextMission(...);
    }

    public override void Update()
    {
        base.Update(); //prevent events from messing up the flow

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

        ModContent.GetInstance<Reverie>().Logger.Debug($"Mission [Journey's Begin] Registered event handlers");

        eventsRegistered = true;
    }

    protected override void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;

        OnNPCChat -= OnNPCChatHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"Mission [Journey's Begin] Unregistered event handlers");
        base.UnregisterEventHandlers();
    }

    #endregion

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        if (Progress != MissionProgress.Ongoing) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.TalkToGuide:
                if (npc.type == NPCID.Guide)
                {
                    UpdateProgress(0);
                }
                break;
        }
    }
}
