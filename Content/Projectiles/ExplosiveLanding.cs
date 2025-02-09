using Terraria.Audio;

namespace Reverie.Content.Projectiles;

public class ExplosiveLanding : ModProjectile
{
    private const int DEFAULT_WIDTH_HEIGHT = 15;
    private const int EXPLOSION_WIDTH_HEIGHT = 80;
    public override string Texture => "Terraria/Images/Star_4";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.Explosive[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = DEFAULT_WIDTH_HEIGHT;
        Projectile.height = DEFAULT_WIDTH_HEIGHT;
        Projectile.friendly = true;
        Projectile.penetrate = -1;

        Projectile.timeLeft = 5;

        DrawOffsetX = -2;
        DrawOriginOffsetY = -5;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.timeLeft = 0;
        Projectile.PrepareBombToBlow();

        if (Projectile.soundDelay == 0)
        {
            SoundEngine.PlaySound(SoundID.Item14);
        }
        Projectile.soundDelay = 10;

        return false;
    }

    public override void AI()
    {
        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
        {
            Projectile.PrepareBombToBlow();
        }
    }

    public override void PrepareBombToBlow()
    {
        Projectile.tileCollide = false;
        Projectile.alpha = 255;
        Projectile.Resize(EXPLOSION_WIDTH_HEIGHT, EXPLOSION_WIDTH_HEIGHT);
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
        // Smoke Dust spawn
        for (var i = 0; i < 50; i++)
        {
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
            dust.velocity *= 1.4f;
        }

        // Fire Dust spawn
        for (var i = 0; i < 80; i++)
        {
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
            dust.noGravity = true;
            dust.velocity *= 5f;
            dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
            dust.velocity *= 3f;
        }

        // Large Smoke Gore spawn
        for (var g = 0; g < 2; g++)
        {
            var goreSpawnPosition = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);
            var gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y -= 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y -= 1.5f;
        }
        Projectile.Resize(DEFAULT_WIDTH_HEIGHT, DEFAULT_WIDTH_HEIGHT);

        if (Projectile.owner == Main.myPlayer)
        {
            var explosionRadius = 7; // Bomb: 4, Dynamite: 7, Explosives & TNT Barrel: 10
            var minTileX = (int)(Projectile.Center.X / 16f - explosionRadius);
            var maxTileX = (int)(Projectile.Center.X / 16f + explosionRadius);
            var minTileY = (int)(Projectile.Center.Y / 16f - explosionRadius);
            var maxTileY = (int)(Projectile.Center.Y / 16f + explosionRadius);

            // Ensure that all tile coordinates are within the world bounds
            Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);

            // These 2 methods handle actually mining the tiles and walls while honoring tile explosion conditions
            var explodeWalls = Projectile.ShouldWallExplode(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY);
            Projectile.ExplodeTiles(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY, explodeWalls);
        }
    }
}