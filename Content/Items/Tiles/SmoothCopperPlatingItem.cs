﻿using Reverie.Content.Tiles.Misc;

namespace Reverie.Content.Items.Tiles;

public class SmoothCopperPlatingItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.value = Item.sellPrice(copper: 4);

        Item.DefaultToPlaceableTile(ModContent.TileType<CopperPlatingTile>());

        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe(4)
        .AddIngredient(ItemID.CopperBar)
        .AddTile(TileID.HeavyWorkBench)
        .Register();
    }
}