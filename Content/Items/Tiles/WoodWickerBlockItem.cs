using Reverie.Content.Tiles.Misc;

namespace Reverie.Content.Items.Tiles;

public class WoodWickerBlockItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.value = Item.sellPrice(copper: 4);

        Item.DefaultToPlaceableTile(ModContent.TileType<WoodWickerTile>());

        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe(4)
        .AddIngredient(ItemID.Wood, 4)
        .AddIngredient(ItemID.Cobweb, 2)
        .AddTile(TileID.Sawmill)
        .Register();
    }
}