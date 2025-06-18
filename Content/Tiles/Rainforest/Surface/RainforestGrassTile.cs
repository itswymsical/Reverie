using Reverie.Common.Tiles;
using Reverie.Content.Tiles.Taiga;
using System.Collections.Generic;

namespace Reverie.Content.Tiles.Rainforest.Surface;

public class RainforestGrassTile : GrassTile
{
    protected override int DirtType => ModContent.TileType<OxisolTile>();
    //public override List<int> PlantTypes => [ModContent.TileType<SnowTaigaPlants>()];
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Merge(TileID.Mud, TileID.ClayBlock, Type, TileID.Sand);
        Main.tileBlockLight[Type] = true;

        DustType = DustID.JungleGrass;
        HitSound = SoundID.Dig;

        VanillaFallbackOnModDeletion = TileID.JungleGrass;

        AddMapEntry(new Color(100, 170, 33));
    }

    public override bool HasWalkDust()
    {
        return true;
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustID.JungleGrass;
    }
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!effectOnly)
        {
            fail = true;
            Framing.GetTileSafely(i, j).TileType = (ushort)ModContent.TileType<OxisolTile>();
        }
    }
}
