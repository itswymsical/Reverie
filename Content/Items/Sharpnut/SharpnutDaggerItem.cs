using Reverie.Content.Projectiles.Sharpnut;

namespace Reverie.Content.Items.Sharpnut;

public class SharpnutDaggerItem : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 8;
        Item.crit = 1; // %5
        Item.width = Item.height = 32;
        Item.useTime = Item.useAnimation = 20;

        Item.knockBack = 0.5f;
        Item.shoot = ModContent.ProjectileType<SharpnutDaggerProj>();
        Item.shootSpeed = 10f;
        Item.value = Item.sellPrice(copper: 85);
        Item.UseSound = SoundID.Item1;
        Item.DamageType = DamageClass.Ranged;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.rare = ItemRarityID.White;

        Item.useTurn = 
            Item.noUseGraphic = 
            Item.noMelee = true;
    }
    public override void AddRecipes()
    {
        base.AddRecipes();
        CreateRecipe()
            .AddIngredient(ItemID.Acorn, 8)
            .AddRecipeGroup(RecipeGroupID.Wood, 18)
            .AddTile(TileID.LivingLoom)
            .Register();
    }
}
