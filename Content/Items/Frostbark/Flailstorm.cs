using Reverie.Content.Projectiles.Frostbark;

namespace Reverie.Content.Items.Frostbark;

public class Flailstorm : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 7;
        Item.width = Item.height = 38;
        Item.useTime = Item.useAnimation = 38;
        Item.knockBack = 2.8f;
        Item.crit = -2;
        Item.value = Item.sellPrice(silver: 14);
        Item.rare = ItemRarityID.Blue;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.DD2_MonkStaffSwing;
        Item.shootSpeed = 10.5f;

        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.noUseGraphic =
            Item.channel = Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<FlailstormProj>();
    }

    public override void AddRecipes()
    {
        var recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.BorealWood, 8);
        recipe.AddIngredient(ItemID.IceBlock, 20);
        recipe.AddRecipeGroup(nameof(ItemID.SilverBar), 8);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}
