using System.Collections.Generic;
using Terraria.GameContent;

namespace Reverie.Common.Items.Types;

public sealed class MagicMirrorGlobalItem : GlobalItem
{
    /// <summary>
    ///     The key of the tooltip line added to magic mirrors.
    /// </summary>
    public const string TOOLTIP_KEY = $"{nameof(Reverie)}:MagicMirror";
    
    /// <summary>
    ///     The color of the tooltip line added to magic mirrors.
    /// </summary>
    public static readonly Color TooltipColor = new(150, 150, 255);
    
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type == ItemID.MagicMirror || entity.type == ItemID.IceMirror || entity.type == ItemID.CellPhone || entity.type == ItemID.Shellphone;
    }
    
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        base.ModifyTooltips(item, tooltips);

        // TODO: Localize.
        var line = new TooltipLine(Mod, TOOLTIP_KEY, $"Right-click to open Magic Mirror [i:{ItemID.FragmentStardust}]")
        {
            OverrideColor = TooltipColor
        };
        
        tooltips.Add(line);
    }
}