using Terraria;
using Reverie.Common.Items.Components;
using Reverie.Core.Missions;
using Reverie.Core.Missions.System;
using System.Linq;
using Reverie.Core.Missions.Core;
using System.Collections.Generic;

namespace Reverie.Utilities;

/// <summary>
/// Helper class for handling mission progress updates with mainline/sideline distinction.
/// Mainline missions sync progress across all players, sideline missions are individual.
/// </summary>
public static class MissionUtils
{
    /// <summary>
    /// Checks if an item should update progress for missions.
    /// For mainline missions, checks all players. For sideline missions, only the triggering player.
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

        var progressUpdated = false;

        // Check missions based on mainline/sideline distinction
        var playersToCheck = GetPlayersToCheckForItem(player);

        foreach (var currentPlayer in playersToCheck)
        {
            var missionPlayer = currentPlayer.GetModPlayer<MissionPlayer>();

            foreach (var mission in missionPlayer.ActiveMissions())
            {
                if (mission.Progress != MissionProgress.Ongoing)
                    continue;

                var currentSet = mission.Objective[mission.CurrentIndex];

                if (currentSet.IsCompleted)
                    continue;

                // Check if this item is relevant to any objectives in the current set
                bool itemIsRelevant = CheckItemRelevantToObjectives(item, currentSet);

                if (itemIsRelevant)
                {
                    progressUpdated = true;
                    // Note: The actual progress update should be handled by the specific mission's event handlers
                    // This method just checks if the item is relevant and should be marked as contributed
                }
            }
        }

        if (progressUpdated)
        {
            MissionItemComponent.MarkAsContributed(item);
        }

        return progressUpdated;
    }

    /// <summary>
    /// Gets the list of players whose missions should be checked for item relevance.
    /// </summary>
    private static List<Player> GetPlayersToCheckForItem(Player triggeringPlayer)
    {
        var playersToCheck = new List<Player>();

        // For item checking, we need to look at both mainline and sideline missions
        // but we'll handle the actual progress updates differently in UpdateMissionProgressForPlayers

        // Always check the triggering player (for their sideline missions)
        if (triggeringPlayer?.active == true)
        {
            playersToCheck.Add(triggeringPlayer);
        }

        // Also check all other players (for mainline missions)
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true && player != triggeringPlayer)
            {
                playersToCheck.Add(player);
            }
        }

        return playersToCheck;
    }

    /// <summary>
    /// Helper method to check if an item is relevant to any objectives in a set.
    /// This is a basic implementation - specific missions should override with their own logic.
    /// </summary>
    private static bool CheckItemRelevantToObjectives(Item item, ObjectiveSet objectiveSet)
    {
        // Basic check - look for item type/name in objective descriptions
        // More sophisticated missions can implement their own relevance checking
        foreach (var objective in objectiveSet.Objectives)
        {
            if (objective.IsCompleted) continue;

            // Simple heuristic: check if item name appears in objective description
            var itemName = item.Name.ToLowerInvariant();
            var objectiveDesc = objective.Description.ToLowerInvariant();

            if (objectiveDesc.Contains(itemName) ||
                objectiveDesc.Contains(item.type.ToString()) ||
                CheckItemByCategory(item, objectiveDesc))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if an item matches certain categories mentioned in objective descriptions.
    /// </summary>
    private static bool CheckItemByCategory(Item item, string objectiveDescription)
    {
        // Check for common item categories
        if (objectiveDescription.Contains("wood") && item.createWall > 0) return true;
        if (objectiveDescription.Contains("ore") && item.createTile > 0 && item.rare > 0) return true;
        if (objectiveDescription.Contains("potion") && item.healLife > 0) return true;
        if (objectiveDescription.Contains("weapon") && item.damage > 0) return true;
        if (objectiveDescription.Contains("tool") && (item.pick > 0 || item.axe > 0 || item.hammer > 0)) return true;

        return false;
    }

    /// <summary>
    /// Updates mission progress with mainline/sideline distinction.
    /// Mainline missions: all players with the mission get progress
    /// Sideline missions: only the triggering player gets progress
    /// </summary>
    /// <param name="missionId">The mission ID to update</param>
    /// <param name="objectiveIndex">The objective index to update</param>
    /// <param name="amount">The amount of progress to add</param>
    /// <param name="triggeringPlayer">The player who triggered this update</param>
    /// <returns>True if any player's mission was updated</returns>
    public static bool UpdateMissionProgressForPlayers(int missionId, int objectiveIndex, int amount = 1, Player triggeringPlayer = null)
    {
        bool anyUpdated = false;

        // First, determine if this is a mainline mission
        var sampleMission = MissionFactory.Instance.GetMissionData(missionId);
        if (sampleMission == null) return false;

        if (sampleMission.IsMainline)
        {
            // Mainline missions: update for ALL players who have this mission active
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (player?.active != true) continue;

                var missionPlayer = player.GetModPlayer<MissionPlayer>();

                if (missionPlayer.missionDict.TryGetValue(missionId, out var mission) &&
                    mission.Progress == MissionProgress.Ongoing)
                {
                    var updated = mission.UpdateProgress(objectiveIndex, amount, triggeringPlayer ?? player);
                    if (updated)
                    {
                        missionPlayer.SyncMissionState(mission);

                        // Update the authoritative mainline state
                        MainlineMissionSyncSystem.UpdateMainlineMissionState(mission);
                        anyUpdated = true;
                    }
                }
            }
        }
        else
        {
            // Sideline missions: only update for the triggering player
            if (triggeringPlayer?.active == true)
            {
                var missionPlayer = triggeringPlayer.GetModPlayer<MissionPlayer>();
                if (missionPlayer.missionDict.TryGetValue(missionId, out var mission) &&
                    mission.Progress == MissionProgress.Ongoing)
                {
                    var updated = mission.UpdateProgress(objectiveIndex, amount, triggeringPlayer);
                    if (updated)
                    {
                        missionPlayer.SyncMissionState(mission);
                        anyUpdated = true;
                    }
                }
            }
        }

        return anyUpdated;
    }

    /// <summary>
    /// Retrieves items from the specified player's inventory.
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

    public static MissionDataContainer ToState(this Mission mission)
    {
        return new MissionDataContainer
        {
            ID = mission.ID,
            Progress = mission.Progress,
            Availability = mission.Status,
            Unlocked = mission.Unlocked,
            CurObjectiveIndex = mission.CurrentIndex,
            ObjectiveIndex = mission.Objective
                .Select(set => new ObjectiveIndexState
                {
                    Objectives = set.Objectives
                        .Select(obj => new ObjectiveState
                        {
                            Description = obj.Description,
                            IsCompleted = obj.IsCompleted,
                            RequiredCount = obj.RequiredCount,
                            CurrentCount = obj.CurrentCount
                        }).ToList()
                }).ToList(),
            NextMissionID = mission.NextMissionID
        };
    }

    public static void LoadState(this Mission mission, MissionDataContainer state)
    {
        if (state == null)
        {
            ModContent.GetInstance<Reverie>().Logger.Warn($"Null state provided for mission {mission.ID}");
            return;
        }

        mission.Progress = state.Progress;
        mission.Status = state.Availability;
        mission.Unlocked = state.Unlocked;

        // Validate current index is within bounds
        if (state.CurObjectiveIndex >= 0 && state.CurObjectiveIndex < mission.Objective.Count)
        {
            mission.CurrentIndex = state.CurObjectiveIndex;
        }
        else
        {
            ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid CurrentIndex {state.CurObjectiveIndex} for mission {mission.ID}, resetting to 0");
            mission.CurrentIndex = 0;
        }

        // If objective counts don't match, log a warning
        if (mission.Objective.Count != state.ObjectiveIndex.Count)
        {
            ModContent.GetInstance<Reverie>().Logger.Warn(
                $"Mission {mission.ID} objective set count mismatch: Expected {mission.Objective.Count}, got {state.ObjectiveIndex.Count}");
        }

        // Process each objective set with improved matching by description
        for (var i = 0; i < Math.Min(mission.Objective.Count, state.ObjectiveIndex.Count); i++)
        {
            var savedSet = state.ObjectiveIndex[i];
            var currentSet = mission.Objective[i];

            // If objective counts within a set don't match, log a warning
            if (currentSet.Objectives.Count != savedSet.Objectives.Count)
            {
                ModContent.GetInstance<Reverie>().Logger.Warn(
                    $"Mission {mission.ID} set {i} objective count mismatch: Expected {currentSet.Objectives.Count}, got {savedSet.Objectives.Count}");
            }

            // Process each objective, using description matching to handle potential reordering
            foreach (var currentObj in currentSet.Objectives)
            {
                var matchingObj = savedSet.Objectives.FirstOrDefault(obj =>
                    obj.Description.Equals(currentObj.Description, StringComparison.OrdinalIgnoreCase));

                if (matchingObj != null)
                {
                    currentObj.IsCompleted = matchingObj.IsCompleted;
                    currentObj.CurrentCount = Math.Min(matchingObj.CurrentCount, currentObj.RequiredCount);

                    if (currentObj.IsCompleted && currentObj.CurrentCount < currentObj.RequiredCount)
                    {
                        currentObj.CurrentCount = currentObj.RequiredCount;
                    }
                }
                else
                {
                    ModContent.GetInstance<Reverie>().Logger.Warn(
                        $"Mission {mission.ID} set {i} couldn't find saved state for objective '{currentObj.Description}'");
                }
            }
        }

        // Ensure mission index is valid if mission is active
        if (mission.Progress == MissionProgress.Ongoing &&
            (mission.CurrentIndex < 0 || mission.CurrentIndex >= mission.Objective.Count))
        {
            ModContent.GetInstance<Reverie>().Logger.Warn(
                $"Ongoing mission {mission.ID} has invalid current index {mission.CurrentIndex}, resetting to 0");
            mission.CurrentIndex = 0;
        }
    }
}