namespace Reverie.Utilities.Extensions;

/// <summary>
///     Provides <see cref="Player"/> extension methods.
/// </summary>
public static class PlayerExtensions
{
    /// <summary>
    ///     Attemps to dig a <see cref="Tile"/> area at the specified coordinates.
    /// </summary>
    /// <param name="i">The horizontal coordinates of the origin.</param>
    /// <param name="j">The vertical coordinates of the origin.</param>
    /// <param name="width">The width of the area, in tiles.</param>
    /// <param name="height">The height of the area, in tiles.</param>
    /// <param name="power">The digging power to use.</param>
    public static void DigArea(this Player player, int i, int j, int width, int height, int power)
    {
        for (int x = i; x < i + width; x++)
        {
            for (int y = j; y < j + height; y++)
            {
                player.DigTile(x, y, power);
            }
        }
    }
        
    /// <summary>
    ///     Attempts to dig a <see cref="Tile"/> at the specified coordinates.
    /// </summary>
    /// <param name="i">The horizontal coordinates of the tile.</param>
    /// <param name="j">The vertical coordinates of the tile.</param>
    /// <param name="power">The digging power to use.</param>
    public static void DigTile(this Player player, int i, int j, int power)
    {
        var type = WorldGen.TileType(i, j);

        if (Main.tileAxe[type] || Main.tileHammer[type])
        {
            return;
        }
        
        player.PickTile(i, j, power);
        
        if (TileID.Sets.CanBeDugByShovel[type])
        {
            return;
        }
        
        player.PickTile(i, j, power - (power / 2));
    }
}