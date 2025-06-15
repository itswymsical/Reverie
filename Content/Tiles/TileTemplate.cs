namespace Reverie.Content.Tiles;

public class TileTemplate : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileMerge[TileID.Stone][Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Dirt;

        HitSound = SoundID.Dig;
        AddMapEntry(new Color(255, 255, 255));
    }
}
