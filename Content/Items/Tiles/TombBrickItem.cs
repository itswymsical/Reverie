using Reverie.Content.Tiles.Misc;

namespace Reverie.Content.Items.Tiles;

public class TombBrickItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.value = Item.sellPrice(copper: 8);

        Item.DefaultToPlaceableTile(ModContent.TileType<TombBrickTile>());

        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe(4)
        .AddIngredient(ItemID.StoneBlock, 8)
        .AddTile(TileID.HeavyWorkBench)
        .Register();
    }
}