using Reverie.Content.Tiles.TemperateForest.Furniture;

namespace Reverie.Content.Items.Tiles.TemperateForest;

public class BirchWallItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Item.sellPrice(0);
        Item.DefaultToPlaceableWall(ModContent.WallType<BirchWoodWall>());
    }
    public override void AddRecipes()
    {
        CreateRecipe(4)
            .AddIngredient<BirchWoodItem>()
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}