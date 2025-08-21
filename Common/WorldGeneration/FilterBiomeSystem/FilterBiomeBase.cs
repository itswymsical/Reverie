using Reverie.lib;
using System.Collections.Generic;
using System.Linq;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.FilterBiomeSystem;

public abstract class FilterBiomeBase : GenPass
{
    protected readonly BiomeConfiguration _config;
    protected FastNoiseLite _baseNoise;
    protected FastNoiseLite _terrainNoise;
    protected BiomeBounds _biomeBounds;
    protected int[] _terrainHeights;

    protected const ushort PRESERVE_AIR = ushort.MaxValue;

    public FilterBiomeBase(string name, float weight, BiomeConfiguration config = null) : base(name, weight)
    {
        _config = config ?? new BiomeConfiguration();
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = $"Locating {PassName} biome...";

        InitializeNoise();
        
        if (!GetBiomeBounds(out _biomeBounds))
        {
            progress.Message = $"Failed to find suitable location for {PassName} - skipping";
            return;
        }

        progress.Message = $"Generating {PassName} base patch...";
        GenerateBasePatch(progress);

        progress.Message = $"Shaping {PassName} terrain...";
        GenerateTerrain(progress);

        progress.Message = $"Populating {PassName}...";
        PopulateBiome(progress);
    }

    #region Abstract Methods
    protected abstract string PassName { get; }
    protected abstract bool GetBiomeBounds(out BiomeBounds bounds);
    protected abstract ushort GetBaseTile(int x, int y, int depthFromSurface, float noiseValue);
    protected abstract ushort GetTerrainTile(int x, int y, int depthFromSurface);
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
        if (tileType == TileID.Dirt || tileType == TileID.Grass ||
            tileType == TileID.ClayBlock || tileType == TileID.Sand ||
            tileType == TileID.Silt || tileType == TileID.Slush)
            return true;

        if (tileType == TileID.CorruptGrass || tileType == TileID.CrimsonGrass)
            return true;

        return false;
    }
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

                if (ShouldSkipTileGeneration(x, y)) continue;

                Tile tile = Main.tile[x, y];
                if (!tile.HasTile || !CanReplaceTile(tile.TileType)) continue;

                float noiseValue = _baseNoise.GetNoise(x, y);
                int depthFromSurface = y - surfaceY;

                float taperFactor = GetHorizontalTaper(x);
                if (WorldGen.genRand.NextFloat() > taperFactor) continue;

                ushort newTileType = GetBaseTile(x, y, depthFromSurface, noiseValue);
                if (newTileType != 0 && newTileType != PRESERVE_AIR)
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

        _terrainHeights = GenerateContouredHeights(leftEdgeHeight, rightEdgeHeight);

        for (int i = 0; i < _biomeBounds.Width; i++)
        {
            int x = _biomeBounds.Left + i;
            progress.Set(0.5 + (double)i / _biomeBounds.Width * 0.5);

            FillTerrainColumn(x, _terrainHeights[i]);
        }
    }

    protected virtual int[] GenerateContouredHeights(int leftEdgeHeight, int rightEdgeHeight)
    {
        int width = _biomeBounds.Width;
        int[] heights = new int[width];
        int[] existingHeights = new int[width];

        // First, sample existing terrain heights
        for (int i = 0; i < width; i++)
        {
            int x = _biomeBounds.Left + i;
            existingHeights[i] = GetActualSurfaceHeight(x);
        }

        // Generate our desired contour
        int baseHeight = (leftEdgeHeight + rightEdgeHeight) / 2;
        int targetSurfaceHeight = (int)Main.worldSurface + _config.BaseHeightOffset;
        baseHeight = Math.Min(baseHeight, targetSurfaceHeight);
        baseHeight = Math.Max(baseHeight, (int)Main.worldSurface - 180);

        for (int i = 0; i < width; i++)
        {
            int x = _biomeBounds.Left + i;
            float normalizedPosition = (float)i / (width - 1);

            // Generate noise-based height variation
            float primaryNoise = _terrainNoise.GetNoise(x * 0.5f, 0f);
            float secondaryNoise = _terrainNoise.GetNoise(x * 1.2f, 100f) * 0.3f;
            float combinedNoise = primaryNoise + secondaryNoise;

            int noiseHeight = (int)(combinedNoise * _config.TerrainHeightVariation * 0.5f); // Reduced intensity

            float taperFactor = GetTaperFactor(normalizedPosition, width);
            int taperedNoiseHeight = (int)(noiseHeight * taperFactor);

            // Blend between existing terrain and our desired height
            int desiredHeight = baseHeight + taperedNoiseHeight;
            int existingHeight = existingHeights[i];

            // Use a weighted blend - respect existing terrain more at edges
            float blendFactor = taperFactor * 0.6f; // Max 60% influence in center
            heights[i] = (int)(existingHeight * (1f - blendFactor) + desiredHeight * blendFactor);

            // Ensure reasonable bounds
            heights[i] = Math.Clamp(heights[i],
                Math.Min(leftEdgeHeight, rightEdgeHeight) - 50,
                Math.Max(leftEdgeHeight, rightEdgeHeight) + 30);
        }

        return heights;
    }

    private void FillTerrainColumn(int x, int surfaceY)
    {
        int maxDepth = surfaceY + (int)(_config.SurfaceDepth / 1.5f); // Reduced depth for less intrusion

        for (int y = surfaceY; y < maxDepth && y < Main.maxTilesY; y++)
        {
            if (ShouldSkipTileGeneration(x, y)) continue;

            Tile tile = Main.tile[x, y];

            // Only fill air or easily replaceable terrain
            if (tile.HasTile && !CanReplaceTile(tile.TileType))
                continue;

            // Only place tiles below the surface or in specific conditions
            int depthFromSurface = y - surfaceY;
            if (depthFromSurface < 0 && tile.HasTile) continue; // Don't place above existing surface

            ushort tileType = GetTerrainTile(x, y, depthFromSurface);

            if (tileType != 0 && tileType != PRESERVE_AIR)
            {
                tile.HasTile = true;
                tile.TileType = tileType;
            }
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

    private int GetActualSurfaceHeight(int x)
    {
        if (!WorldGen.InWorld(x, 0)) return (int)Main.worldSurface;

        // Start from a reasonable surface area
        for (int y = (int)Main.worldSurface - 100; y < (int)Main.worldSurface + 100; y++)
        {
            if (!WorldGen.InWorld(x, y)) continue;

            var tile = Main.tile[x, y];

            // Skip evil biome air pockets
            if (!tile.HasTile && IsEvilWall(tile.WallType))
                continue;

            // Found solid ground
            if (tile.HasTile && Main.tileSolid[tile.TileType])
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

    protected bool IsNearBiome(int startX, int width, params int[] biomeTypes)
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

    protected Rectangle GetSpawnBounds(int minDistance, int maxDistance, int width)
    {
        int spawnX = Main.spawnTileX;
        bool leftSideClear = !IsNearBiome(spawnX - maxDistance, width);
        bool rightSideClear = !IsNearBiome(spawnX + minDistance, width);

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

    private bool IsEvilWall(ushort wallType)
    {
        // Corruption walls
        if (wallType >= WallID.CorruptionUnsafe1 && wallType <= WallID.CorruptionUnsafe4)
            return true;

        // Crimson walls  
        if (wallType >= WallID.CrimsonUnsafe1 && wallType <= WallID.CrimsonUnsafe4)
            return true;

        // Additional evil walls that might be missed
        if (wallType == WallID.EbonstoneUnsafe || wallType == WallID.CorruptHardenedSand ||
            wallType == WallID.CrimstoneUnsafe || wallType == WallID.CrimsonHardenedSand ||
            wallType == WallID.CorruptGrassUnsafe || wallType == WallID.CrimsonGrassUnsafe)
            return true;

        return false;
    }

    private bool IsEvilTile(ushort tileType)
    {
        if (tileType == TileID.Ebonstone ||
            tileType == TileID.CorruptSandstone || tileType == TileID.CorruptHardenedSand)
            return true;

        if (tileType == TileID.Crimstone ||
            tileType == TileID.CrimsonSandstone || tileType == TileID.CrimsonHardenedSand)
            return true;

        return false;
    }

    private bool ShouldSkipTileGeneration(int x, int y)
    {
        if (!WorldGen.InWorld(x, y)) return true;

        var tile = Main.tile[x, y];

        if (!tile.HasTile && IsEvilWall(tile.WallType))
            return true;

        if (tile.HasTile && IsEvilTile(tile.TileType))
            return true;

        if (tile.HasTile && IsImportantStructure(tile.TileType))
            return true;

        int evilCount = 0;
        int totalChecked = 0;

        for (int checkX = x - 3; checkX <= x + 3; checkX++)
        {
            for (int checkY = y - 3; checkY <= y + 3; checkY++)
            {
                if (!WorldGen.InWorld(checkX, checkY)) continue;

                var checkTile = Main.tile[checkX, checkY];
                totalChecked++;

                if (!checkTile.HasTile && IsEvilWall(checkTile.WallType))
                    evilCount++;

                if (checkTile.HasTile && IsEvilTile(checkTile.TileType))
                    evilCount++;
            }
        }

        return totalChecked > 0 && (float)evilCount / totalChecked > 0.333f;
    }

    private bool IsImportantStructure(ushort tileType)
    {
        return tileType == TileID.LivingWood || tileType == TileID.LeafBlock ||
               tileType == TileID.WoodBlock || tileType == TileID.Platforms ||
               tileType == TileID.Trees || tileType == TileID.BeeHive;
    }

    #endregion
}