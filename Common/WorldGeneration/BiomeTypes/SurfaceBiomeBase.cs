using Reverie.lib;
using System.Collections.Generic;
using System.Linq;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.BiomeTypes;

public abstract class SurfaceBiomeBase : GenPass
{
    protected readonly BiomeConfiguration _config;
    protected FastNoiseLite _baseNoise;
    protected FastNoiseLite _terrainNoise;
    protected BiomeBounds _biomeBounds;
    protected int[] _terrainHeights;

    public SurfaceBiomeBase(string name, float weight, BiomeConfiguration config = null) : base(name, weight)
    {
        _config = config ?? new BiomeConfiguration();
    }

    private void SpreadGrass(GenerationProgress progress)
    {
        var soilTiles = new List<Point>();
        var soilType = GetSoilTileType();

        // Look around actual terrain surface heights, not entire biome bounds
        for (int i = 0; i < _biomeBounds.Width; i++)
        {
            int x = _biomeBounds.Left + i;
            int surfaceY = _terrainHeights[i];

            // Check a small range around the surface
            for (int y = surfaceY - 5; y < surfaceY + 20; y++)
            {
                if (!WorldGen.InWorld(x, y)) continue;

                var tile = Main.tile[x, y];
                if (tile.HasTile && tile.TileType == soilType)
                {
                    soilTiles.Add(new Point(x, y));
                }
            }
        }

        // Convert exposed soil tiles to grass
        int processed = 0;
        foreach (var soilPos in soilTiles)
        {
            processed++;
            if (processed % 100 == 0)
                progress.Set((double)processed / soilTiles.Count);

            if (IsExposedToAir(soilPos.X, soilPos.Y))
            {
                var tile = Main.tile[soilPos.X, soilPos.Y];
                tile.TileType = GetGrassTileType();
                WorldGen.SquareTileFrame(soilPos.X, soilPos.Y, true);

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendTileSquare(-1, soilPos.X, soilPos.Y, 1, TileChangeType.None);
                }
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

                var neighborTile = Main.tile[checkX, checkY];
                if (!neighborTile.HasTile)
                    return true;
            }
        }
        return false;
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = $"Locating {PassName} biome...";

        InitializeNoise();

        if (!TryCalculateBiomeBounds(out _biomeBounds))
        {
            progress.Message = $"Failed to find suitable location for {PassName} - skipping";
            return;
        }

        progress.Message = $"Generating {PassName} base patch...";
        GenerateBasePatch(progress);

        progress.Message = $"Shaping {PassName} terrain...";
        GenerateTerrain(progress);

        if (ShouldSpreadGrass())
        {
            progress.Message = $"Spreading {PassName} grass...";
            SpreadGrass(progress);
        }

        progress.Message = $"Populating {PassName}...";
        PopulateBiome(progress);

        PostGeneration(progress);
    }

    #region Abstract Methods
    protected abstract string PassName { get; }
    protected abstract bool TryCalculateBiomeBounds(out BiomeBounds bounds);
    protected abstract ushort GetBaseTileType(int x, int y, int depthFromSurface, float noiseValue);
    protected abstract ushort GetTerrainTileType(int x, int y, int depthFromSurface);
    protected abstract void PopulateBiome(GenerationProgress progress);
    #endregion

    #region Virtual Methods
    protected virtual void InitializeNoise()
    {
        _baseNoise = new FastNoiseLite();
        _baseNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        _baseNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _baseNoise.SetFractalOctaves(3);
        _baseNoise.SetFrequency(_config.BaseNoiseFreq);

        _terrainNoise = new FastNoiseLite();
        _terrainNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        _terrainNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _terrainNoise.SetFractalOctaves(2);
        _terrainNoise.SetFrequency(_config.TerrainNoiseFreq);
    }

    protected virtual bool CanReplaceTile(int tileType)
    {
        return tileType == TileID.Dirt || tileType == TileID.Grass ||
               tileType == TileID.ClayBlock || tileType == TileID.CrimsonGrass ||
               tileType == TileID.CorruptGrass || tileType == TileID.Stone;
    }

    protected virtual void PostGeneration(GenerationProgress progress) { }

    protected virtual ushort GetSoilTileType() => 0; // Override to enable grass spreading
    protected virtual ushort GetGrassTileType() => 0; // Override to enable grass spreading

    protected virtual bool ShouldSpreadGrass() => GetSoilTileType() != 0 && GetGrassTileType() != 0;
    #endregion

    private void GenerateBasePatch(GenerationProgress progress)
    {
        int totalTiles = _biomeBounds.Width * (_config.SurfaceDepth + 50);
        int processedTiles = 0;

        for (int x = _biomeBounds.Left; x < _biomeBounds.Right; x++)
        {
            int surfaceY = GetSurfaceHeight(x);
            int maxDepth = surfaceY + _config.SurfaceDepth;

            for (int y = Math.Max(surfaceY - 10, 0); y < Math.Min(maxDepth, Main.maxTilesY); y++)
            {
                if (!WorldGen.InWorld(x, y)) continue;

                Tile tile = Main.tile[x, y];
                if (!tile.HasTile || !CanReplaceTile(tile.TileType)) continue;

                float noiseValue = _baseNoise.GetNoise(x, y);
                int depthFromSurface = y - surfaceY;

                float taperFactor = GetHorizontalTaper(x);
                if (WorldGen.genRand.NextFloat() > taperFactor) continue;

                ushort newTileType = GetBaseTileType(x, y, depthFromSurface, noiseValue);
                if (newTileType != 0)
                {
                    tile.TileType = newTileType;
                }

                processedTiles++;
            }

            if (processedTiles % 100 == 0)
                progress.Set((double)processedTiles / totalTiles * 0.5);
        }
    }

    private void GenerateTerrain(GenerationProgress progress)
    {
        int leftEdgeHeight = GetSurfaceHeight(_biomeBounds.Left - 1);
        int rightEdgeHeight = GetSurfaceHeight(_biomeBounds.Right + 1);

        _terrainHeights = GenerateTerrainHeights(leftEdgeHeight, rightEdgeHeight);

        for (int i = 0; i < _biomeBounds.Width; i++)
        {
            int x = _biomeBounds.Left + i;
            progress.Set(0.5 + (double)i / _biomeBounds.Width * 0.5);

            FillTerrainColumn(x, _terrainHeights[i]);
        }
    }

    protected int[] GenerateTerrainHeights(int leftEdgeHeight, int rightEdgeHeight)
    {
        int width = _biomeBounds.Width;
        int[] heights = new int[width];
        int baseHeight = (leftEdgeHeight + rightEdgeHeight) / 2;

        int targetSurfaceHeight = (int)Main.worldSurface + _config.BaseHeightOffset;
        baseHeight = Math.Min(baseHeight, targetSurfaceHeight);
        baseHeight = Math.Max(baseHeight, (int)Main.worldSurface - 180);

        for (int i = 0; i < width; i++)
        {
            int x = _biomeBounds.Left + i;
            float normalizedPosition = (float)i / (width - 1);

            float primaryNoise = _terrainNoise.GetNoise(x * 0.5f, 0f);
            float secondaryNoise = _terrainNoise.GetNoise(x * 1.2f, 100f) * 0.3f;
            float combinedNoise = primaryNoise + secondaryNoise;

            int noiseHeight = (int)(combinedNoise * _config.TerrainHeightVariation * 1.5f);

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

    private void FillTerrainColumn(int x, int surfaceY)
    {
        int maxDepth = surfaceY + _config.SurfaceDepth;

        for (int y = surfaceY; y < maxDepth && y < Main.maxTilesY; y++)
        {
            if (!WorldGen.InWorld(x, y)) continue;

            Tile tile = Main.tile[x, y];
            tile.HasTile = true;

            int depthFromSurface = y - surfaceY;
            ushort tileType = GetTerrainTileType(x, y, depthFromSurface);

            if (tileType != 0)
                tile.TileType = tileType;
        }
    }

    #region Helper Methods
    protected int GetSurfaceHeight(int x)
    {
        if (!WorldGen.InWorld(x, 0)) return (int)Main.worldSurface - 200;

        for (int y = (int)Main.worldSurface - 250; y < Main.maxTilesY - 300; y++)
        {
            if (WorldGen.InWorld(x, y) && Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
                return y;
        }

        return (int)Main.worldSurface;
    }

    protected float GetHorizontalTaper(int x)
    {
        float normalizedX = (float)(x - _biomeBounds.Left) / _biomeBounds.Width;
        return GetTaperFactor(normalizedX, _biomeBounds.Width);
    }

    protected float GetTaperFactor(float normalizedPosition, int width)
    {
        float taperZone = Math.Min(_config.EdgeTaperZone, width * 0.2f) / width;

        float distanceFromEdge;
        if (normalizedPosition < taperZone)
            distanceFromEdge = normalizedPosition / taperZone;
        else if (normalizedPosition > 1f - taperZone)
            distanceFromEdge = (1f - normalizedPosition) / taperZone;
        else
            return 1f;

        return (float)(0.5 * (1 + Math.Cos(Math.PI * (1 - distanceFromEdge))));
    }

    protected int GetContourHeight(float normalizedPosition, int baseHeight, int lHeight, int rHeight, float taperFactor)
    {
        if (taperFactor >= 0.8f)
            return baseHeight;

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

    protected bool IsNearExistingBiome(int startX, int width, params int[] biomeTypes)
    {
        int surfaceY = (int)Main.worldSurface;
        int samplePoints = Math.Min(10, width / 10);

        for (int i = 0; i < samplePoints; i++)
        {
            int x = startX + (width * i / samplePoints);
            if (!WorldGen.InWorld(x, surfaceY)) continue;

            Tile tile = Main.tile[x, surfaceY];
            if (biomeTypes.Contains(tile.TileType))
                return true;
        }

        return false;
    }

    protected Rectangle CalculateSpawnProximityBounds(int minDistance, int maxDistance, int width)
    {
        int spawnX = Main.spawnTileX;
        bool leftSideClear = !IsNearExistingBiome(spawnX - maxDistance, width);
        bool rightSideClear = !IsNearExistingBiome(spawnX + minDistance, width);

        if (!leftSideClear && !rightSideClear)
            return Rectangle.Empty;

        int left, right;
        if (leftSideClear && rightSideClear)
        {
            if (WorldGen.genRand.NextBool())
            {
                left = spawnX - minDistance - width;
                right = spawnX - minDistance;
            }
            else
            {
                left = spawnX + minDistance;
                right = spawnX + minDistance + width;
            }
        }
        else if (leftSideClear)
        {
            left = spawnX - minDistance - width;
            right = spawnX - minDistance;
        }
        else
        {
            left = spawnX + minDistance;
            right = spawnX + minDistance + width;
        }

        left = Math.Max(left, 100);
        right = Math.Min(right, Main.maxTilesX - 100);

        return new Rectangle(left, 0, right - left, 0);
    }
    #endregion
}