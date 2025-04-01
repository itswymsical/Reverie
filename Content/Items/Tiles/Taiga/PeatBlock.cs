using Reverie.Content.Tiles.Taiga;

namespace Reverie.Content.Items.Tiles.Taiga;

public class PeatBlock : ModItem
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

        CreateRecipe(2)
        .AddIngredient(ItemID.MudBlock, 1)
        .AddRecipeGroup(RecipeGroupID.Wood, 2)
        .Register();
    }
}
