namespace Reverie.Content.Tiles.Misc;

public class LodestoneTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileMerge[TileID.Stone][Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Tin;
        MineResist = 2f;
        MinPick = 42;
        HitSound = SoundID.Tink;
        Main.tileSpelunker[Type] = true;
        AddMapEntry(new Color(137, 130, 116));
    }
    public override bool IsTileSpelunkable(int i, int j) => true;   
}
