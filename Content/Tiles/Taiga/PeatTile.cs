using Reverie.Common.Systems;

namespace Reverie.Content.Tiles.Taiga;
public class PeatTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        this.Merge(TileID.ClayBlock, TileID.Stone, TileID.Ebonstone, TileID.Crimstone, 
            TileID.Slush, TileID.Grass, TileID.SnowBlock, ModContent.TileType<SnowTaigaGrassTile>(), TileID.GrayBrick);

        Main.tileBlockLight[Type] = true;
        MineResist = 0.5f;
        DustType = DustID.Mud;
        HitSound = SoundID.Dig;
        VanillaFallbackOnModDeletion = TileID.Dirt;

        AddMapEntry(new Color(126, 95, 74));
    }
}