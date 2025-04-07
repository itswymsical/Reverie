﻿using System.Collections.Generic;

using Terraria.DataStructures;

using Reverie.Core.Dialogue;

using Reverie.Utilities;
using Reverie.Utilities.Extensions;
using System.Linq;
using Reverie.Common.UI.Missions;
using Terraria.UI;

namespace Reverie.Core.Missions;

public partial class MissionPlayer
{

    /// <summary>
    /// called in PostUpdate(), to trigger mission unlocks and events.
    /// </summary>
    public void PlayerTriggerEvents()
    {
        bool merchantPresent = NPC.AnyNPCs(NPCID.Merchant);
        var cS = GetMission(MissionID.CopperStandard);
        if (cS.Progress != MissionProgress.Completed && merchantPresent && Player.HasItemInAnyInventory(ItemID.CopperBar))
        {
            UnlockMission(MissionID.CopperStandard);
        }   
    }
}

public class ObjectiveEventItem : GlobalItem
{
    public override void OnCreated(Item item, ItemCreationContext context)
    {
        MissionManager.Instance.OnItemCreated(item, context);
        base.OnCreated(item, context);
    }

    public override bool OnPickup(Item item, Player player)
    {
        MissionUtils.TryUpdateProgressForItem(item, player);
        return base.OnPickup(item, player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        base.UpdateEquip(item, player);
        MissionUtils.TryUpdateProgressForItem(item, player);
    }

    public override void UpdateInventory(Item item, Player player)
    {
        MissionUtils.TryUpdateProgressForItem(item, player);
        base.UpdateInventory(item, player);
    }
}

public class ObjectiveEventNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    private static readonly Dictionary<int, bool> NotificationCreated = [];

    public static void ResetNotificationTracking()
    {
        NotificationCreated.Clear();
    }

    public override bool? CanChat(NPC npc)
        => (npc.isLikeATownNPC || npc.townNPC) && !DialogueManager.Instance.IsAnyActive();

    public override void GetChat(NPC npc, ref string chat)
    {
        base.GetChat(npc, ref chat);
        MissionManager.Instance.OnNPCChat(npc);
    }

    public override void OnChatButtonClicked(NPC npc, bool firstButton)
    {
        base.OnChatButtonClicked(npc, firstButton);
        if (!firstButton)
        {
            var plr = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

            var mission = plr.AvailableMissions().FirstOrDefault(m => npc.type == m.Employer);
            if (mission == null)
                return;

            plr.StartMission(mission.ID);
        }
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        var mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        var AFallingStar = mPlayer.GetMission(MissionID.AFallingStar);

        if (AFallingStar?.Progress == MissionProgress.Active)
        {
            var currentSet = AFallingStar.Objective[AFallingStar.CurrentIndex];
            if (spawnInfo.PlayerInTown && AFallingStar.CurrentIndex >= 5)
            {
                if (AFallingStar.CurrentIndex >= 3 && Main.LocalPlayer.ZoneOverworldHeight)
                {
                    pool.Add(NPCID.BlueSlime, 0.08f);
                    pool.Add(NPCID.GreenSlime, 0.102f);
                }
            }
        }
        else
            base.EditSpawnPool(pool, spawnInfo);
    }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        var mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

        var AFallingStar = mPlayer.GetMission(MissionID.AFallingStar);

        if (AFallingStar?.Progress == MissionProgress.Active && player.ZoneOverworldHeight)
        {
            var currentSet = AFallingStar.Objective[AFallingStar.CurrentIndex];
            if (AFallingStar.CurrentIndex >= 4)
            {
                spawnRate = 3;
                maxSpawns = 7;
            }
            if (AFallingStar.CurrentIndex >= 5)
            {
                spawnRate = 2;
                maxSpawns = 9;
            }
            else if (AFallingStar.CurrentIndex >= 9)
            {
                spawnRate = 2;
                maxSpawns = 11;
            }
        }
        else
            base.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
    }

    public override void AI(NPC npc)
    {
        base.AI(npc);

        if (npc.isLikeATownNPC)
        {
            npc.ForceBubbleChatState();
            //var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

            //int whoAmI = npc.whoAmI;
            //if (missionPlayer.NPCHasAvailableMission(npc.type) &&
            //    !missionPlayer.ActiveMissions().Any(m => npc.type == m.Employer) &&
            //    (!NotificationCreated.ContainsKey(whoAmI) || !NotificationCreated[whoAmI]))
            //{
                
            //    var mission = missionPlayer.AvailableMissions().FirstOrDefault(m => npc.type == m.Employer);
            //    if (mission != null)
            //    {

            //        InGameNotificationsTracker.AddNotification(new NPCMissionNotification(npc, mission, npc.Top));
            //        NotificationCreated[whoAmI] = true;
            //    }
            //}
            //else if (!missionPlayer.NPCHasAvailableMission(npc.type) && NotificationCreated.ContainsKey(whoAmI))
            //{
            //    NotificationCreated[whoAmI] = false;
            //}
        }
    }

    public override void OnKill(NPC npc)
    {
        base.OnKill(npc);
        if (!npc.immortal)
            MissionManager.Instance.OnNPCKill(npc);
    }
}

public class ObjectiveEventTile : GlobalTile
{
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);
        MissionManager.Instance.OnBreakTile(type, ref fail, ref effectOnly);
    }
}