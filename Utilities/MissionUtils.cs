using Terraria;
using Reverie.Common.Items.Components;
using Reverie.Core.Missions;
using System.Linq;
using Reverie.Core.Missions.Core;
using System.Collections.Generic;

namespace Reverie.Utilities;

/// <summary>
/// Helper class for handling mission progress updates while preventing duplicate progress from the same items.
/// </summary>
public static class MissionUtils
{
    /// <summary>
    /// Updates mission progress based on item collection, preventing duplicate contributions.
    /// </summary>
    public static bool TryUpdateProgressForItem(Item item, Player player)
    {
        if (MissionItemComponent.HasItemContributed(item))
        {
            return false;
        }

        var missionPlayer = player.GetModPlayer<MissionPlayer>();
        var progressUpdated = false;

        foreach (var mission in missionPlayer.ActiveMissions())
        {
            if (mission.Progress != MissionProgress.Ongoing)
                continue;

            var currentSet = mission.Objective[mission.CurrentIndex];
            if (currentSet.IsCompleted)
                continue;

            foreach (var objective in currentSet.Objectives.Where(o => !o.IsCompleted))
            {
                if (objective.Description.Contains(item.Name, StringComparison.OrdinalIgnoreCase) ||
                    objective.Description.Contains(item.type.ToString()))
                {
                    progressUpdated = true;
                    break;
                }
            }

            if (progressUpdated)
                break;
        }

        if (progressUpdated)
        {
            MissionItemComponent.MarkAsContributed(item);
        }

        return progressUpdated;
    }

    public static void RetrieveItemsFromPlayer(Player player, int itemType, int amount)
    {
        var remainingAmount = amount;

        for (var i = 0; i < player.inventory.Length && remainingAmount > 0; i++)
        {
            var item = player.inventory[i];
            if (item.type == itemType)
            {
                var amountToTake = Math.Min(remainingAmount, item.stack);
                remainingAmount -= amountToTake;
                item.stack -= amountToTake;

                if (item.stack <= 0)
                {
                    item.TurnToAir();
                }
            }
        }
    }

    public static bool UpdateMissionProgress(Player player, int missionId, int objectiveIndex, int amount = 1)
    {
        var mission = GetMission(player, missionId);

        if (mission?.Progress == MissionProgress.Ongoing)
        {
            if (mission.IsMainline)
            {
                return WorldMissionSystem.Instance.UpdateProgress(missionId, objectiveIndex, amount, player);
            }
            else
            {
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                return missionPlayer.UpdateMissionProgress(missionId, objectiveIndex, amount);
            }
        }

        return false;
    }

    public static void StartMission(Player player, int missionId)
    {
        var mission = GetMission(player, missionId);

        if (mission?.Status == MissionStatus.Unlocked)
        {
            if (mission.IsMainline)
            {
                WorldMissionSystem.Instance.StartMission(missionId);
            }
            else
            {
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                missionPlayer.StartMission(missionId);
            }
        }
    }

    public static void UnlockMission(Player player, int missionId, bool broadcast = false)
    {
        var mission = GetMission(player, missionId);

        if (mission?.Status == MissionStatus.Locked)
        {
            if (mission.IsMainline)
            {
                WorldMissionSystem.Instance.UnlockMission(missionId, broadcast);
            }
            else
            {
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                missionPlayer.UnlockMission(missionId, broadcast);
            }
        }
    }

    public static void CompleteMission(Player player, int missionId)
    {
        var mission = GetMission(player, missionId);

        if (mission?.Progress == MissionProgress.Ongoing)
        {
            if (mission.IsMainline)
            {
                WorldMissionSystem.Instance.CompleteMission(missionId, player);
            }
            else
            {
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                missionPlayer.CompleteMission(missionId);
            }
        }
    }

    public static Mission GetMission(Player player, int missionId)
    {
        var missionPlayer = player.GetModPlayer<MissionPlayer>();
        return missionPlayer.GetMission(missionId);
    }

    public static IEnumerable<Mission> GetActiveMissions(Player player)
    {
        var missionPlayer = player.GetModPlayer<MissionPlayer>();
        return missionPlayer.ActiveMissions();
    }

    public static IEnumerable<Mission> GetAvailableMissions(Player player)
    {
        var missionPlayer = player.GetModPlayer<MissionPlayer>();
        return missionPlayer.AvailableMissions();
    }

    public static bool HasCompletedMission(Player player, int missionId)
    {
        var mission = GetMission(player, missionId);
        return mission?.Progress == MissionProgress.Completed;
    }

    public static bool IsMissionActive(Player player, int missionId)
    {
        var mission = GetMission(player, missionId);
        return mission?.Progress == MissionProgress.Ongoing;
    }

    public static bool IsMissionAvailable(Player player, int missionId)
    {
        var mission = GetMission(player, missionId);
        return mission?.Status == MissionStatus.Unlocked && mission.Progress == MissionProgress.Inactive;
    }
}
