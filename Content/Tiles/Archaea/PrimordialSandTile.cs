using Reverie.Content.Projectiles.Archaea;

namespace Reverie.Content.Tiles.Archaea;

public class PrimordialSandTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBrick[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;

        Main.tileSand[Type] = true;
        TileID.Sets.Conversion.Sand[Type] = true;
        TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        TileID.Sets.Falling[Type] = true;
        TileID.Sets.Suffocate[Type] = true;
        TileID.Sets.FallingBlockProjectile[Type] = new TileID.Sets.FallingBlockProjectileInfo(ModContent.ProjectileType<PrimordialSandBallFallingProjectile>(), 10);

        TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.ChecksForMerge[Type] = true;
        MineResist = 0.5f;
        DustType = DustID.Sand;
        AddMapEntry(Color.SandyBrown);
    }

    public override bool HasWalkDust()
    {
        return Main.rand.NextBool(3);
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustID.Sand;
    }
}
