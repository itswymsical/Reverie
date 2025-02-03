namespace Reverie.Content.Items.Frostbark;

public class FlailstormItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.damage = 10;
        Item.crit = 3;
        Item.knockBack = 2.8f;

        Item.channel = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        
        Item.width = 38;
        Item.height = 38;
        
        Item.UseSound = SoundID.DD2_MonkStaffSwing;
        Item.useTime = 38;
        Item.useAnimation = 38;
        Item.useStyle = ItemUseStyleID.Shoot;

        Item.value = Item.sellPrice(silver: 14);
        Item.rare = ItemRarityID.Blue;

        Item.shootSpeed = 10.5f;
        // Item.shoot = ModContent.ProjectileType<FlailstormProj>();
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.BorealWood, 8)
            .AddIngredient(ItemID.IceBlock, 20)
            .AddRecipeGroup(RecipeGroupID.IronBar, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}