using Reverie.Content.Tiles.Canopy.Surface;
using Reverie.lib;
using Terraria.GameContent.Biomes;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.WoodlandCanopy;

public class CanopyConfiguration
{
    public float HillFrequency { get; set; } = 0.0022f;
    public int BaseHeightOffset { get; set; } = 6;
    public int HillHeightVariation { get; set; } = 15;
    public int CanopyDepth { get; set; } = (int)(Main.maxTilesY / 1.7647f);
}

public class CanopySurfacePass : GenPass
{
    #region Fields
    private readonly CanopyConfiguration _config;
    private FastNoiseLite _hillNoise;
    private int _canopyLeft, _canopyRight;
    #endregion

    public CanopySurfacePass() : base("Canopy Overworld", 80f)
    {
        _config = new CanopyConfiguration();
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Growing Canopy Biome...";

        Initialize();
        CalculateCanopyBounds();
        GenerateCanopyTerrain(progress);
    }

    private void Initialize()
    {
        _hillNoise = new FastNoiseLite();
        _hillNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        _hillNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _hillNoise.SetFractalOctaves(3);
        _hillNoise.SetFrequency(_config.HillFrequency);
    }

    private void CalculateCanopyBounds()
    {
        // Position adjacent to jungle using jungle boundaries
        int jungleLeft = GenVars.jungleMinX;
        int jungleRight = GenVars.jungleMaxX;
        int jungleCenter = (jungleLeft + jungleRight) / 2;
        int worldCenter = Main.maxTilesX / 2;

        // Determine which side of jungle to place canopy
        // If jungle is on left side of world, place canopy on left side of jungle
        // If jungle is on right side of world, place canopy on right side of jungle
        bool placeOnLeftSide = jungleCenter < worldCenter;

        int canopyWidth = WorldGen.genRand.Next((int)(Main.maxTilesX / 21.33f), (int)(Main.maxTilesX / 11.43f));
        int jungleOffset = 30;

        if (placeOnLeftSide)
        {
            _canopyRight = jungleLeft - jungleOffset;
            _canopyLeft = _canopyRight - canopyWidth;
        }
        else
        {
            // Place canopy to the right of jungle
            _canopyLeft = jungleRight + jungleOffset;
            _canopyRight = _canopyLeft + canopyWidth;
        }

        // Ensure bounds stay within world limits
        _canopyLeft = Math.Max(_canopyLeft, 200); // Stay away from ocean
        _canopyRight = Math.Min(_canopyRight, Main.maxTilesX - 200); // Stay away from ocean

        // If boundaries are invalid, fallback to original positioning
        if (_canopyLeft >= _canopyRight)
        {
            // Fallback to coastal positioning if jungle adjacency fails
            bool spawnLeftSide = WorldGen.genRand.NextBool();
            int coastalOffset = 220;

            if (spawnLeftSide)
            {
                _canopyLeft = coastalOffset;
                _canopyRight = coastalOffset + canopyWidth;
            }
            else
            {
                _canopyRight = Main.maxTilesX - coastalOffset;
                _canopyLeft = _canopyRight - canopyWidth;
            }
        }
    }

    private void GenerateCanopyTerrain(GenerationProgress progress)
    {
        int totalColumns = _canopyRight - _canopyLeft;

        for (int x = _canopyLeft; x < _canopyRight; x++)
        {
            if (!IsValidX(x)) continue;

            progress.Set((double)(x - _canopyLeft) / totalColumns);
            GenerateCanopyColumn(x);
        }
    }

    private void GenerateCanopyColumn(int x)
    {
        int actualSurface = ScanForSurface(x);
        if (actualSurface == -1) return; // No surface found

        int hillHeight = CalculateHillHeight(x, actualSurface);
        FillCanopyColumn(x, hillHeight);
    }

    private int ScanForSurface(int x)
    {
        // Scan downward from sky to find first solid tile
        for (int y = Main.maxTilesY / 16; y < Main.maxTilesY - 200; y++)
        {
            if (Main.tile[x, y].HasTile)
            {
                return y; // Found the surface
            }
        }
        return -1; // No surface found
    }

    private int CalculateHillHeight(int x, int actualSurface)
    {
        // Calculate edge distance for tapering
        float distanceFromLeft = (float)(x - _canopyLeft);
        float distanceFromRight = (float)(_canopyRight - x);
        float minDistanceFromEdge = Math.Min(distanceFromLeft, distanceFromRight);
        float biomeWidth = _canopyRight - _canopyLeft;

        // Create taper effect - reduce hill variation near edges
        float taperZone = Math.Min(100f, biomeWidth * 0.15f); // 15% of biome width or 100 tiles max
        float edgeFactor = Math.Min(1f, minDistanceFromEdge / taperZone);
        edgeFactor = edgeFactor * edgeFactor; // Square for smoother curve

        // Generate noise and apply edge tapering
        float noiseValue = _hillNoise.GetNoise(x / 2f, actualSurface / 2f);
        float taperedHeightVariation = _config.HillHeightVariation * edgeFactor;

        int height = (int)(actualSurface - _config.BaseHeightOffset + noiseValue * taperedHeightVariation);

        // Clamp relative to actual surface
        return Math.Clamp(height, actualSurface - 60, actualSurface + 30);
    }

    private void FillCanopyColumn(int x, int hillHeight)
    {
        int maxDepth = hillHeight + _config.CanopyDepth;

        for (int y = hillHeight; y < maxDepth && y < Main.maxTilesY; y++)
        {
            if (!IsValidY(y)) continue;

            PlaceCanopyTile(x, y, hillHeight);
        }
    }

    private void PlaceCanopyTile(int x, int y, int surfaceY)
    {
        Tile tile = Main.tile[x, y];
        tile.HasTile = true;

        int depthFromSurface = y - surfaceY;
        if (depthFromSurface < 1)
        {
            tile.TileType = (ushort)ModContent.TileType<CanopyGrassTile>();
        }
        else if (depthFromSurface <= 15)
        {
            tile.TileType = (ushort)ModContent.TileType<OxisolTile>();
        }
        else if (depthFromSurface <= 25)
        {
            tile.TileType = TileID.ClayBlock;
        }
        else if (depthFromSurface <= 50)
        {
            tile.TileType = TileID.Mud;
        }
        else if (depthFromSurface <= 280)
        {
            tile.TileType = (ushort)ModContent.TileType<OxisolTile>();
        }
        else
        {
            tile.TileType = TileID.LivingWood;
        }
    }

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