using ReLogic.Utilities;
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
    public static bool IsValidOreLocation(int x, int y, HashSet<ushort> validBlocks = null)
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

    /// <summary>
    /// Generates a vanilla mountain, modified to allow flexibilty and modifications to its geometry
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="centerX"></param>
    /// <param name="centerY"></param>
    /// <param name="baseRadius"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static bool VanillaMountain(int currentX, int currentY, int mountainCenterX, int mountainCenterY, int initialRadius, int mountainHeight)
    {
        double currentRadius = initialRadius;
        double adjustedRadius = currentRadius;
        double remainingHeight = mountainHeight;
        Vector2D mountainCenter = new(mountainCenterX, mountainCenterY + remainingHeight / 2.0);
        Vector2D drift = new(WorldGen.genRand.Next(-10, 11) * 0.1, WorldGen.genRand.Next(-20, -10) * 0.1);

        while (currentRadius > 0.0 && remainingHeight > 0.0)
        {
            currentRadius -= WorldGen.genRand.Next(4);
            remainingHeight -= 1.0;

            int leftBound = (int)(mountainCenter.X - currentRadius * 0.5);
            int rightBound = (int)(mountainCenter.X + currentRadius * 0.5);
            int topBound = (int)(mountainCenter.Y - currentRadius * 0.5);
            int bottomBound = (int)(mountainCenter.Y + currentRadius * 0.5);

            adjustedRadius = currentRadius * WorldGen.genRand.Next(80, 120) * 0.01;

            double horizontalDistance = Math.Abs(currentX - mountainCenter.X);
            double verticalDistance = Math.Abs(currentY - mountainCenter.Y);

            if (Math.Sqrt(horizontalDistance * horizontalDistance + verticalDistance * verticalDistance) < adjustedRadius * 0.4 &&
                currentX >= leftBound && currentX < rightBound && currentY >= topBound && currentY < bottomBound)
            {
                return true;
            }

            mountainCenter += drift;
            drift.X += WorldGen.genRand.Next(-10, 11) * 0.05;
            drift.Y += WorldGen.genRand.Next(-10, 11) * 0.05;
            drift.X = Math.Clamp(drift.X, -0.5, 0.5);
            drift.Y = Math.Clamp(drift.Y, -1.5, -0.5);
        }

        return false;
    }
    
    public static bool GenerateCanopyShape(int x, int y, int centerX, int centerY, int width, int height, float curveFrequency, int curveAmplitude, int thornHeight, int thornWidth)
    {
        int relativeY = y - centerY;
        int curveOffset = (int)(Math.Sin(relativeY * curveFrequency) * curveAmplitude);
        int waveX = centerX + curveOffset;
        int halfWidth = width / 2;

        // Apply splotching effect
        double splotchFactor = WorldGen.genRand.Next(80, 120) * 0.01;
        int splotchedWidth = (int)(width * splotchFactor);
        int splotchedHalfWidth = splotchedWidth / 2;

        // Check if the point is within the trunk's width (considering splotching)
        if (x >= waveX - splotchedHalfWidth && x <= waveX + splotchedHalfWidth)
        {
            return true;
        }

        // Check for thorns at the positive and negative peaks
        bool isPositivePeak = Math.Abs(relativeY % (2 * Math.PI / curveFrequency)) < 0.5;
        bool isNegativePeak = Math.Abs(relativeY % (2 * Math.PI / curveFrequency) - Math.PI / curveFrequency) < 0.5;
        if (isPositivePeak || isNegativePeak)
        {
            int peakY = y;
            int topTriangleY = isPositivePeak ? peakY - thornHeight : peakY + thornHeight;
            int leftTriangleX = waveX - thornWidth / 2;
            int rightTriangleX = waveX + thornWidth / 2;

            // Apply splotching effect to thorn width
            int splotchedThornWidth = (int)(thornWidth * splotchFactor);
            int splotchedLeftTriangleX = waveX - splotchedThornWidth / 2;
            int splotchedRightTriangleX = waveX + splotchedThornWidth / 2;

            if (x >= splotchedLeftTriangleX && x <= splotchedRightTriangleX)
            {
                double slope = (double)thornHeight / (splotchedThornWidth / 2);
                double triangleY = isPositivePeak ? topTriangleY + Math.Abs(x - waveX) * slope : topTriangleY - Math.Abs(x - waveX) * slope;
                if ((isPositivePeak && y >= triangleY && y <= peakY) || (isNegativePeak && y <= triangleY && y >= peakY))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool GenerateRectangle(int x, int y, int centerX, int centerY, int width, int height)
    {
        int left = centerX - width / 2;
        int right = centerX + width / 2;
        int top = centerY - height / 2;
        int bottom = centerY + height / 2;

        return x >= left && x <= right && y >= top && y <= bottom;
    }

    public static bool GenerateCircle(int x, int y, int centerX, int centerY, int radius)
    {
        int dx = x - centerX;
        int dy = y - centerY;
        return dx * dx + dy * dy <= radius * radius;
    }

    public static bool GenerateTrapezoid(int x, int y, int centerX, int centerY, int horizontalRadius, int verticalRadius)
    {
        int topY = centerY - verticalRadius;
        int bottomY = centerY + verticalRadius;
        float topLeftX = centerX - horizontalRadius / 2f;
        float topRightX = centerX + horizontalRadius / 2f;
        float bottomLeftX = centerX - horizontalRadius * 1.5f / 2f;
        float bottomRightX = centerX + horizontalRadius * 1.5f / 2f;

        if (y < topY || y > bottomY)
        {
            return false;
        }

        float t = (float)(y - topY) / (bottomY - topY);
        float leftX = (1 - t) * topLeftX + t * bottomLeftX;
        float rightX = (1 - t) * topRightX + t * bottomRightX;

        return x >= leftX && x <= rightX;
    }

    public static bool GenerateTriangle(int x, int y, int centerX, int centerY, int baseWidth, int height)
    {
        int leftX = centerX - baseWidth / 2;
        int rightX = centerX + baseWidth / 2;
        int topY = centerY - height / 2;
        int bottomY = centerY + height / 2;

        if (y < topY || y > bottomY)
        {
            return false;
        }

        float slope = (float)baseWidth / height;
        float leftBoundary = leftX + (y - topY) * slope / 2;
        float rightBoundary = rightX - (y - topY) * slope / 2;

        return x >= leftBoundary && x <= rightBoundary;
    }

    public static bool GenerateStar(int x, int y, int centerX, int centerY, int outerRadius, int innerRadius, int points)
    {
        for (int i = 0; i < points * 2; i++)
        {
            double angle = i * Math.PI / points;
            int radius = i % 2 == 0 ? outerRadius : innerRadius;
            int pointX = centerX + (int)(radius * Math.Cos(angle));
            int pointY = centerY + (int)(radius * Math.Sin(angle));

            if (i == 0)
            {
                if (IsPointInTriangle(x, y, centerX, centerY, pointX, pointY,
                    centerX + (int)(innerRadius * Math.Cos(angle + Math.PI / points)),
                    centerY + (int)(innerRadius * Math.Sin(angle + Math.PI / points))))
                {
                    return true;
                }
            }
            else
            {
                int prevRadius = (i - 1) % 2 == 0 ? outerRadius : innerRadius;
                int prevPointX = centerX + (int)(prevRadius * Math.Cos((i - 1) * Math.PI / points));
                int prevPointY = centerY + (int)(prevRadius * Math.Sin((i - 1) * Math.PI / points));

                if (IsPointInTriangle(x, y, centerX, centerY, pointX, pointY, prevPointX, prevPointY))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsPointInTriangle(int px, int py, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        float area = 0.5f * (-y2 * x3 + y1 * (-x2 + x3) + x1 * (y2 - y3) + x2 * y3);
        float s = 1 / (2 * area) * (y1 * x3 - x1 * y3 + (y3 - y1) * px + (x1 - x3) * py);
        float t = 1 / (2 * area) * (x1 * y2 - y1 * x2 + (y1 - y2) * px + (x2 - x1) * py);

        return s >= 0 && t >= 0 && (s + t) <= 1;
    }

    public static void GenerateCellNoise(int cX, int cY, int hR, int vR, int density, int iterations, bool killTile, int type, bool forced)
    {
        var caveMap = new bool[hR * 2, vR * 2];
        for (var x = 0; x < hR * 2; x++)
        {
            for (var y = 0; y < vR * 2; y++)
            {
                caveMap[x, y] = Main.rand.Next(100) < density;

            }
        }
        for (var iteration = 0; iteration < iterations; iteration++)
        {
            caveMap = PerformStep(caveMap, hR * 2, vR * 2);
        }
        for (var x = 0; x < hR * 2; x++)
        {
            for (var y = 0; y < vR * 2; y++)
            {
                if (caveMap[x, y])
                {
                    var worldX = cX - hR + x;
                    var worldY = cY - vR + y;
                    if (killTile)
                    {
                        WorldGen.KillTile(worldX, worldY);
                    }
                    else
                    {
                        WorldGen.PlaceTile(worldX, worldX, type, forced: forced);
                    }
                }
            }
        }

    }

    public static void GenerateCellNoise_Walls(int cX, int cY, int hR, int vR, int density, int iterations)
    {
        var caveMap = new bool[hR * 2, vR * 2];
        for (var x = 0; x < hR * 2; x++)
        {
            for (var y = 0; y < vR * 2; y++)
            {

                caveMap[x, y] = Main.rand.Next(100) < density;
            }
        }
        for (var iteration = 0; iteration < iterations; iteration++)
        {
            caveMap = PerformStep(caveMap, hR * 2, vR * 2);
        }
        for (var x = 0; x < hR * 2; x++)
        {
            for (var y = 0; y < vR * 2; y++)
            {
                if (caveMap[x, y])
                {
                    var worldX = cX - hR + x;
                    var worldY = cY - vR + y;
                    var tile = Main.tile[worldX, worldY];
                    tile.WallType = 0;
                }
            }
        }
    }
    
    private static int CountSolidNeighbors(bool[,] map, int x, int y, int width, int height)
    {
        int count = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                int neighborX = x + i;
                int neighborY = y + j;

                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    if (map[neighborX, neighborY])
                    {
                        count++;
                    }
                }
                else
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static bool[,] PerformStep(bool[,] map, int width, int height)
    {
        var newMap = new bool[width, height];
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                int solidNeighbors = CountSolidNeighbors(map, x, y, width, height);

                if (solidNeighbors > 4)
                    newMap[x, y] = true;
                else if (solidNeighbors < 4)
                    newMap[x, y] = false;
                else
                    newMap[x, y] = map[x, y];
            }
        }

        return newMap;
    }

    #endregion
}