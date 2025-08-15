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

            var currentSet = mission.ObjectiveList[mission.CurrentList];

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
            CurObjectiveIndex = mission.CurrentList,
            ObjectiveIndex = mission.ObjectiveList
                .Select(set => new ObjectiveIndexState
                {
                    Objectives = set.Objective
                        .Select(obj => new ObjectiveState
                        {
                            Description = obj.Description,
                            IsCompleted = obj.IsCompleted,
                            RequiredCount = obj.RequiredCount,
                            CurrentCount = obj.Count
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

        if (state.CurObjectiveIndex >= 0 && state.CurObjectiveIndex < mission.ObjectiveList.Count)
        {
            mission.CurrentList = state.CurObjectiveIndex;
        }
        else
        {
            ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid CurrentList {state.CurObjectiveIndex} for mission {mission.ID}, resetting to 0");
            mission.CurrentList = 0;
        }

        if (mission.ObjectiveList.Count != state.ObjectiveIndex.Count)
        {
            ModContent.GetInstance<Reverie>().Logger.Warn(
                $"Mission {mission.ID} objective set count mismatch: Expected {mission.ObjectiveList.Count}, got {state.ObjectiveIndex.Count}");
        }

        // Process each objective set with improved matching by description
        for (var i = 0; i < Math.Min(mission.ObjectiveList.Count, state.ObjectiveIndex.Count); i++)
        {
            var savedSet = state.ObjectiveIndex[i];
            var currentSet = mission.ObjectiveList[i];

            if (currentSet.Objective.Count != savedSet.Objectives.Count)
            {
                ModContent.GetInstance<Reverie>().Logger.Warn(
                    $"Mission {mission.ID} set {i} objective count mismatch: Expected {currentSet.Objective.Count}, got {savedSet.Objectives.Count}");
            }

            foreach (var currentObj in currentSet.Objective)
            {
                var matchingObj = savedSet.Objectives.FirstOrDefault(obj =>
                    obj.Description.Equals(currentObj.Description, StringComparison.OrdinalIgnoreCase));

                if (matchingObj != null)
                {
                    currentObj.IsCompleted = matchingObj.IsCompleted;
                    currentObj.Count = Math.Min(matchingObj.CurrentCount, currentObj.RequiredCount);

                    if (currentObj.IsCompleted && currentObj.Count < currentObj.RequiredCount)
                    {
                        currentObj.Count = currentObj.RequiredCount;
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
            (mission.CurrentList < 0 || mission.CurrentList >= mission.ObjectiveList.Count))
        {
            ModContent.GetInstance<Reverie>().Logger.Warn(
                $"Ongoing mission {mission.ID} has invalid current index {mission.CurrentList}, resetting to 0");
            mission.CurrentList = 0;
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