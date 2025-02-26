namespace Reverie.Content.Tiles;

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

        AddMapEntry(new Color(137, 130, 116));
    }
}
