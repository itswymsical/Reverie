namespace Reverie.Content.Projectiles.Misc;

public class MagnoliaPheromonesProj : ModProjectile
{
    public override string Texture => INVIS;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 8;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    private int delay = 0;
    public override bool? CanCutTiles() => false;
    
    public override void AI()
    {
        base.AI();
        delay++;

        if (delay >= 2)
        {
            Vector2 tilePos = Projectile.Center / 16f;
            if (Projectile.Hitbox.Intersects(
                new Rectangle((int)(tilePos.X - 0.5f) * 16, (int)(tilePos.Y - 0.5f) * 16, 16, 16)))
            {
                int i = (int)tilePos.X;
                int j = (int)tilePos.Y;

                Tile tile = Main.tile[i, j];

                if (tile != null && tile.HasTile && (tile.TileType == 81 || tile.TileType == 82 || tile.TileType == 83))
                {
                    if (tile.TileType == 81)
                        tile.TileType = 82;
                    else if (tile.TileType == 82)
                        tile.TileType = 83;
                    else if (tile.TileType == 83)
                        tile.TileType = 84;
                }
            }
            delay = 0;
        }

        for (int count = 0; count < 16; count++)
        {
            Dust dust7 = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, Projectile.velocity.X, Projectile.velocity.Y, 4)];

            dust7.noGravity = count % 3 != 0;
            if (!dust7.noGravity)
            {
                Dust dust2 = dust7;
                dust2.scale *= 1.25f;
                dust2 = dust7;
                dust2.velocity /= 2f;
                dust7.velocity.Y -= 2.2f;
            }
        }
    }
}