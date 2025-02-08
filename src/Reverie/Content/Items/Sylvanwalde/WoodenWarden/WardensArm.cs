using Terraria.DataStructures;

using Reverie.Content.Projectiles.Sylvanwalde.WoodenWarden;

namespace Reverie.Content.Items.Sylvanwalde.WoodenWarden;

public class WardensArm : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 17;
        Item.width = 30;
        Item.height = 32;
        Item.useTime = Item.useAnimation = 34;
        Item.knockBack = 4.2f;
        Item.crit = 21;
        Item.mana = 6;
        Item.noUseGraphic = true;
        Item.value = Item.sellPrice(silver: 88);
        Item.rare = ItemRarityID.Blue;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.DD2_DarkMageHealImpact;
        Item.DamageType = DamageClass.Magic;
        Item.shoot = ModContent.ProjectileType<WardensArmProj>();
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var randomRotation = Main.rand.NextFloat(MathHelper.TwoPi);
        var distanceX = Main.rand.NextFloat(30f, 90f);
        var distanceY = Main.rand.NextFloat(20f, 90f);
        var x = player.Center.X + distanceX;
        var spawnPosition = player.Center - new Vector2(distanceX * player.direction, distanceY / 2);

        Projectile.NewProjectile(
            source,
            spawnPosition,
            Vector2.Zero,
            type,
            damage,
            knockback,
            player.whoAmI
        );

        return false;
    }
}

