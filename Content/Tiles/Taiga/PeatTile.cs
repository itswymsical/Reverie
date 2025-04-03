namespace Reverie.Content.Tiles.Taiga;
public class PeatTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[TileID.SnowBlock][Type] = true;
        Main.tileMerge[Type][TileID.SnowBlock] = true;

        Main.tileMerge[TileID.IceBlock][Type] = true;
        Main.tileMerge[Type][TileID.IceBlock] = true;

        Main.tileMerge[TileID.ClayBlock][Type] = true;
        Main.tileMerge[Type][TileID.ClayBlock] = true;

        Main.tileMerge[TileID.Slush][Type] = true;
        Main.tileMerge[Type][TileID.Slush] = true;

        Main.tileMerge[ModContent.TileType<TaigaGrassTile>()][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<TaigaGrassTile>()] = true;
        Main.tileBlockLight[Type] = true;

        DustType = DustID.Mud;
        HitSound = SoundID.Dig;

        AddMapEntry(new Color(126, 95, 74));
    }
}