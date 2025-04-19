using Terraria.ModLoader.IO;
using Reverie.Core.Items.Components;
using Terraria.DataStructures;

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
        return clone;
    }

    public override void SaveData(Item item, TagCompound tag)
    {
        tag["hasContributed"] = hasContributed;
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        hasContributed = tag.GetBool("hasContributed");
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
    public override void OnCreated(Item item, ItemCreationContext context)
    {
        // prevent pickup from triggering when you craft shit
        base.OnCreated(item, context);
        var component = GetOrCreate(item);
        component.HasContributedToProgress = true;
    }

    /// <summary>
    /// Checks if an item has already contributed to mission progress.
    /// </summary>
    public static bool HasItemContributed(Item item)
    {
        var component = GetOrCreate(item);
        return component.HasContributedToProgress;
    }
    #endregion
}