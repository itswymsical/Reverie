using System.Collections.Generic;
using Reverie.Framework.Items.Components;
using Terraria.Utilities;

namespace Reverie.Common.Items.Components;

public sealed class ShovelItemComponent : ItemComponent
{
    public const string TOOLTIP_NAME = $"{nameof(Reverie)}:ShovelInfo";
    
    public const string SMART_CURSOR_TOOLTIP_NAME = $"{nameof(Reverie)}:ShovelInfo_SmartCursor";
    
    /// <summary>
    ///     Gets or sets the prefixes that can be applied to the shovel.
    /// </summary>
    public int[] Prefixes { get; set; } =
    [
        PrefixID.Agile,
        PrefixID.Quick,
        PrefixID.Light,

        PrefixID.Slow,
        PrefixID.Sluggish,
        PrefixID.Lazy,

        PrefixID.Bulky,
        PrefixID.Heavy,

        PrefixID.Damaged,
        PrefixID.Broken,

        PrefixID.Unhappy,
        PrefixID.Nimble,
        PrefixID.Dull
    ];
    
    /// <summary>
    ///     Gets or sets the shovel range, in tiles.
    /// </summary>
    public int ShovelRange { get; set; } = 5;
    
    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        return Enabled ? rand.Next(Prefixes) : base.ChoosePrefix(item, rand);
    }
    
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        base.ModifyTooltips(item, tooltips);

        if (!Enabled)
        {
            return;
        }
        
        // TODO: Localize.
        var tooltip = new TooltipLine(Mod, TOOLTIP_NAME, "Stronger on soft tiles");
        
        tooltips.Add(tooltip);
        
        if (!Main.SmartCursorWanted)
        {
            return;
        }
        
        // TODO: Localize.
        var smartTooltip = new TooltipLine(Mod, SMART_CURSOR_TOOLTIP_NAME, "Smart cursor disables craters");
            
        tooltips.Add(smartTooltip);
    }
    
    public override bool? UseItem(Item item, Player player)
    {
        var distance = 16f * ShovelRange;
        
        if (player.DistanceSQ(Main.MouseWorld) < distance * distance)
        {
            
        }

        return true;
    }
}