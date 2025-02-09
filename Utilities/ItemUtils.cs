using Reverie.Common.Items.Components;
using Reverie.Core.Items.Components;
using System.Collections.Generic;

namespace Reverie.Utilities;

/// <summary>
///     Provides utility methods for items.
/// </summary>
public static class ItemUtils
{
    public static bool IsArmorSet<TModHead, TModBody, TModLegs>(Item head, Item body, Item legs)
        where TModHead : ModItem
        where TModBody : ModItem
        where TModLegs : ModItem
    {
        return head.type == ModContent.ItemType<TModHead>() && body.type == ModContent.ItemType<TModBody>() && legs.type == ModContent.ItemType<TModLegs>();
    }

    public static bool IsMiningTool(this Item item) => item.pick > 0 || item.TryEnable(out ShovelItemComponent component);

    private static readonly HashSet<string> MetalKeywords =
    [
        "Ore", "Bar", "Iron", "Lead", "Silver", "Gold", "Platinum", "Tungsten", "Tin", "Copper",
        "Cobalt", "Palladium", "Mythril", "Orichalcum", "Adamantite", "Titanium"
    ];

    public static bool IsAMetalItem(Item item)
    {
        foreach (string keyword in MetalKeywords)
        {
            if (item.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsWeapon(this Item item) => (item.DamageType == DamageClass.Magic
        || item.DamageType == DamageClass.Summon || item.DamageType == DamageClass.Melee
        || item.DamageType == DamageClass.Throwing);

    public static bool IsWood(this Item item)
    {
        return item.type == ItemID.Wood
            || item.type == ItemID.BorealWood
            || item.type == ItemID.PalmWood
            || item.type == ItemID.RichMahogany
            || item.type == ItemID.Ebonwood
            || item.type == ItemID.Shadewood
            || item.type == ItemID.AshWood
            || item.type == ItemID.Pearlwood;
    }

    public static bool IsMirror(this Item item)
    {
        return item.type == ItemID.MagicMirror
            || item.type == ItemID.IceMirror
            || item.type == ItemID.CellPhone
            || item.type == ItemID.Shellphone;
    }
}