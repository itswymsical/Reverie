using Reverie.Content.Projectiles.Frostbark;

namespace Reverie.Content.Items.Frostbark;

public class BorealHewer : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 9;
        Item.DamageType = DamageClass.Melee;
        Item.width = Item.height = 50;
        Item.useTime = Item.useAnimation = 24;
        Item.knockBack = 1.2f;
        Item.axe = 9;
        Item.crit = 9;
        Item.value = Item.sellPrice(silver: 12);
        Item.rare = ItemRarityID.Blue;
        Item.useTurn = false;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.DD2_MonkStaffSwing;
        Item.shootSpeed = 10.5f;
        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<BorealHewerProj>();
    }
    public override bool CanUseItem(Player player)
    {
        return player.ownedProjectileCounts[ModContent.ProjectileType<BorealHewerProj>()] <= 0;
    }
    public override void AddRecipes()
    {
        var recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.BorealWood, 16);
        recipe.AddIngredient(ItemID.IceBlock, 8);
        recipe.AddRecipeGroup(RecipeGroupID.IronBar, 4);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}
