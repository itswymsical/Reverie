using Terraria;
using Reverie.Common.Items.Components;
using Reverie.Core.Missions;
using System.Linq;

namespace Reverie.Utilities;

/// <summary>
///     Helper class for handling mission progress updates while preventing duplicate progress from the same items.
/// </summary>
public static class MissionUtils
{
    /// <summary>
    ///     Updates mission progress for an item if it hasn't already contributed.
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
        var currentStack = 0;

        foreach (var mission in missionPlayer.GetActiveMissions())
        {
            var currentSet = mission.ObjectiveIndex[mission.CurObjectiveIndex];

            if (item.stack > 0 && !currentSet.IsCompleted)
            {
                MissionManager.Instance.OnItemObtained(item);
                progressUpdated = true;
            }
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

    public static MissionDataContainer ToState(this Mission mission)
    {
        return new MissionDataContainer
        {
            ID = mission.ID,
            Progress = mission.Progress,
            Availability = mission.Availability,
            Unlocked = mission.Unlocked,
            CurObjectiveIndex = mission.CurObjectiveIndex,
            ObjectiveIndex = mission.ObjectiveIndex
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
        if (state == null) return;

        mission.Progress = state.Progress;
        mission.Availability = state.Availability;
        mission.Unlocked = state.Unlocked;
        mission.CurObjectiveIndex = state.CurObjectiveIndex;


        for (var i = 0; i < Math.Min(mission.ObjectiveIndex.Count, state.ObjectiveIndex.Count); i++)
        {
            var savedSet = state.ObjectiveIndex[i];
            var currentSet = mission.ObjectiveIndex[i];

            for (var j = 0; j < Math.Min(currentSet.Objectives.Count, savedSet.Objectives.Count); j++)
            {
                var savedObj = savedSet.Objectives[j];
                var currentObj = currentSet.Objectives[j];

                if (savedObj.Description == currentObj.Description)
                {
                    currentObj.IsCompleted = savedObj.IsCompleted;
                    currentObj.CurrentCount = savedObj.CurrentCount;
                }
            }
        }
    }
}
