using Reverie.Content.Tiles.Sylvanwalde;
using Reverie.lib;

using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.Sylvanwalde.Generation;

public class SylvanConfiguration
{
    public float NoiseFrequency { get; set; } = 0.002f;
    public int BaseHeightOffset { get; set; } = 10;
    public int HillVariation { get; set; } = 53;
    public int LoamDepth { get; set; } = (int)(Main.maxTilesY / 1.6f);
}

public class SylvanTerrainPass : GenPass
{
    #region Constants
    private const float SURFACE_LAYER_RATIO = 1f / 5f;
    #endregion

    #region Fields
    private readonly SylvanConfiguration _config;
    private FastNoiseLite groundNoise;
    private int _surfaceHeight;
    #endregion

    #region Initialization
    public SylvanTerrainPass() : base("[Sylvan] Terrain", 247.43f)
    {
        _config = new SylvanConfiguration();
    }

    private void Initialize()
    {
        InitializeNoise();
        CalculateWorldParameters();
    }

    private void InitializeNoise()
    {
        groundNoise = new FastNoiseLite();
        groundNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        groundNoise.SetFractalType(FastNoiseLite.FractalType.PingPong);
        groundNoise.SetFractalOctaves(6);
        groundNoise.SetFrequency(_config.NoiseFrequency);
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
        progress.Message = "Generating Terrain";
        Initialize();

        progress.Set(0.0f);
        GenerateTerrain();
        progress.Set(0.8f);

        progress.Message = "Adding Details";
        GeneratePatches();
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

        var peakHeight = CalculatePeakHeight(x);
        FillColumn(x, peakHeight);
    }
    #endregion

    #region Height Calculations
    private int CalculatePeakHeight(int x)
    {
        var noiseValue = groundNoise.GetNoise(x / 2, Main.maxTilesY / 2);
        var height = (int)(_surfaceHeight - _config.BaseHeightOffset + noiseValue * _config.HillVariation);
        return Math.Clamp(height, 0, Main.maxTilesY - 1);
    }
    #endregion

    #region Tile Placement
    private void FillColumn(int x, int peakHeight)
    {
        for (var y = peakHeight; y < Main.UnderworldLayer; y++)
        {
            if (!IsValidY(y)) continue;
            var tile = Main.tile[x, y];

            tile.HasTile = true;
            tile.TileType = (ushort)ModContent.TileType<LoamTile>();
        }
    }

    private void GeneratePatches()
    {
        // Get the transition depth area boundaries
        var transitionStart = Main.UnderworldLayer - _config.LoamDepth;
        var transitionDepth = 40;

        // Number of patches scales with world size
        var patchCount = (int)(Main.maxTilesX * 0.2f);

        for (var i = 0; i < patchCount; i++)
        {
            // Randomize position within the lower half of the transition zone
            var x = _random.Next(10, Main.maxTilesX - 10);
            var y = _random.Next(
                transitionStart + transitionDepth / 2,
                transitionStart + transitionDepth
            );

            // Create small pockets that extend upward
            if (Main.tile[x, y].TileType == (ushort)ModContent.TileType<LoamTile>())
            {
                WorldGen.TileRunner(
                    x, y,
                    _random.Next(5, 15),
                    _random.Next(10, 30),
                    (ushort)ModContent.TileType<CobblestoneTile>(),
                    true
                );
            }

            y = _random.Next(
                transitionStart,
                transitionStart + transitionDepth / 2
            );

            if (Main.tile[x, y].TileType == (ushort)ModContent.TileType<CobblestoneTile>())
            {
                WorldGen.TileRunner(
                    x, y,
                    _random.Next(5, 15),
                    _random.Next(10, 30),
                    (ushort)ModContent.TileType<LoamTile>(),
                    true
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