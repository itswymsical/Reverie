namespace Reverie.Content.Tiles;

public class WoodWickerTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[TileID.WoodBlock][Type] = true;
        Main.tileMerge[Type][TileID.WoodBlock] = true;

        Main.tileMerge[TileID.LivingWood][Type] = true;
        Main.tileMerge[Type][TileID.LivingWood] = true;

        Main.tileBlockLight[Type] = true;

        DustType = DustID.WoodFurniture;
        MineResist = 1f;
        HitSound = SoundID.Dig;

        AddMapEntry(new Color(178, 131, 94));
    }
}
