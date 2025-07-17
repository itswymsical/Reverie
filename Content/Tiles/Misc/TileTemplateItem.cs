namespace Reverie.Content.Tiles.Misc;

public class TileTemplateItem : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.value = Item.sellPrice(copper: 8);

        Item.DefaultToPlaceableTile(ModContent.TileType<TileTemplate>());

        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe(9999)
        .AddIngredient(ItemID.DirtBlock)
        .Register();
    }
}