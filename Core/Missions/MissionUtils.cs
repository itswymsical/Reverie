using Reverie.Common.Items.Components;
using System.Linq;

namespace Reverie.Core.Missions;

/// <summary>
/// Helper class for handling mission progress updates while preventing duplicate progress from the same items.
/// Single player only.
/// </summary>
public static class MissionUtils
{
    public static bool ValidateGameMode()
    {
        return Main.netMode == NetmodeID.SinglePlayer;
    }

    /// <summary>
    /// Checks if an item should update progress for a mission.
    /// Prevents duplicate progress from the same items.
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

        foreach (var mission in missionPlayer.ActiveMissions())
        {
            if (mission.Progress != MissionProgress.Ongoing)
                continue;

            var currentSet = mission.Objective[mission.CurrentIndex];

            if (currentSet.IsCompleted)
                continue;

            progressUpdated = true;
        }

        if (progressUpdated)
        {
            MissionItemComponent.MarkAsContributed(item);
        }

        return progressUpdated;
    }

    /// <summary>
    /// Removes items from player inventory for mission consumption.
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
    /// Converts a mission to a data container for serialization.
    /// </summary>
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

    /// <summary>
    /// Loads mission state from a data container.
    /// </summary>
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

        if (state.CurObjectiveIndex >= 0 && state.CurObjectiveIndex < mission.Objective.Count)
        {
            mission.CurrentIndex = state.CurObjectiveIndex;
        }
        else
        {
            ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid CurrentIndex {state.CurObjectiveIndex} for mission {mission.ID}, resetting to 0");
            mission.CurrentIndex = 0;
        }

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

            if (currentSet.Objectives.Count != savedSet.Objectives.Count)
            {
                ModContent.GetInstance<Reverie>().Logger.Warn(
                    $"Mission {mission.ID} set {i} objective count mismatch: Expected {currentSet.Objectives.Count}, got {savedSet.Objectives.Count}");
            }

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

        if (mission.Progress == MissionProgress.Ongoing &&
            (mission.CurrentIndex < 0 || mission.CurrentIndex >= mission.Objective.Count))
        {
            ModContent.GetInstance<Reverie>().Logger.Warn(
                $"Ongoing mission {mission.ID} has invalid current index {mission.CurrentIndex}, resetting to 0");
            mission.CurrentIndex = 0;
        }
    }

    /// <summary>
    /// Checks if mainline missions should be blocked due to multiplayer.
    /// Shows warning if needed.
    /// </summary>
    public static bool ShouldBlockMainlineMission(Mission mission)
    {
        if (mission.IsMainline && !ValidateGameMode())
        {
            Main.NewText("Story missions are only available in single player worlds.", Color.OrangeRed);
            return true;
        }
        return false;
    }
}