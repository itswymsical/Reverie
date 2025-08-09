using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.NPCs.Bosses.KingSlime;

public class GelBallProjectile : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 2;
        ProjectileID.Sets.TrailingMode[Type] = 0;
        Main.projFrames[Type] = 4;
    }

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.aiStyle = ProjAIStyleID.Arrow;
        Projectile.penetrate = 1;
        Projectile.alpha = 100;
        Projectile.timeLeft = 300;
        Projectile.scale = 1.25f;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        int tileX = (int)(Projectile.Center.X / 16f);
        int tileY = (int)(Projectile.Center.Y / 16f);

        for (int x = tileX - 1; x <= tileX + 1; x++)
        {
            for (int y = tileY - 1; y <= tileY + 1; y++)
            {
                if (WorldGen.InWorld(x, y))
                {
                    Tile tile = Framing.GetTileSafely(x, y);

                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        SlimedTileSystem.AddSlimedTile(x, y);
                    }
                }
            }
        }

        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeConsume") with { Pitch = -0.3f ,Volume = 0.33f}, Projectile.position);
        for (int i = 0; i < 30; i++)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.position, DustID.t_Slime, -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * 4f, 100, 
                new Color(86, 162, 255, 100), 1.4f);
            dust.noGravity = true;
        }

        return true;
    }

    public override void AI()
    {
        base.AI();
        Projectile.frameCounter++;
        if (Projectile.frameCounter >= 5)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(BuffID.Slow, 5 * 60);
        target.AddBuff(BuffID.Slimed, 5 * 60);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.instance.LoadProjectile(Projectile.type);
        var texture = TextureAssets.Projectile[Projectile.type].Value;

        var frameHeight = texture.Height / Main.projFrames[Projectile.type];
        var sourceRectangle = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

        Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (var k = 0; k < Projectile.oldPos.Length; k++)
        {
            var drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            var color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, sourceRectangle, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        return true;
    }
}