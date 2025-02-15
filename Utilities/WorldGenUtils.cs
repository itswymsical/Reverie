using Reverie.Content.Tiles;
using Reverie.lib;
using System.Collections.Generic;

namespace Reverie.Utilities;

/// <summary>
/// Provides utility methods for world generation tasks.
/// </summary>
public static class WorldGenUtils
{

    public readonly record struct OreConfig(ushort TileType, int MinSize, int MaxSize, int MinHeight);

    #region Validation
    /// <summary>
    /// Checks if a location is valid for ore placement.
    /// </summary>
    public static bool IsValidOreLocation(int x, int y, HashSet<ushort>? validBlocks = null)
    {
        var tile = Main.tile[x, y];
        if (!tile.HasTile) return false;
        return validBlocks?.Contains(tile.TileType) ?? true;
    }

    /// <summary>
    /// Checks if coordinates are within world bounds.
    /// </summary>
    private static bool IsInWorld(int x, int y) =>
        x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY;
    #endregion

    #region Helper Methods
    /// <summary>
    /// Calculates the depth percentage at a given Y coordinate.
    /// </summary>
    public static float GetDepthPercentage(int y) =>
        (float)y / Main.maxTilesY;

    /// <summary>
    /// Gets a scaled value based on depth.
    /// </summary>
    public static float GetDepthScaledValue(int y, float minValue, float maxValue)
    {
        float depthPercent = GetDepthPercentage(y);
        return minValue + (maxValue - minValue) * depthPercent;
    }

    /// <summary>
    /// Creates a circular pattern of tiles.
    /// </summary>
    public static void MakeCircle(Tile pos, int centerX, int centerY, int radius, ushort tileType)
    {
        int rSquared = radius * radius;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= rSquared)
                {
                    int worldX = centerX + x;
                    int worldY = centerY + y;
                    if (IsInWorld(worldX, worldY))
                    {
                        pos.TileType = tileType;
                        pos.HasTile = true;
                    }
                }
            }
        }
    }
    #endregion
}