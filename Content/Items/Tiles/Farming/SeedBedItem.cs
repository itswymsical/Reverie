using Reverie.Content.Tiles.Farming;

namespace Reverie.Content.Items.Tiles.Farming;

public class SeedBedItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<SeedBedTile>());
        Item.value = Item.sellPrice(copper: 78);
    }
    public override void AddRecipes()
    {
        CreateRecipe(4)
        .AddIngredient(ItemID.ClayBlock, 5)
        .AddIngredient(ItemID.Fertilizer, 2)
        .AddTile(TileID.WorkBenches)
        .Register();
    }
}
