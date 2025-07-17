namespace Reverie.Content.Tiles.TemperateForest.Furniture;

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