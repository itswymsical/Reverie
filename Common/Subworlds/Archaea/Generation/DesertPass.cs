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
    public int HardenedSandDepth { get; set; } = 12;
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
    public DesertPass(string name, float loadWeight) : base(name, loadWeight)
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
        GenerateTerrain();
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
        if (y >= Main.UnderworldLayer - _config.HardenedSandDepth)
        {
            tile.TileType = TileID.HardenedSand;
        }
        else
        {
            tile.TileType = (ushort)ModContent.TileType<PrimordialSandTile>();
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