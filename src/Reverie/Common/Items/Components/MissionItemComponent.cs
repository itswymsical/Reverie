using System.Collections.Generic;
using Reverie.Core.Items.Components;

namespace Reverie.Common.Items.Components;

public sealed class MissionItemComponent : ItemComponent
{
    /// <summary>
    ///     The color of the tooltip line for mission items.
    /// </summary>
    public static readonly Color TooltipColor = new(95, 205, 228);
    
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        base.ModifyTooltips(item, tooltips);
        
        if (!Enabled)
        {
            return;
        }
        
        var index = tooltips.FindIndex(static line => line.Name == "Quest");

        if (index == -1)
        {
            return;
        }

        var line = tooltips[index];
        
        // TODO: Localize.
        line.Text = "Mission Item";
        line.OverrideColor = TooltipColor;
            
        // Does this tooltip line need to be added again?
        tooltips.Add(line);
    }
}