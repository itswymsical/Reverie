namespace Reverie.Content.Tiles.Canopy;
public class ClayLoamTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[TileID.SnowBlock][Type] = true;
        Main.tileMerge[Type][TileID.SnowBlock] = true;

        Main.tileMerge[TileID.Stone][Type] = true;
        Main.tileMerge[Type][TileID.Stone] = true;

        Main.tileMerge[TileID.IceBlock][Type] = true;
        Main.tileMerge[Type][TileID.IceBlock] = true;

        Main.tileMerge[TileID.Ebonstone][Type] = true;
        Main.tileMerge[Type][TileID.Ebonstone] = true;
        Main.tileMerge[TileID.Crimstone][Type] = true;
        Main.tileMerge[Type][TileID.Crimstone] = true;


        Main.tileMerge[TileID.ClayBlock][Type] = true;
        Main.tileMerge[Type][TileID.ClayBlock] = true;

        Main.tileMerge[TileID.Silt][Type] = true;
        Main.tileMerge[Type][TileID.Silt] = true;

        Main.tileMerge[ModContent.TileType<CanopyGrassTile>()][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<CanopyGrassTile>()] = true;
        Main.tileBlockLight[Type] = true;

        MineResist = 0.5f;
        DustType = DustID.Clay;
        HitSound = SoundID.Dig;
        VanillaFallbackOnModDeletion = TileID.ClayBlock;

        AddMapEntry(new Color(164, 100, 93));
    }
}