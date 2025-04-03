using Reverie.Common.Tiles;

namespace Reverie.Content.Tiles.Taiga;

public class TaigaGrassTile : GrassTile
{
    protected override int DirtType => ModContent.TileType<PeatTile>();

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Merge(TileID.IceBlock, TileID.SnowBlock, Type, ModContent.TileType<SnowTaigaGrassTile>());
        Main.tileBlockLight[Type] = true;

        DustType = DustID.Mud;
        HitSound = SoundID.Dig;

        TileID.Sets.SnowBiome[Type] = Type;
        AddMapEntry(new Color(88, 150, 112));
    }
}

public class SnowTaigaGrassTile : GrassTile
{
    protected override int DirtType => ModContent.TileType<PeatTile>();
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Merge(TileID.IceBlock, TileID.SnowBlock, Type, ModContent.TileType<TaigaGrassTile>());
        Main.tileBlockLight[Type] = true;

        DustType = DustID.Mud;
        HitSound = SoundID.Dig;

        VanillaFallbackOnModDeletion = TileID.SnowBlock;

        TileID.Sets.SnowBiome[Type] = Type;
        TileID.Sets.Snow[Type] = true;
        AddMapEntry(new Color(190, 223, 232));
    }

    public override bool HasWalkDust()
    {
        return true;
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustID.Snow;
    }
}