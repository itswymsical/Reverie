using Reverie.Content.Items.Sylvanwalde.Canopy;
using Reverie.Content.Projectiles.Sylvanwalde;

using Terraria.Audio;
using Terraria.DataStructures;

using static Reverie.Reverie;

namespace Reverie.Content.Items.Sylvanwalde;

public class AcornLauncherItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.Ranged;
        Item.damage = 17;

        Item.width = 92;
        Item.height = 34;

        Item.useTime = 20;
        Item.useAnimation = 20;

        Item.useAmmo = ItemID.Acorn;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = new SoundStyle($"{SFX_DIRECTORY}AcornCharge")
        {
            Volume = 1f,
            PitchVariance = 0.2f,
            MaxInstances = 2,
        };
        Item.rare = ItemRarityID.Blue;

        Item.autoReuse = false;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.channel = true;
        Item.shootSpeed = 7f;

        Item.shoot = ModContent.ProjectileType<AcornLauncherProj>();
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 25f;
        if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
        {
            position += muzzleOffset;
        }
        Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<AcornLauncherProj>(), damage, knockback, player.whoAmI);
        return false;
    }

    public override bool CanUseItem(Player player)
    {
        return player.ownedProjectileCounts[ModContent.ProjectileType<AcornLauncherProj>()] <= 0;
    }
    public override void AddRecipes()
    {
        var recipe = CreateRecipe();
        recipe.AddRecipeGroup(RecipeGroupID.Wood, 16);
        recipe.AddIngredient(ModContent.ItemType<AlluviumOreItem>(), 7);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}