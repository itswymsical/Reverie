using Reverie.Content.Tiles;

namespace Reverie.Content.Items.Tiles;

public class TombBrickWallItem : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.value = Item.sellPrice(copper: 4);

        Item.DefaultToPlaceableWall(ModContent.WallType<TombBrickWall>());

        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe(4)
        .AddIngredient(ModContent.ItemType<TombBrickItem>(), 1)
        .AddTile(TileID.WorkBenches)
        .Register();
    }
}