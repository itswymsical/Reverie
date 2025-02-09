using Reverie.Core.Animation;
using Reverie.Core.Graphics;

using System.Collections.Generic;

using Terraria.Audio;
using Terraria.GameContent;

using static Reverie.Reverie;

namespace Reverie.Content.Projectiles.Sylvanwalde.WoodenWarden;
public class WardensArmProj : ModProjectile
{
    private Player Owner => Main.player[Projectile.owner];
    private bool Initialized;
    private Vector2 TargetPosition;
    private float ThrowProgress;
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

            AnimationEase = new EaseBuilder();
            AnimationEase.AddPoint(0f, 0f, EaseFunction.Linear);
            AnimationEase.AddPoint(0.4f, MathHelper.ToRadians(-10), EaseFunction.EaseCubicIn);
            AnimationEase.AddPoint(1f, MathHelper.ToRadians(90), EaseFunction.EaseCubicOut);

            Initialized = true;
        }

        ThrowProgress = 1f - Projectile.timeLeft / (float)THROW_DURATION;

        Projectile.rotation = AnimationEase.Ease(ThrowProgress) * 2f * Owner.direction;

        if (Projectile.timeLeft == THROW_DURATION / 2)
        {
            var direction = TargetPosition - Projectile.Center;
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

            for (var i = 0; i < 10; i++)
            {
                var speed = Main.rand.NextVector2Circular(3f, 3f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, speed.X, speed.Y);
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Projectile.type].Value;
        var origin = texture.Size() / 2f;
        var scale = 1f + MathF.Sin(ThrowProgress * MathHelper.Pi) * 0.2f;

        var flip = SpriteEffects.FlipHorizontally;
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
    private readonly List<Vector2> oldPosition = [];

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

        oldPosition.Add(Projectile.Center);
        if (oldPosition.Count > 4)
            oldPosition.RemoveAt(0);

        ManageCaches();
        ManageTrail();

        if (Main.rand.NextBool(2))
        {
            var dust = Dust.NewDustDirect(
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
        if (cache is null)
        {
            cache = [];
            for (var i = 0; i < 4; i++)
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
        trail ??= new Trail(Main.instance.GraphicsDevice, 4, new NoTip(), factor => 12, factor =>
        {
            var progress = factor.Length();
            var opacity = (1f - progress) * 0.15f;
            return Color.DarkGray * opacity;
        });
        trail.Positions = [.. cache];
        trail.NextPosition = Projectile.Center;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Projectile.type].Value;
        var origin = texture.Size() / 2f;

        for (var k = 0; k < oldPosition.Count; k++)
        {
            var progress = k / (float)oldPosition.Count;
            var color = lightColor * (1f - -progress) * 0.1f;

            var position = oldPosition[k];
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
        for (var i = 0; i < 15; i++)
        {
            var speed = Main.rand.NextVector2Circular(5f, 5f);
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, speed.X, speed.Y);
        }

        for (var i = 0; i < 3; i++)
        {
            var speed = Main.rand.NextVector2Circular(5f, 5f);
            Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.position, speed, Mod.Find<ModGore>($"{Name}_Gore_{i}").Type, 1f);
            Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.position, speed, Mod.Find<ModGore>($"{Name}_Gore_{i}").Type, 0.75f);
        }

        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}BoulderImpact")
        {
            Volume = 1f,
            PitchVariance = 0.2f,
            MaxInstances = 3,
        }, Projectile.position);
    }
}
