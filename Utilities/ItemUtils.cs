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

    public static bool IsMiningTool(this Item item)
    {
        return item.pick > 0 || item.Name.Contains("Shovel");
    }

    /// <summary>
    /// Items counting as a magic mirror.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool IsMirror(this Item item)
    {
        return item.type == ItemID.MagicMirror
            || item.type == ItemID.IceMirror
            || item.type == ItemID.CellPhone
            || item.type == ItemID.Shellphone;
    }

    /// <summary>
    /// Items that count as hooks, non-block destroying tools, or other utility items.
    /// you can expand on this list later.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool MiscellaneousTool(this Item item)
    {
        return item.fishingPole > 0 
            || item.Name.Contains("Hook") 
            || item.type == ItemID.WebSlinger 
            || item.type == ItemID.Umbrella
            || item.type == ItemID.Binoculars
            || item.type == ItemID.BreathingReed
            || item.type == ItemID.SandcastleBucket
            || item.IsMirror();
    }

    public static bool IsOre(this Item item)
           => item.Name.Contains("Ore") || item.Name.EndsWith("ium")
           || item.Name.EndsWith("ite") || item.Name.EndsWith("yte");


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

    public static bool DealsDamage(this Item item) => item.DamageType != DamageClass.Default;

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

}