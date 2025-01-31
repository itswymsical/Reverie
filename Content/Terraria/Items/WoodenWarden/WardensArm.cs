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
using Reverie.Core.PrimitiveDrawing;
using System.Collections.Generic;

public class WardensArm : ModItem
{
    public override string Texture => Assets.Terraria.Items.WoodenWarden + Name;
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
        // Spawn arm projectile at random position around player
        float randomRotation = Main.rand.NextFloat(MathHelper.TwoPi);
        float distanceX = Main.rand.NextFloat(30f, 90f);
        float distanceY = Main.rand.NextFloat(20f, 90f);
        float x = player.Center.X + distanceX;
        Vector2 spawnPosition = player.Center - new Vector2(distanceX * player.direction, distanceY / 2);

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

        SpriteEffects flip = SpriteEffects.FlipHorizontally;
        if (Projectile.position.X < Owner.Center.X)
            flip = SpriteEffects.None;
        
        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            lightColor,
            Projectile.rotation,
            origin,
            scale,
            flip,
            0f
        );

        return false;
    }
}

public class AlluvialBoulderProj : ModProjectile
{
    private Trail trail;
    private List<Vector2> cache;
    private List<Vector2> oldPosition = [];

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

        // Store position history for trail
        oldPosition.Add(Projectile.Center);
        if (oldPosition.Count > 4)
            oldPosition.RemoveAt(0);

        // Trail management
        ManageCaches();
        ManageTrail();

        // Dust effects
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

    private void ManageCaches()
    {
        if (cache == null)
        {
            cache = [];
            for (int i = 0; i < 4; i++)
            {
                cache.Add(Projectile.Center);
            }
        }

        cache.Add(Projectile.Center);

        while (cache.Count > 4)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        if (trail is null)
        {
            trail = new Trail(Main.instance.GraphicsDevice, 4, new NoTip(), factor => 12, factor =>
            {
                float progress = factor.Length(); // or factor.X if you want just one component
                float opacity = (1f - progress) * 0.15f;
                return Color.DarkGray * opacity;
            });
        }
        trail.Positions = cache.ToArray();
        trail.NextPosition = Projectile.Center;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Vector2 origin = texture.Size() / 2f;

        // Draw shadow trail
        for (int k = 0; k < oldPosition.Count; k++)
        {
            float progress = k / (float)oldPosition.Count;
            Color color = lightColor * (1f - -progress) * 0.1f; // Fade out as trail gets older

            Vector2 position = oldPosition[k];
            Main.spriteBatch.Draw(
                texture,
                position - Main.screenPosition,
                null,
                color,
                Projectile.rotation,
                origin,
                1f - (-progress * 0.01f),
                SpriteEffects.None,
                0f
            );
        }

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

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 15; i++)
        {
            Vector2 speed = Main.rand.NextVector2Circular(5f, 5f);
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, speed.X, speed.Y);
        }

        for (int i = 0; i < 3; i++)
        {
            Vector2 speed = Main.rand.NextVector2Circular(5f, 5f);
            Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.position, speed, Mod.Find<ModGore>($"{Name}_Gore_{i}").Type, 1f);
            Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.position, speed, Mod.Find<ModGore>($"{Name}_Gore_{i}").Type, 0.75f);
        }

        SoundEngine.PlaySound(new SoundStyle($"{Assets.SFX.Directory}BoulderImpact")
        {
            Volume = 1f,
            PitchVariance = 0.2f,
            MaxInstances = 3,
        }, Projectile.position);
    }
}

public class AlluvialBoulderProj_Gore_0 : ModGore
{
    public override string Texture => Assets.Gores.Warden + Name;
    public override void SetStaticDefaults()
    {
        ChildSafety.SafeGore[Type] = true;
    }
}
public class AlluvialBoulderProj_Gore_1 : ModGore
{
    public override string Texture => Assets.Gores.Warden + Name;
    public override void SetStaticDefaults()
    {
        ChildSafety.SafeGore[Type] = true;
    }
}
public class AlluvialBoulderProj_Gore_2 : ModGore
{
    public override string Texture => Assets.Gores.Warden + Name;
    public override void SetStaticDefaults()
    {
        ChildSafety.SafeGore[Type] = true;
    }
}
public class AlluvialBoulderProj_Gore_3 : ModGore
{
    public override string Texture => Assets.Gores.Warden + Name;
    public override void SetStaticDefaults()
    {
        ChildSafety.SafeGore[Type] = true;
    }
}