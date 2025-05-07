namespace Reverie.Common.Tiles;

internal class TileCounts : ModSystem
{
    public int canopyBlockCount;
    //public int emberiteCavernsBlockCount;

    public static TileCounts Instance => ModContent.GetInstance<TileCounts>();

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        canopyBlockCount = tileCounts[TileID.LivingWood];
        //emberiteCavernsBlockCount = tileCounts[ModContent.TileType<CarbonTile>()];
    }
}
