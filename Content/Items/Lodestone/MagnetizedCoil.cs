
namespace Reverie.Content.Items.Lodestone;

public class MagnetizedCoil : ModItem
{
    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(silver: 3);
        Item.width = Item.height = 28;
        Item.maxStack = 999;
    }

    public override void AddRecipes()
    {

        var recipe = CreateRecipe(2);
        recipe.AddIngredient(ItemID.Chain, 2);
        recipe.AddRecipeGroup(nameof(ItemID.CopperBar), 1);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}