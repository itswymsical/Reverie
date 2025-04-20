using Reverie.Content.Tiles;

namespace Reverie.Content.Items.Tiles;

public class WoodWickerWallItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.value = Item.sellPrice(copper: 4);

        Item.DefaultToPlaceableWall(ModContent.WallType<WoodWickerWall>());

        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe(4)
        .AddIngredient(ModContent.ItemType<WoodWickerBlockItem>(), 1)
        .AddTile(TileID.WorkBenches)
        .Register();
    }
}