namespace Reverie.Content.Tiles.Rainforest.Surface;
public class OxisolTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[TileID.Mud][Type] = true;
        Main.tileMerge[Type][TileID.Mud] = true;

        Main.tileMerge[TileID.Stone][Type] = true;
        Main.tileMerge[Type][TileID.Stone] = true;

        Main.tileMerge[TileID.Sand][Type] = true;
        Main.tileMerge[Type][TileID.Sand] = true;

        Main.tileMerge[TileID.ClayBlock][Type] = true;
        Main.tileMerge[Type][TileID.ClayBlock] = true;

        Main.tileMerge[ModContent.TileType<RainforestGrassTile>()][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<RainforestGrassTile>()] = true;

        Main.tileBlockLight[Type] = true;

        DustType = DustID.Clay;
        HitSound = SoundID.Dig;
        VanillaFallbackOnModDeletion = TileID.Mud;

        AddMapEntry(new Color(118, 78, 78));
    }
}