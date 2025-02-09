using Reverie.Core.Graphics;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.Projectiles.Sylvanwalde.WoodenWarden;

public class BrambleArrowProj : ModProjectile
{
    public int MaxStickies => 3;
    private bool isStuck = false;

    protected NPC Target => Main.npc[(int)Projectile.ai[1]];

    protected bool stickToNPC;

    protected bool stickingToNPC;

    private Vector2 offset;

    private float oldRotation;

    private Trail trail;
    private List<Vector2> cache;
    private List<Vector2> oldPosition = [];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 4;
        ProjectileID.Sets.TrailingMode[Type] = 4;
    }
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;

        stickToNPC =
            Projectile.tileCollide =
            Projectile.arrow =
            Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Ranged;

        Projectile.localNPCHitCooldown = 20;
        Projectile.timeLeft = 660;
        Projectile.penetrate = 3;
    }
    public override void AI()
    {
        oldPosition.Add(Projectile.Center);
        if (oldPosition.Count > 4)
            oldPosition.RemoveAt(0);

        ManageCaches();
        ManageTrail();

        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] >= 15f)
        {
            Projectile.ai[0] = 15f;
            Projectile.velocity.Y += 0.1f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        if (Projectile.velocity.Y > 16f)
        {
            Projectile.velocity.Y = 16f;
        }

        if (stickingToNPC)
        {
            if (Target.active && !Target.dontTakeDamage)
            {
                Projectile.tileCollide = false;

                Projectile.Center = Target.Center - offset;

                Projectile.gfxOffY = Target.gfxOffY;
            }
            else
            {
                Projectile.Kill();
            }
        }

        if (stickingToNPC)
            Projectile.rotation = oldRotation;

        var player = Main.player[Projectile.owner];
        var distanceToPlayer = Vector2.Distance(Projectile.Center, player.Center);

        Projectile.spriteDirection = Projectile.direction;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!stickingToNPC && stickToNPC)
        {
            Projectile.ai[1] = target.whoAmI;

            oldRotation = Projectile.rotation;

            offset = target.Center - Projectile.Center + Projectile.velocity * 0.75f;

            stickingToNPC = true;

            Projectile.netUpdate = true;
        }
        else
        {
            RemoveStackProjectiles();
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(SoundID.Item50, target.position);
        isStuck = true;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        for (var i = 0; i < 5; i++)
        {
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.t_LivingWood);
            dust.noGravity = true;
            dust.velocity *= 1.5f;
            dust.scale *= 0.9f;
        }
    }

    private void ManageCaches()
    {
        if (cache == null)
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
        var origin = texture.Size() / 1.8f;

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

    protected void RemoveStackProjectiles()
    {
        var sticking = new Point[MaxStickies];
        var index = 0;

        for (var i = 0; i < Main.maxProjectiles; i++)
        {
            var currentProjectile = Main.projectile[i];

            if (i != Projectile.whoAmI
                && currentProjectile.active
                && currentProjectile.owner == Main.myPlayer
                && currentProjectile.type == Projectile.type
                && currentProjectile.ai[0] == 1f
                && currentProjectile.ai[1] == Target.whoAmI
            )
            {
                sticking[index++] = new Point(i, currentProjectile.timeLeft);

                if (index >= sticking.Length)
                    break;
            }
        }

        if (index >= sticking.Length)
        {
            var oldIndex = 0;

            for (var i = 1; i < sticking.Length; i++)
                if (sticking[i].Y < sticking[oldIndex].Y)
                    oldIndex = i;

            Main.projectile[sticking[oldIndex].X].Kill();
        }
    }
}
