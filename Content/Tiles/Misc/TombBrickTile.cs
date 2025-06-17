namespace Reverie.Content.Tiles;

public class TombBrickTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[TileID.Stone][Type] = true;
        Main.tileMerge[Type][TileID.Stone] = true;

        Main.tileMerge[TileID.GrayBrick][Type] = true;
        Main.tileMerge[Type][TileID.GrayBrick] = true;

        Main.tileBlockLight[Type] = true;

        DustType = DustID.Stone;
        MineResist = 1f;
        HitSound = SoundID.Dig;

        AddMapEntry(new Color(92, 88, 86));
    }
}
