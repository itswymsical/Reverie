using Reverie.Content.Tiles;

namespace Reverie.Content.Items;

public class WoodenShingle : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.Wood}";
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.value = Item.sellPrice(copper: 15);

        Item.DefaultToPlaceableTile(ModContent.TileType<WoodenShingleTile>());

        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe(4)
        .AddIngredient(ItemID.Wood, 2)
        .AddTile(TileID.Sawmill)
        .Register();
    }
}