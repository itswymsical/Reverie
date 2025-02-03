namespace Reverie.Content.Items.Frostbark;

public class BorealHewerItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.DamageType = DamageClass.Melee;
        Item.damage = 9;
        Item.crit = 9;
        Item.knockBack = 1.2f;
        
        Item.noUseGraphic = true;
        Item.useTurn = false;

        Item.width = 50;
        Item.height = 50;
        
        Item.axe = 9;

        Item.UseSound = SoundID.DD2_MonkStaffSwing;
        Item.useTime = 24;
        Item.useAnimation = 24;
        Item.useStyle = ItemUseStyleID.Swing;

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 12);
        
        Item.shootSpeed = 10.5f;
        // Item.shoot = ModContent.ProjectileType<BorealHewerProj>();
    }
    
    public override bool CanUseItem(Player player)
    {
        return player.ownedProjectileCounts[Item.shoot] == 0;
    }
    
    public override void AddRecipes()
    {
        base.AddRecipes();
        
        CreateRecipe()
            .AddIngredient(ItemID.BorealWood, 16)
            .AddIngredient(ItemID.IceBlock, 8)
            .AddRecipeGroup(RecipeGroupID.IronBar, 4)
            .AddTile(TileID.Anvils)
            .Register();
    }
}