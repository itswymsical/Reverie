using Reverie.Content.Tiles;

namespace Reverie.Content.Items.Tiles;

public class WoodWickerBlock : ModItem
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