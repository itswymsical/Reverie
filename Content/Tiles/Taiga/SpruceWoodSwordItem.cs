using Reverie.Content.Tiles.Taiga.Furniture;
using Reverie.Content.Tiles.TemperateForest.Furniture;

namespace Reverie.Content.Tiles.Taiga;

public class SpruceWoodSwordItem : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 7;
        Item.width = Item.height = 40;
        Item.useTime = 20;
        Item.useAnimation = 25;
        Item.knockBack = 5f;
        Item.value = Item.sellPrice(copper: 20);
        Item.rare = ItemRarityID.White;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item1;

        Item.DamageType = DamageClass.Melee;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
        .AddIngredient<SpruceWoodItem>(7)
        .AddTile(TileID.WorkBenches)
        .Register();
    }
}

