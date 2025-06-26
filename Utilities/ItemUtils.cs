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

    public static bool IsGem(this Item item)
    {
        return item.type == ItemID.Amethyst
            || item.type == ItemID.Topaz
            || item.type == ItemID.Sapphire
            || item.type == ItemID.Emerald
            || item.type == ItemID.Ruby
            || item.type == ItemID.Diamond
            || item.type == ItemID.Amber;
    }

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

    public static bool IsMadeFromMetal(this Item item, params int[] metalTypes)
    {
        foreach (var metalType in metalTypes)
        {
            var metalItem = new Item();
            metalItem.SetDefaults(metalType);

            if (item.CraftedFromRecursive(metalItem))
                return true;
        }

        return false;
    }

    public static bool IsMadeFromAnyMetal(this Item item)
    {
        int[] commonMetals = [
        ItemID.CopperOre, ItemID.CopperBar,
        ItemID.TinOre, ItemID.TinBar,
        ItemID.IronOre, ItemID.IronBar,
        ItemID.LeadOre, ItemID.LeadBar,
        ItemID.SilverOre, ItemID.SilverBar,
        ItemID.TungstenOre, ItemID.TungstenBar,
        ItemID.GoldOre, ItemID.GoldBar,
        ItemID.PlatinumOre, ItemID.PlatinumBar,

        ItemID.DemoniteOre, ItemID.DemoniteBar,
        ItemID.CrimtaneOre, ItemID.CrimtaneBar,
        ItemID.Hellstone, ItemID.HellstoneBar,

        ItemID.CobaltOre, ItemID.CobaltBar,
        ItemID.PalladiumOre, ItemID.PalladiumBar,
        ItemID.MythrilOre, ItemID.MythrilBar,
        ItemID.OrichalcumOre, ItemID.OrichalcumBar,
        ItemID.AdamantiteOre, ItemID.AdamantiteBar,
        ItemID.TitaniumOre, ItemID.TitaniumBar,

        ItemID.ChlorophyteOre, ItemID.ChlorophyteBar,
        ItemID.HallowedBar, ItemID.LunarBar,
    ];

        return item.IsMadeFromMetal(commonMetals);
    }

    public static bool CraftedWith(this Item item, int ingredient)
    {
        for (var i = 0; i < Recipe.maxRecipes; i++)
        {
            var currentRecipe = Main.recipe[i];

            if (currentRecipe == null || currentRecipe.createItem.type != item.type)
                continue;

            for (var j = 0; j < currentRecipe.requiredItem.Count; j++)
            {
                if (currentRecipe.requiredItem[j] != null && currentRecipe.requiredItem[j].type == ingredient)
                    return true;
            }
        }

        return false;
    }

    public static bool CraftedWith(this Item item, Item ingredient)
    {
        for (var i = 0; i < Recipe.maxRecipes; i++)
        {
            var currentRecipe = Main.recipe[i];

            if (currentRecipe == null || currentRecipe.createItem.type != item.type)
                continue;

            for (var j = 0; j < currentRecipe.requiredItem.Count; j++)
            {
                if (currentRecipe.requiredItem[j] != null && currentRecipe.requiredItem[j].type == ingredient.type)
                    return true;
            }
        }

        return false;
    }

    public static bool CraftedFromRecursive(this Item item, Item baseIngredient, HashSet<int> visitedItems = null)
    {
        // Initialize visited items set to prevent infinite recursion
        if (visitedItems == null)
            visitedItems = new HashSet<int>();

        // If we've already checked this item, skip to prevent cycles
        if (visitedItems.Contains(item.type))
            return false;

        // Add current item to visited set
        visitedItems.Add(item.type);

        // Check all recipes that create this item
        for (var i = 0; i < Recipe.maxRecipes; i++)
        {
            var currentRecipe = Main.recipe[i];

            // Skip null or invalid recipes
            if (currentRecipe == null || currentRecipe.createItem.type != item.type)
                continue;

            // Check each ingredient in this recipe
            for (var j = 0; j < currentRecipe.requiredItem.Count; j++)
            {
                var requiredItem = currentRecipe.requiredItem[j];
                if (requiredItem == null)
                    continue;

                // Direct match - this recipe directly uses the base ingredient
                if (requiredItem.type == baseIngredient.type)
                    return true;

                // Recursive check - does this ingredient's crafting chain contain the base ingredient?
                if (requiredItem.CraftedFromRecursive(baseIngredient, visitedItems))
                    return true;
            }
        }

        return false;
    }
}