using Terraria;
using Reverie.Common.Items.Components;

namespace Reverie.Core.Missions.MissionAttributes;

/// <summary>
///     Helper class for handling mission progress updates while preventing duplicate progress from the same items.
/// </summary>
public static class MissionProgressHelper
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
        bool progressUpdated = false;

        foreach (var mission in missionPlayer.GetActiveMissions())
        {
            var currentSet = mission.ObjectiveSets[mission.CurrentSetIndex];

            if (item.stack > 0 && !currentSet.IsCompleted)
            {
                // Let the mission handler process the item
                MissionHandlerManager.Instance.OnItemPickup(item);
                progressUpdated = true;
            }
        }

        if (progressUpdated)
        {
            // Mark the item as having contributed to prevent duplicate progress
            MissionItemComponent.MarkAsContributed(item);
        }

        return progressUpdated;
    }
}
