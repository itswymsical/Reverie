using Reverie.Common.Systems;

namespace Reverie.Content.Tiles.Taiga;
public class PeatTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        this.Merge(TileID.ClayBlock, TileID.Stone, TileID.Ebonstone, TileID.Crimstone, 
            TileID.Slush, TileID.Grass, TileID.SnowBlock, ModContent.TileType<SnowTaigaGrassTile>());

        Main.tileBlockLight[Type] = true;

        MineResist = 0.5f;
        DustType = DustID.Mud;
        HitSound = SoundID.Dig;
        VanillaFallbackOnModDeletion = TileID.Dirt;

        AddMapEntry(new Color(126, 95, 74));
    }
    //public override void RandomUpdate(int i, int j)
    //{
    //    if (!Main.rand.NextBool(6)) return;

    //    int[] directions = { -1, 1 };
    //    foreach (int xDir in directions)
    //    {
    //        int x = i + xDir;
    //        if (x < 0 || x >= Main.maxTilesX) continue;

    //        foreach (int yDir in directions)
    //        {
    //            int y = j + yDir;
    //            if (y < 0 || y >= Main.maxTilesY) continue;

    //            Tile tile = Main.tile[x, y];
    //            if (tile.HasTile && tile.TileType == Type)
    //            {
    //                if (!Main.tile[x, y - 1].HasTile || !Main.tileSolid[Main.tile[x, y - 1].TileType])
    //                {
    //                    tile.TileType = (ushort)ModContent.TileType<TaigaGrassTile>();

    //                    if (Main.netMode == NetmodeID.Server)
    //                        NetMessage.SendTileSquare(-1, x, y, 1);
    //                }
    //            }
    //        }
    //    }
    //}
}