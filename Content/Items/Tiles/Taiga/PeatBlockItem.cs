using Reverie.Content.Tiles.Taiga;

namespace Reverie.Content.Items.Tiles.Taiga;

public class PeatBlockItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.value = Item.sellPrice(copper: 0);

        Item.DefaultToPlaceableTile(ModContent.TileType<PeatTile>());

        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe()
        .AddIngredient(ItemID.MudBlock, 1)
        .AddIngredient(ItemID.ClayBlock, 1)
        .Register();
    }
}
