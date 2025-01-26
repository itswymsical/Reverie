using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Reverie;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using System;
using Terraria.Audio;
using Reverie.Core;

public class WardensArm : ModItem
{
    public override string Texture => Assets.Terraria.Items.WoodenWarden + Name;
    public override void SetDefaults()
    {
        Item.damage = 20;
        Item.width = 30;
        Item.height = 32;
        Item.useTime = Item.useAnimation = 30;
        Item.knockBack = 4.2f;
        Item.crit = 7;
        Item.mana = 6;
        Item.noUseGraphic = true;
        Item.value = Item.sellPrice(silver: 29);
        Item.rare = ItemRarityID.Blue;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.DD2_DarkMageHealImpact;
        Item.DamageType = DamageClass.MagicSummonHybrid;
        Item.shoot = ModContent.ProjectileType<WardensArmProj>();
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // Spawn arm projectile at random position around player
        float randomRotation = Main.rand.NextFloat(MathHelper.TwoPi);
        float distance = Main.rand.NextFloat(-50f, 90f);
        float x = player.Center.X + distance;
        Vector2 spawnPosition = player.Center - new Vector2(distance * player.direction, distance / 2);

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

public class WardensArmProj : ModProjectile
{
    public override string Texture => Assets.Terraria.Projectiles.WoodenWarden + Name;
    private Player Owner => Main.player[Projectile.owner];
    private bool Initialized;
    private Vector2 TargetPosition;
    private float ThrowProgress;
    private const float THROW_SPEED = 0.05f;
    private const int THROW_DURATION = 50;
    private EaseBuilder AnimationEase;

    public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.MagicSummonHybrid;
        Projectile.timeLeft = THROW_DURATION;
    }

    public override void AI()
    {
        if (!Initialized)
        {
            TargetPosition = Main.MouseWorld;

            // Setup animation sequence
            AnimationEase = new EaseBuilder();
            AnimationEase.AddPoint(0f, 0f, EaseFunction.Linear);
            AnimationEase.AddPoint(0.4f, MathHelper.ToRadians(-10), EaseFunction.EaseCubicIn);
            AnimationEase.AddPoint(1f, MathHelper.ToRadians(90), EaseFunction.EaseCubicOut);

            Initialized = true;
        }

        // Calculate throw animation progress (0 to 1)
        ThrowProgress = 1f - (Projectile.timeLeft / (float)THROW_DURATION);

        // Update rotation using EaseBuilder
        Projectile.rotation = (AnimationEase.Ease(ThrowProgress) * 2f) * Owner.direction;

        // Create boulder at specific point in animation
        if (Projectile.timeLeft == THROW_DURATION / 2)
        {
            Vector2 direction = TargetPosition - Projectile.Center;
            direction.Normalize();

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                direction * 15f,
                ModContent.ProjectileType<AlluvialBoulderProj>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );

            // Create throw effect
            for (int i = 0; i < 10; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, speed.X, speed.Y);
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Vector2 origin = texture.Size() / 2f;
        float scale = 1f + MathF.Sin(ThrowProgress * MathHelper.Pi) * 0.2f;

        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            lightColor,
            Projectile.rotation,
            origin,
            scale,
            SpriteEffects.None,
            0f
        );

        return false;
    }
}

public class AlluvialBoulderProj : ModProjectile
{
    public override string Texture => Assets.Terraria.Projectiles.WoodenWarden + Name;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.tileCollide = true;
        Projectile.penetrate = 1;
        Projectile.DamageType = DamageClass.Magic;
    }

    public override void AI()
    {
        Projectile.rotation += 0.2f * Projectile.velocity.X;

        Projectile.velocity.Y += 0.3f;

        if (Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.Dirt,
                Projectile.velocity.X * 0.2f,
                Projectile.velocity.Y * 0.2f,
                100,
                default,
                1.2f
            );
            dust.noGravity = true;
        }
    }
    public override void OnKill(int timeLeft)
    {
        // Create impact effect
        for (int i = 0; i < 15; i++)
        {
            Vector2 speed = Main.rand.NextVector2Circular(5f, 5f);
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, speed.X, speed.Y);
        }

        // Play impact sound
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Vector2 origin = texture.Size() / 2f;

        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            lightColor,
            Projectile.rotation,
            origin,
            1f,
            SpriteEffects.None,
            0f
        );

        return false;
    }
}