namespace Reverie.Common.Systems;

internal class TileCountSystem : ModSystem
{
    public int canopyBlockCount;
    public int emberiteCavernsBlockCount;

    public static TileCountSystem Instance => ModContent.GetInstance<TileCountSystem>();

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        canopyBlockCount = tileCounts[TileID.LivingWood];
        //emberiteCavernsBlockCount = tileCounts[ModContent.TileType<CarbonTile>()];
    }
}
