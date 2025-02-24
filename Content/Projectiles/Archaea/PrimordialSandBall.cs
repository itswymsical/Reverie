using Reverie.Content.Items.Archaea;
using Reverie.Content.Tiles.Archaea;

namespace Reverie.Content.Projectiles.Archaea;

public abstract class PrimordialSandBallProjectile : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.FallingBlockDoesNotFallThroughPlatforms[Type] = true;
        ProjectileID.Sets.ForcePlateDetection[Type] = true;
    }
}

public class PrimordialSandBallFallingProjectile : PrimordialSandBallProjectile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ProjectileID.Sets.FallingBlockTileItem[Type] = new(ModContent.TileType<PrimordialSandTile>(), ModContent.ItemType<PrimordialSandItem>());
    }

    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.SandBallFalling);
    }
}

public class PrimordialSandBallGunProjectile : PrimordialSandBallProjectile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ProjectileID.Sets.FallingBlockTileItem[Type] = new(ModContent.TileType<PrimordialSandTile>());
    }

    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.SandBallGun);
        AIType = ProjectileID.SandBallGun;
    }
}
