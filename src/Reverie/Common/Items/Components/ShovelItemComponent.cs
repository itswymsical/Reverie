using System.Collections.Generic;
using Reverie.Core.Items.Components;
using Reverie.Utilities;
using Reverie.Utilities.Extensions;
using Terraria.Utilities;

namespace Reverie.Common.Items.Components;

public sealed class ShovelItemComponent : ItemComponent
{
    public const string TOOLTIP_NAME = $"{nameof(Reverie)}:ShovelInfo";
    public const string TOOLTIP_POWER = $"{nameof(Reverie)}:ShovelPower";
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
    ///     Gets or sets the shovel's range, in tiles.
    /// </summary>
    public int Range { get; set; } = 5;
    
    /// <summary>
    ///     Gets or sets the shovel's power.
    /// </summary>
    public int Power { get; set; }
    
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
        var tooltip = new TooltipLine(Mod, TOOLTIP_NAME, "'stronger on soft tiles'");
        
        tooltips.Add(tooltip);

        // TODO: Localize.
        var power = new TooltipLine(Mod, TOOLTIP_POWER, $"{Power}% digging power");

        tooltips.Add(power);
    }
    public override void HoldItem(Item item, Player player)
    {
        base.HoldItem(item, player);

        if (!Enabled)
        {
            return;
        }

        var distance = 16f * Range;

        if (player.DistanceSQ(Main.MouseWorld) < distance * distance)
        {

            player.cursorItemIconEnabled = true;
        }
    }
    public override bool? UseItem(Item item, Player player)
    {
        var distance = 16f * Range;

        if (player.DistanceSQ(Main.MouseWorld) < distance * distance)
        {
            
            player.DigArea((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, Power);
        }

        return true;
    }
}