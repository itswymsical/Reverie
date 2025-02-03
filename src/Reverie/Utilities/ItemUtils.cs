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
}