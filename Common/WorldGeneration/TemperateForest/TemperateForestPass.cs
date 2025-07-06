using Reverie.Content.Tiles.TemperateForest;
using Reverie.lib;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.TemperateForest;

public class TemperateForestConfiguration
{
    public float HillFrequency { get; set; } = 0.005f;
    public int BaseHeightOffset { get; set; } = -8;
    public int HillHeightVariation { get; set; } = 26;
    public int SurfaceDepth { get; set; } = (int)(Main.maxTilesY * 0.03f);
}

public class TemperateForestPass : GenPass
{
    #region Fields
    private readonly TemperateForestConfiguration _config;
    private FastNoiseLite _hillNoise;
    private FastNoiseLite _decorationNoise;
    private int _forestLeft, _forestRight;
    #endregion

    public TemperateForestPass() : base("[Reverie] Temperate Forest", 80f)
    {
        _config = new TemperateForestConfiguration();
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Growing Forests";

        _hillNoise = new FastNoiseLite();
        _hillNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        _hillNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _hillNoise.SetFractalOctaves(3);
        _hillNoise.SetFrequency(_config.HillFrequency);

        _decorationNoise = new FastNoiseLite(WorldGen.genRand.Next());
        _decorationNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        _decorationNoise.SetFrequency(0.05f);

        CalculateForestBounds();
        GenerateTerrain(progress);

        DoGrassAndFoliage(progress);
    }

    private void CalculateForestBounds()
    {
        int forestWidth = (int)(Main.maxTilesX * 0.07f);

        int attempts = 0;
        int maxAttempts = 50;

        while (attempts < maxAttempts)
        {
            int startX = WorldGen.genRand.Next(200, Main.maxTilesX - forestWidth - 200);
            int endX = startX + forestWidth;

            int leftHeight = GetTerrainHeightAt(startX - 1);
            int rightHeight = GetTerrainHeightAt(endX + 1);

            if (Math.Abs(leftHeight - rightHeight) < 50)
            {
                _forestLeft = startX;
                _forestRight = endX;
                return;
            }

            attempts++;
        }

        _forestLeft = Main.maxTilesX / 4;
        _forestRight = _forestLeft + forestWidth;
    }

    private void GenerateTerrain(GenerationProgress progress)
    {
        progress.Message = "Preparing Temperate Forest terrain...";

        int leftEdgeHeight = GetTerrainHeightAt(_forestLeft - 1);
        int rightEdgeHeight = GetTerrainHeightAt(_forestRight + 1);

        progress.Message = "Clearing terrain...";
        ClearArea();

        progress.Message = "Forest Erosion...";
        int[] terrainHeights = GenerateTerrainHeights(leftEdgeHeight, rightEdgeHeight);

        progress.Message = "Building Forest columns...";
        int totalColumns = _forestRight - _forestLeft;
        for (int i = 0; i < totalColumns; i++)
        {
            int x = _forestLeft + i;
            progress.Set((double)i / (totalColumns * 2));

            FillTerrainColumn(x, terrainHeights[i]);
        }
    }

    private void DoGrassAndFoliage(GenerationProgress progress)
    {
        int forestWidth = _forestRight - _forestLeft;
        int processedTiles = 0;

        progress.Message = "Spreading temperate grass...";

        for (int x = _forestLeft; x <= _forestRight; x++)
        {
            for (int y = 50; y < Main.maxTilesY - 100; y++)
            {
                processedTiles++;
                if (processedTiles % 500 == 0)
                {
                    progress.Set(0.5 + (double)processedTiles / (forestWidth * 600 * 3));
                }

                if (!WorldGen.InWorld(x, y)) continue;
                SpreadGrass(x, y);
            }
        }

        progress.Message = "Cleaning up tiles...";
        processedTiles = 0;

        for (int x = _forestLeft; x <= _forestRight; x++)
        {
            for (int y = 50; y < Main.maxTilesY - 100; y++)
            {
                processedTiles++;
                if (processedTiles % 500 == 0)
                {
                    progress.Set(0.66 + (double)processedTiles / (forestWidth * 600 * 3));
                }

                if (!WorldGen.InWorld(x, y)) continue;
                CleanupTiles(x, y);
            }
        }

        progress.Message = "Adding forest foliage...";
        processedTiles = 0;

        for (int x = _forestLeft; x <= _forestRight; x++)
        {
            for (int y = 50; y < Main.maxTilesY - 100; y++)
            {
                processedTiles++;
                if (processedTiles % 500 == 0)
                {
                    progress.Set(0.83 + (double)processedTiles / (forestWidth * 600 * 3));
                }

                if (!WorldGen.InWorld(x, y)) continue;
                AddDecorations(x, y);
            }
        }
    }

    #region Grass and Foliage Logic
    private void SpreadGrass(int x, int y)
    {
        Tile tile = Framing.GetTileSafely(x, y);

        if (!tile.HasTile) return;

        if (tile.TileType == TileID.Dirt)
        {
            if (IsExposedToAir(x, y))
            {
                TryDoGrass(x, y, (ushort)ModContent.TileType<TemperateGrassTile>());
            }
        }
    }

    private bool IsExposedToAir(int x, int y)
    {
        for (int checkX = x - 1; checkX <= x + 1; checkX++)
        {
            for (int checkY = y - 1; checkY <= y + 1; checkY++)
            {
                if (checkX == x && checkY == y) continue;
                if (!WorldGen.InWorld(checkX, checkY)) continue;

                Tile neighborTile = Framing.GetTileSafely(checkX, checkY);

                if (!neighborTile.HasTile)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void TryDoGrass(int tileX, int tileY, ushort grassType)
    {
        Tile tile = Framing.GetTileSafely(tileX, tileY);

        if (!tile.HasTile) return;

        tile.TileType = grassType;

        WorldGen.SquareTileFrame(tileX, tileY, true);

        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendTileSquare(-1, tileX, tileY, 1, TileChangeType.None);
        }
    }

    private void CleanupTiles(int x, int y)
    {
        Tile tile = Framing.GetTileSafely(x, y);

        if (!tile.HasTile) return;

        bool isTemperateTile = tile.TileType == (ushort)ModContent.TileType<TemperateGrassTile>() ||
                              tile.TileType == TileID.Dirt;

        if (!isTemperateTile) return;

        int solidNeighbors = 0;

        for (int checkX = x - 1; checkX <= x + 1; checkX++)
        {
            for (int checkY = y - 1; checkY <= y + 1; checkY++)
            {
                if (checkX == x && checkY == y) continue;

                if (!WorldGen.InWorld(checkX, checkY)) continue;

                Tile neighborTile = Framing.GetTileSafely(checkX, checkY);
                if (neighborTile.HasTile && Main.tileSolid[neighborTile.TileType])
                {
                    solidNeighbors++;
                }
            }
        }

        if (solidNeighbors < 3)
        {
            tile.HasTile = false;
            tile.WallType = 0;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
            }
        }
    }

    private void AddDecorations(int x, int y)
    {
        Tile tile = Framing.GetTileSafely(x, y);
        Tile tileAbove = Framing.GetTileSafely(x, y - 1);
        Tile tileBelow = Framing.GetTileSafely(x, y + 1);

        if (!tile.HasTile) return;

        bool validTile = tile.TileType == (ushort)ModContent.TileType<TemperateGrassTile>();

        if (!validTile) return;

        float decorationValue = _decorationNoise.GetNoise(x, y);
        //if (validTile && !tileAbove.HasTile && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
        //{
        //    if (WorldGen.genRand.NextBool(4))
        //    {
        //        if (ModContent.TileType<TemperateFoliageTile>() > 0)
        //        {
        //            WorldGen.PlaceTile(x, y - 1, (ushort)ModContent.TileType<TemperateFoliageTile>());
        //            tileAbove = Framing.GetTileSafely(x, y - 1);
        //            tileAbove.TileFrameY = 0;
        //            tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(10) * 18);
        //            WorldGen.SquareTileFrame(x, y - 1, true);

        //            if (Main.netMode == NetmodeID.Server)
        //            {
        //                NetMessage.SendTileSquare(-1, x, y - 1, 1, TileChangeType.None);
        //            }
        //        }
        //    }
        //}
    }
    #endregion

    private void PlaceTerrain(int x, int y, int surfaceY)
    {
        Tile tile = Main.tile[x, y];
        tile.HasTile = true;

        int depthFromSurface = y - surfaceY;

        if (depthFromSurface <= _config.SurfaceDepth * 0.2f)
        {
            tile.TileType = TileID.ClayBlock;
        }
        else if (depthFromSurface <= _config.SurfaceDepth * 0.38f)
        {
            tile.TileType = TileID.ClayBlock;
            tile.TileColor = PaintID.BrownPaint;
        }
        else if (depthFromSurface <= _config.SurfaceDepth * 0.52f)
        {
            tile.TileType = TileID.ClayBlock;
        }
        else
        {
            tile.TileType = TileID.Dirt;
        }
    }

    #region Helper Methods (Adapted from CanopyBase)
    private int GetTerrainHeightAt(int x)
    {
        if (!WorldGen.InWorld(x, 0)) return (int)Main.worldSurface - 200;

        for (int y = (int)Main.worldSurface - 250; y < Main.maxTilesY - 300; y++)
        {
            if (WorldGen.InWorld(x, y) && Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
            {
                return y;
            }
        }

        return (int)Main.worldSurface;
    }

    private void ClearArea()
    {
        int clearStartY = Math.Max(0, (int)Main.worldSurface - 300); // Start from sky
        int clearEndY = (int)Main.worldSurface + _config.SurfaceDepth; // Clear down to surface + depth

        for (int x = _forestLeft; x < _forestRight; x++)
        {
            for (int y = clearStartY; y < Math.Min(Main.maxTilesY, clearEndY); y++)
            {
                if (WorldGen.InWorld(x, y))
                {
                    Tile tile = Main.tile[x, y];
                    tile.ClearEverything();
                    tile.LiquidAmount = 0;
                }
            }
        }
    }

    private int[] GenerateTerrainHeights(int leftEdgeHeight, int rightEdgeHeight)
    {
        int width = _forestRight - _forestLeft;
        int[] heights = new int[width];

        int baseHeight = (leftEdgeHeight + rightEdgeHeight) / 2;

        int targetSurfaceHeight = (int)Main.worldSurface + _config.BaseHeightOffset;
        baseHeight = Math.Min(baseHeight, targetSurfaceHeight);
        baseHeight = Math.Max(baseHeight, (int)Main.worldSurface - 180);

        for (int i = 0; i < width; i++)
        {
            int x = _forestLeft + i;
            float normalizedPosition = (float)i / (width - 1);

            float primaryNoise = _hillNoise.GetNoise(x * 0.5f, 0f);
            float secondaryNoise = _hillNoise.GetNoise(x * 1.2f, 100f) * 0.3f;
            float combinedNoise = primaryNoise + secondaryNoise;

            int noiseHeight = (int)(combinedNoise * _config.HillHeightVariation * 1.5f);

            float taperFactor = GetTaperFactor(normalizedPosition, width);
            int taperedNoiseHeight = (int)(noiseHeight * taperFactor);

            int contourHeight = GetContourHeight(normalizedPosition, baseHeight, leftEdgeHeight, rightEdgeHeight, taperFactor);

            heights[i] = contourHeight + taperedNoiseHeight;

            heights[i] = Math.Clamp(heights[i],
                Math.Min(leftEdgeHeight, rightEdgeHeight) - 100,
                Math.Max(leftEdgeHeight, rightEdgeHeight) + 60);
        }

        return heights;
    }

    private int GetContourHeight(float normalizedPosition, int baseHeight, int lHeight, int rHeight, float taperFactor)
    {
        if (taperFactor >= 0.8f)
        {
            return baseHeight;
        }

        float edgeInfluence = (1f - taperFactor) * 0.9f;

        if (normalizedPosition < 0.5f)
        {
            float leftInfluence = (0.5f - normalizedPosition) * 2f * edgeInfluence;
            return (int)(baseHeight * (1f - leftInfluence) + lHeight * leftInfluence);
        }
        else
        {
            float rightInfluence = (normalizedPosition - 0.5f) * 2f * edgeInfluence;
            return (int)(baseHeight * (1f - rightInfluence) + rHeight * rightInfluence);
        }
    }

    private float GetTaperFactor(float normalizedPosition, int width)
    {
        float taperZone = Math.Min(100f, width * 0.2f) / width;

        float distanceFromEdge;
        if (normalizedPosition < taperZone)
        {
            distanceFromEdge = normalizedPosition / taperZone;
        }
        else if (normalizedPosition > 1f - taperZone)
        {
            distanceFromEdge = (1f - normalizedPosition) / taperZone;
        }
        else
        {
            return 1f;
        }

        return (float)(0.5 * (1 + Math.Cos(Math.PI * (1 - distanceFromEdge))));
    }

    private void FillTerrainColumn(int x, int surfaceY)
    {
        int maxDepth = surfaceY + _config.SurfaceDepth;

        for (int y = surfaceY; y < maxDepth && y < Main.maxTilesY; y++)
        {
            if (!IsValidY(y)) continue;

            PlaceTerrain(x, y, surfaceY);
        }
    }
    #endregion

    #region Validation
    private bool IsValidX(int x)
    {
        return x >= 0 && x < Main.maxTilesX;
    }

    private bool IsValidY(int y)
    {
        return y >= 0 && y < Main.maxTilesY;
    }
    #endregion
}
