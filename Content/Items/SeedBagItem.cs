using Reverie.Content.Projectiles;
using Terraria.DataStructures;

namespace Reverie.Content.Items;

public class SeedBagItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.MagicSummonHybrid;
        Item.damage = 3;
        Item.knockBack = 0.8f;
        Item.width = 24;
        Item.height = 32;
        Item.useTime = Item.useAnimation = 32;
        Item.useStyle = ItemUseStyleID.RaiseLamp;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = false;
        Item.noMelee = true;
        Item.mana = 8;
        Item.shootSpeed = 14f;
        Item.holdStyle = ItemHoldStyleID.HoldFront;
        Item.shoot = ModContent.ProjectileType<SeedProj>();
    }
    public override void HoldStyle(Player player, Rectangle heldItemFrame)
    {
        var armRotation = MathHelper.ToRadians(80f);
        if (player.direction == 1)
        {
            player.itemLocation.X -= heldItemFrame.X + 14;
            player.itemLocation.Y += heldItemFrame.Y + 18;
            armRotation = -armRotation;
        }
        if (player.direction != 1)
        {
            player.itemLocation.X -= heldItemFrame.X - 14;
            player.itemLocation.Y += heldItemFrame.Y + 18;
        }
        Item.scale = 0.85f;
        Item.autoReuse = false;
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        var armRotation = MathHelper.ToRadians(80f);
        if (player.direction == 1)
        {
            player.itemLocation.X -= heldItemFrame.X + 4;
            player.itemLocation.Y += heldItemFrame.Y + 8;
            armRotation = -armRotation;
        }
        if (player.direction != 1)
        {
            player.itemLocation.X -= heldItemFrame.X - 4;
            player.itemLocation.Y += heldItemFrame.Y + 8;
        }
        Item.scale = 0.85f;
        Item.autoReuse = false;
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var numProjectiles = 3;
        var spreadAngle = MathHelper.ToRadians(8f);

        for (var i = 0; i < numProjectiles; i++)
        {
            var newVelocity = velocity.RotatedBy(MathHelper.Lerp(-spreadAngle / 2, spreadAngle / 2, i / (float)(numProjectiles - 1)));

            newVelocity *= 1f - Main.rand.NextFloat(0.2f);

            Projectile.NewProjectile(source, position, newVelocity, type, damage, knockback, player.whoAmI);
        }

        return false;
    }
}
