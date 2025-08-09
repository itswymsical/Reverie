using Reverie.Content.Tiles.TemperateForest.Furniture;

namespace Reverie.Content.Tiles.TemperateForest;

public class BirchWoodHammerItem : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 4;
        Item.width = Item.height = 40;
        Item.knockBack = 5.5f;
        Item.useTime = Item.useAnimation = 30;
        Item.value = Item.sellPrice(copper: 10);
        Item.rare = ItemRarityID.White;
        Item.hammer = 35;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item1;

        Item.DamageType = DamageClass.Melee;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
        .AddIngredient<BirchWoodItem>(8)
        .AddTile(TileID.WorkBenches)
        .Register();
    }
}

