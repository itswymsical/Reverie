using Reverie.Common.Systems;

namespace Reverie.Content.Tiles.Taiga;
public class PeatTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileBlockLight[Type] = true;

        this.Merge(ModContent.TileType<SnowTaigaGrassTile>(), TileID.Slush, TileID.ClayBlock,
            TileID.Crimstone, TileID.Ebonstone, TileID.IceBlock, TileID.Stone, TileID.SnowBlock, TileID.Grass);

        MineResist = 0.5f;
        DustType = DustID.Mud;
        HitSound = SoundID.Dig;
        VanillaFallbackOnModDeletion = TileID.Dirt;

        AddMapEntry(new Color(126, 95, 74));
    }
}