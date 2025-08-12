using Terraria;
using Reverie.Common.Items.Components;
using Reverie.Core.Missions;
using System.Linq;
using Reverie.Core.Missions.Core;
using System.Collections.Generic;

namespace Reverie.Utilities;

/// <summary>
/// Helper class for handling mission progress updates while preventing duplicate progress from the same items.
/// Uses direct calls to WorldMissionSystem for mainline missions - no packets needed!
/// </summary>
public static class MissionUtils
{
    /// <summary>
    /// Checks if an item should update progress for missions.
    /// Handles both mainline (world-level) and sideline (player-level) missions.
    /// </summary>
    /// <param name="item">The item to process.</param>
    /// <param name="player">The player who picked up or has the item.</param>
    /// <returns>True if progress was updated, false if item had already contributed.</returns>
    public static bool TryUpdateProgressForItem(Item item, Player player)
    {
        if (MissionItemComponent.HasItemContributed(item))
        {
            return false;
        }

        var missionPlayer = player.GetModPlayer<MissionPlayer>();
        var progressUpdated = false;

        // Check all active missions (both mainline and sideline)
        foreach (var mission in missionPlayer.ActiveMissions())
        {
            if (mission.Progress != MissionProgress.Ongoing)
                continue;

            var currentSet = mission.Objective[mission.CurrentIndex];
            if (currentSet.IsCompleted)
                continue;

            // If this item contributes to any objective in the current set, mark as updated
            // The specific mission logic will determine if this item actually triggers progress
            foreach (var objective in currentSet.Objectives.Where(o => !o.IsCompleted))
            {
                // This is a simplified check - specific missions should override this logic
                // For now, we just check if the item type name appears in the objective description
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

    /// <summary>
    /// Removes items from a player's inventory for mission requirements.
    /// </summary>
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

    /// <summary>
    /// Updates mission progress using direct calls - no packets needed!
    /// Automatically handles mainline vs sideline missions.
    /// </summary>
    /// <param name="player">The player triggering the progress update</param>
    /// <param name="missionId">The mission to update</param>
    /// <param name="objectiveIndex">The objective index to update</param>
    /// <param name="amount">Amount of progress to add</param>
    /// <returns>True if progress was updated</returns>
    public static bool UpdateMissionProgress(Player player, int missionId, int objectiveIndex, int amount = 1)
    {
        var mission = GetMission(player, missionId);

        if (mission?.Progress == MissionProgress.Ongoing)
        {
            if (mission.IsMainline)
            {
                // Direct call to world system - world data sync handles multiplayer automatically
                return WorldMissionSystem.Instance.UpdateMissionProgress(missionId, objectiveIndex, amount, player);
            }
            else
            {
                // Sideline missions are handled locally
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                return missionPlayer.UpdateMissionProgress(missionId, objectiveIndex, amount);
            }
        }

        return false;
    }

    /// <summary>
    /// Starts a mission using direct calls - no packets needed!
    /// Automatically handles mainline vs sideline missions.
    /// </summary>
    public static void StartMission(Player player, int missionId)
    {
        var mission = GetMission(player, missionId);

        if (mission?.Status == MissionStatus.Unlocked)
        {
            if (mission.IsMainline)
            {
                // Direct call to world system - world data sync handles multiplayer automatically
                WorldMissionSystem.Instance.StartMission(missionId);
            }
            else
            {
                // Sideline missions are handled locally
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                missionPlayer.StartMission(missionId);
            }
        }
    }

    /// <summary>
    /// Unlocks a mission using direct calls - no packets needed!
    /// Automatically handles mainline vs sideline missions.
    /// </summary>
    public static void UnlockMission(Player player, int missionId, bool broadcast = false)
    {
        var mission = GetMission(player, missionId);

        if (mission?.Status == MissionStatus.Locked)
        {
            if (mission.IsMainline)
            {
                // Direct call to world system - world data sync handles multiplayer automatically
                WorldMissionSystem.Instance.UnlockMission(missionId, broadcast);
            }
            else
            {
                // Sideline missions are handled locally
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                missionPlayer.UnlockMission(missionId, broadcast);
            }
        }
    }

    /// <summary>
    /// Completes a mission using direct calls - no packets needed!
    /// Automatically handles mainline vs sideline missions.
    /// </summary>
    public static void CompleteMission(Player player, int missionId)
    {
        var mission = GetMission(player, missionId);

        if (mission?.Progress == MissionProgress.Ongoing)
        {
            if (mission.IsMainline)
            {
                // Direct call to world system - world data sync handles multiplayer automatically
                WorldMissionSystem.Instance.CompleteMission(missionId, player);
            }
            else
            {
                // Sideline missions are handled locally
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                missionPlayer.CompleteMission(missionId);
            }
        }
    }

    /// <summary>
    /// Gets a mission from the appropriate source (world for mainline, player for sideline).
    /// </summary>
    public static Mission GetMission(Player player, int missionId)
    {
        var missionPlayer = player.GetModPlayer<MissionPlayer>();
        return missionPlayer.GetMission(missionId);
    }

    /// <summary>
    /// Gets all active missions for a player (combines mainline and sideline).
    /// </summary>
    public static IEnumerable<Mission> GetActiveMissions(Player player)
    {
        var missionPlayer = player.GetModPlayer<MissionPlayer>();
        return missionPlayer.ActiveMissions();
    }

    /// <summary>
    /// Gets all available missions for a player (combines mainline and sideline).
    /// </summary>
    public static IEnumerable<Mission> GetAvailableMissions(Player player)
    {
        var missionPlayer = player.GetModPlayer<MissionPlayer>();
        return missionPlayer.AvailableMissions();
    }

    /// <summary>
    /// Checks if a player has completed a specific mission.
    /// </summary>
    public static bool HasCompletedMission(Player player, int missionId)
    {
        var mission = GetMission(player, missionId);
        return mission?.Progress == MissionProgress.Completed;
    }

    /// <summary>
    /// Checks if a mission is currently active for a player.
    /// </summary>
    public static bool IsMissionActive(Player player, int missionId)
    {
        var mission = GetMission(player, missionId);
        return mission?.Progress == MissionProgress.Ongoing;
    }

    /// <summary>
    /// Checks if a mission is available (unlocked but not started) for a player.
    /// </summary>
    public static bool IsMissionAvailable(Player player, int missionId)
    {
        var mission = GetMission(player, missionId);
        return mission?.Status == MissionStatus.Unlocked && mission.Progress == MissionProgress.Inactive;
    }
}