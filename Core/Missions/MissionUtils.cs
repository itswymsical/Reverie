using Reverie.Common.Items.Components;
using System.Linq;

namespace Reverie.Core.Missions;

/// <summary>
/// Helper class for handling mission progress updates while preventing duplicate progress from the same items.
/// Single player only.
/// </summary>
public static class MissionUtils
{

    /// <summary>
    /// Checks if an item should update progress for a mission.
    /// Prevents duplicate progress from the same items.
    /// </summary>
    /// <param name="item">The item to process.</param>
    /// <param name="player">The player who picked up or has the item.</param>
    /// <returns>True if progress was updated, false if item had already contributed.</returns>
    public static bool Check_If_ItemUpdatedProgress(Item item, Player player)
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
    public static void TakePlayerItems(Player player, int itemType, int amount)
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
}