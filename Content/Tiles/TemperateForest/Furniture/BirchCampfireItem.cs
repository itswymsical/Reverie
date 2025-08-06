using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Tiles.TemperateForest.Furniture;

public class BirchCampfireItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<BirchCampfireTile>(), 0);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroupID.Wood, 10)
            .AddIngredient<BirchTorchItem>(5)
            .Register();

        CreateRecipe()
            .AddIngredient<BirchWoodItem>(10)
            .AddIngredient<BirchTorchItem>(5)
            .Register();
    }
}
