using Reverie.Common.Tiles;

namespace Reverie.Content.Tiles.Canopy;

public class CanopyGrassTile : GrassTile
{
    protected override int DirtType => ModContent.TileType<ClayLoamTile>();
    //public override List<int> PlantTypes => [ModContent.TileType<SnowTaigaPlants>()];
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Merge(TileID.Mud, TileID.Stone, Type, DirtType, TileID.Sand, TileID.ClayBlock, TileID.Silt);
        Main.tileBlockLight[Type] = true;

        DustType = DustID.JungleGrass;
        HitSound = SoundID.Dig;

        VanillaFallbackOnModDeletion = TileID.JungleGrass;

        AddMapEntry(new Color(113, 186, 39));
    }

    public override bool HasWalkDust()
    {
        return true;
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustID.JungleGrass;
    }
}
