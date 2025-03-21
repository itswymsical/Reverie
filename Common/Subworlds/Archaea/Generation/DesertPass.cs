using Reverie.Content.Tiles.Archaea;
using Reverie.lib;

using Terraria.IO;
using Terraria.WorldBuilding;


namespace Reverie.Common.Subworlds.Archaea.Generation;

public class DesertConfiguration
{
    public float DuneFrequency { get; set; } = 0.05f;
    public int BaseHeightOffset { get; set; } = 10;
    public int DuneHeightVariation { get; set; } = 35;
    public int HardenedSandDepth { get; set; } = (int)(Main.maxTilesY / 1.6f);
}

public class DesertPass : GenPass
{
    #region Constants
    private const float SURFACE_LAYER_RATIO = 1f / 5f;
    #endregion

    #region Fields
    private readonly DesertConfiguration _config;
    private FastNoiseLite _duneNoise;
    private int _surfaceHeight;
    #endregion

    #region Initialization
    public DesertPass() : base("[Archaea] Desert", 247.43f)
    {
        _config = new DesertConfiguration();
    }

    private void Initialize()
    {
        InitializeNoise();
        CalculateWorldParameters();
    }

    private void InitializeNoise()
    {
        _duneNoise = new FastNoiseLite();
        _duneNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        _duneNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _duneNoise.SetFractalOctaves(3);
        _duneNoise.SetFrequency(_config.DuneFrequency);
    }

    private void CalculateWorldParameters()
    {
        _surfaceHeight = (int)(Main.maxTilesY * SURFACE_LAYER_RATIO);
        Main.worldSurface = _surfaceHeight;
        Main.spawnTileX = Main.maxTilesX / 2;
        Main.spawnTileY = _surfaceHeight;
    }
    #endregion

    #region Core Generation
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating Desert Terrain";
        Initialize();

        progress.Set(0.0f);
        GenerateTerrain();
        progress.Set(0.8f);

        progress.Message = "Adding Details";
        GenerateSandPatches();
        progress.Set(1.0f);
    }

    private void GenerateTerrain()
    {
        for (var x = 0; x < Main.maxTilesX; x++)
        {
            GenerateColumn(x);
        }
    }

    private void GenerateColumn(int x)
    {
        if (!IsValidX(x)) return;

        int peakHeight = CalculatePeakHeight(x);
        FillColumn(x, peakHeight);
    }
    #endregion

    #region Height Calculations
    private int CalculatePeakHeight(int x)
    {
        float noiseValue = _duneNoise.GetNoise(x / 2, Main.maxTilesY / 2);
        int height = (int)(_surfaceHeight - _config.BaseHeightOffset + noiseValue * _config.DuneHeightVariation);
        return Math.Clamp(height, 0, Main.maxTilesY - 1);
    }
    #endregion

    #region Tile Placement
    private void FillColumn(int x, int peakHeight)
    {
        for (var y = peakHeight; y < Main.UnderworldLayer; y++)
        {
            if (!IsValidY(y)) continue;
            PlaceSandTile(x, y);
        }
    }

    private void PlaceSandTile(int x, int y)
    {
        var tile = Main.tile[x, y];
        tile.HasTile = true;

        // Calculate transition zone - create a blend area where both tile types can appear
        int transitionStart = Main.UnderworldLayer - _config.HardenedSandDepth;
        int transitionDepth = 40; // Controls how wide the blending area is

        if (y >= transitionStart + transitionDepth)
        {
            // Deep area - always hardened sand
            tile.TileType = TileID.HardenedSand;
        }
        else if (y < transitionStart)
        {
            // Upper area - always primordial sand
            tile.TileType = (ushort)ModContent.TileType<PrimordialSandTile>();
        }
        else
        {
            // Transition zone - use noise and distance to create dithering effect
            float depthRatio = (float)(y - transitionStart) / transitionDepth;
            float noise = _duneNoise.GetNoise(x * 0.8f, y * 0.8f) * 0.5f + 0.5f;

            if (noise < depthRatio)
            {
                tile.TileType = TileID.HardenedSand;
            }
            else
            {
                tile.TileType = (ushort)ModContent.TileType<PrimordialSandTile>();
            }
        }
    }
    private void GenerateSandPatches()
    {
        // Get the transition depth area boundaries
        int transitionStart = Main.UnderworldLayer - _config.HardenedSandDepth;
        int transitionDepth = 40;

        // Number of patches scales with world size
        int patchCount = (int)(Main.maxTilesX * 0.2f);

        for (int i = 0; i < patchCount; i++)
        {
            // Randomize position within the lower half of the transition zone
            int x = GenBase._random.Next(10, Main.maxTilesX - 10);
            int y = GenBase._random.Next(
                transitionStart + transitionDepth / 2,
                transitionStart + transitionDepth
            );

            // Create small pockets of hardened sand that extend upward
            if (Main.tile[x, y].TileType == (ushort)ModContent.TileType<PrimordialSandTile>())
            {
                WorldGen.TileRunner(
                    x, y,
                    GenBase._random.Next(5, 15),     // Size of the patch
                    GenBase._random.Next(10, 30),    // Steps the runner takes
                    TileID.HardenedSand,             // New tile type
                    true                             // Add tile (not remove)
                );
            }

            // Create small pockets of primordial sand that extend downward
            y = GenBase._random.Next(
                transitionStart,
                transitionStart + transitionDepth / 2
            );

            if (Main.tile[x, y].TileType == TileID.HardenedSand)
            {
                WorldGen.TileRunner(
                    x, y,
                    GenBase._random.Next(5, 15),     // Size of the patch
                    GenBase._random.Next(10, 30),    // Steps the runner takes
                    (ushort)ModContent.TileType<PrimordialSandTile>(), // New tile type
                    true                             // Add tile (not remove)
                );
            }
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