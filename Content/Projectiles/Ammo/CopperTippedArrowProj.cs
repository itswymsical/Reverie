using Reverie.Utilities.Extensions;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.Projectiles.Ammo;

public class CopperTippedArrowProj : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 10;

        Projectile.arrow = true;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 1200;
    }

    public override void AI()
    {
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
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        DustExtensions.SpawnDustCloud(Projectile.position, Projectile.width, Projectile.height, DustID.Copper, scale: 0.7f);
        for (var i = 0; i < 3; ++i)
        {
            Projectile.NewProjectile(default, Projectile.Center, -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * 4f,
                ModContent.ProjectileType<CopperShrapnelProj>(), (int)(Projectile.damage * 0.4f), 0.2f, Projectile.owner);
        }
    }
}

public class CopperShrapnelProj : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 10;
        Projectile.aiStyle = 1;
        Projectile.friendly = Projectile.tileCollide = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 120;
        Projectile.penetrate = 1;
        Projectile.scale = 0.79f;
    }
    public override void AI()
    {
        Projectile.ai[1] += 1f;
        if (Projectile.ai[1] > 40f)
        {
            Projectile.Kill();
        }
        Projectile.velocity.Y = Projectile.velocity.Y + 0.2f;
        if (Projectile.velocity.Y > 18f)
        {
            Projectile.velocity.Y = 18f;
        }
        Projectile.velocity.X = Projectile.velocity.X * 0.98f;
        return;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Main.instance.LoadProjectile(Projectile.type);
        var texture = TextureAssets.Projectile[Projectile.type].Value;

        Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (var k = 0; k < Projectile.oldPos.Length; k++)
        {
            var drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            var color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        return true;
    }

    public override void OnKill(int timeLeft)
    {
        DustExtensions.SpawnDustCloud(Projectile.position, Projectile.width, Projectile.height, DustID.Copper, scale: 0.4f);
    }
}