using Terraria.ModLoader.IO;
using Reverie.Core.Items.Components;
using Terraria.DataStructures;
using System.Collections.Generic;

namespace Reverie.Common.Items.Components;

/// <summary>
/// Component that marks items as having contributed to mission progress,
/// preventing duplicate progress updates when the same item is picked up multiple times.
/// </summary>
public class MissionItemComponent : ItemComponent
{
    #region Fields
    /// <summary>
    ///     Indicates whether this item has already contributed to mission progress.
    /// </summary>
    private bool hasContributed;

    /// <summary>
    ///     Indicates whether this item is marked for a mission.
    /// </summary>
    private bool markedForMission;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets whether this item has already contributed to mission progress.
    /// </summary>
    public bool HasContributedToProgress
    {
        get => hasContributed;
        set => hasContributed = value;
    }

    /// <summary>
    /// Gets or sets whether this item is marked for a mission.
    /// </summary>
    public bool MarkedForMission
    {
        get => markedForMission;
        set => markedForMission = value;
    }
    #endregion

    #region Initialization
    public MissionItemComponent()
    {
        Enabled = true;
    }
    #endregion

    #region Override Methods
    public override GlobalItem Clone(Item from, Item to)
    {
        var clone = (MissionItemComponent)base.Clone(from, to);
        clone.hasContributed = hasContributed;
        clone.markedForMission = markedForMission;
        return clone;
    }

    public override void SaveData(Item item, TagCompound tag)
    {
        tag["hasContributed"] = hasContributed;
        tag["markedForMission"] = markedForMission;
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        hasContributed = tag.GetBool("hasContributed");
        markedForMission = tag.GetBool("markedForMission");
    }

    public override void OnCreated(Item item, ItemCreationContext context)
    {
        // prevent pickup from triggering when you craft shit
        base.OnCreated(item, context);
        var component = GetOrCreate(item);
        component.HasContributedToProgress = true;
    }
    #endregion

    #region Helper Methods
    /// <summary>
    ///     Gets the MissionItemComponent for a given item, creating it if it doesn't exist.
    /// </summary>
    public static MissionItemComponent GetOrCreate(Item item)
    {
        return item.GetGlobalItem<MissionItemComponent>();
    }

    /// <summary>
    ///     Marks an item as having contributed to mission progress.
    /// </summary>
    public static void MarkAsContributed(Item item)
    {
        var component = GetOrCreate(item);
        component.HasContributedToProgress = true;
    }

    /// <summary>
    ///     Marks an item for a mission, making it a protected quest item.
    /// </summary>
    public static void MarkForMission(Item item)
    {
        item.value = 0;
        item.favorited = true;
        item.questItem = true;
        var component = GetOrCreate(item);
        component.MarkedForMission = true;
    }

    /// <summary>
    /// Checks if an item has already contributed to mission progress.
    /// </summary>
    public static bool HasItemContributed(Item item)
    {
        var component = GetOrCreate(item);
        return component.HasContributedToProgress;
    }

    /// <summary>
    /// Checks if an item is marked for a mission.
    /// </summary>
    public static bool IsMarkedForMission(Item item)
    {
        var component = GetOrCreate(item);
        return component.MarkedForMission;
    }

    /// <summary>
    /// Removes mission marking from an item.
    /// </summary>
    public static void UnmarkForMission(Item item)
    {
        var component = GetOrCreate(item);
        component.MarkedForMission = false;
        item.questItem = false;
        item.favorited = false;
    }
    #endregion

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        base.ModifyTooltips(item, tooltips);
        //if (IsMarkedForMission(item))
        //{
        //    tooltips.Add(new TooltipLine(Mod, "Mission Item", "Mission Item")
        //    {
        //        OverrideColor = Color.Gold
        //    });
        //}
    }
}