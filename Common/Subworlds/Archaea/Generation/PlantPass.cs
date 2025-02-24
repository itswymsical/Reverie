using SubworldLibrary;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.Archaea.Generation;

public class PlantConfiguration
{
    public float CactusSpawnChance { get; set; } = 0.125f;
    public float PalmTreeSpawnChance { get; set; } = 0.045f;
    public Vector2 FailRangeMinMax { get; set; } = new(5, 10);
    public int CactusClusterAttempts { get; set; } = 150;
    public Vector2 ClusterXOffset { get; set; } = new(-1, 2);
    public Vector2 ClusterYOffset { get; set; } = new(-10, 2);
}

public class PlantPass : GenPass
{
    #region Constants
    private const int SAFE_DISTANCE = 6;
    #endregion

    #region Fields
    private readonly PlantConfiguration _config;
    private int _failCount;
    private readonly int _maxWidth;
    private readonly int _maxHeight;
    #endregion

    #region Initialization
    public PlantPass(string name, float loadWeight) : base(name, loadWeight)
    {
        _config = new PlantConfiguration();
        _failCount = 0;

        // Get dimensions from the current subworld
        var subworld = SubworldSystem.Current as ArchaeaSubworld;
        _maxWidth = subworld?.Width ?? Main.maxTilesX;
        _maxHeight = subworld?.Height ?? Main.maxTilesY;
    }
    #endregion

    #region Core Generation
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Growing Desert Plants";
        GeneratePlants(progress);
    }

    private void GeneratePlants(GenerationProgress progress)
    {
        // Skip the edges of the world
        for (var x = SAFE_DISTANCE; x < _maxWidth - SAFE_DISTANCE; x++)
        {
            progress.Set(x / (float)_maxWidth);

            if (!ShouldProcessTile())
                continue;

            TryPlacePlantsAtColumn(x);
        }
    }

    private void TryPlacePlantsAtColumn(int x)
    {
        if (WorldGen.genRand.NextFloat() < _config.CactusSpawnChance)
            TryPlacePlantAtSurface(x, PlantType.Cactus);

        if (WorldGen.genRand.NextFloat() < _config.PalmTreeSpawnChance)
            TryPlacePlantAtSurface(x, PlantType.PalmTree);
    }
    #endregion

    #region Plant Placement
    private void TryPlacePlantAtSurface(int x, PlantType plantType)
    {
        // Only search down to surface level
        int surfaceLimit = (int)(_maxHeight * 0.4); // Adjust this value as needed

        for (var y = SAFE_DISTANCE; y < surfaceLimit; y++)
        {
            if (!IsSuitablePlantingSpot(x, y))
                continue;

            PlacePlant(x, y, plantType);
            SetFailCount();
            break;
        }
    }

    private void PlacePlant(int x, int y, PlantType plantType)
    {
        switch (plantType)
        {
            case PlantType.Cactus:
                PlaceCactus(x, y);
                break;

            case PlantType.PalmTree:
                PlacePalmTree(x, y);
                break;
        }
    }

    private void PlaceCactus(int x, int y)
    {
        // Ensure we're within safe bounds for the subworld
        if (!IsWithinSafeBounds(x, y))
            return;

        // Try to place initial cactus
        try
        {
            WorldGen.GrowCactus(x, y);

            // Generate cluster
            for (int attempt = 0; attempt < _config.CactusClusterAttempts; attempt++)
            {
                int clusterX = x + WorldGen.genRand.Next(
                    -(SAFE_DISTANCE - 1),
                    SAFE_DISTANCE
                );

                int clusterY = y + WorldGen.genRand.Next(
                    -(SAFE_DISTANCE - 1),
                    SAFE_DISTANCE
                );

                if (IsWithinSafeBounds(clusterX, clusterY))
                {
                    WorldGen.GrowCactus(clusterX, clusterY);
                }
            }
        }
        catch (Exception ex)
        {
            Logging.PublicLogger.Debug($"Failed to place cactus at {x}, {y}: {ex.Message}");
        }
    }

    private void PlacePalmTree(int x, int y)
    {
        if (!IsWithinSafeBounds(x, y))
            return;

        try
        {
            WorldGen.PlaceTile(x, y - 1, TileID.Saplings, mute: true);
            WorldGen.GrowPalmTree(x, y - 1);
        }
        catch (System.Exception ex)
        {
            Logging.PublicLogger.Debug($"Failed to place palm tree at {x}, {y}: {ex.Message}");
        }
    }
    #endregion

    #region Helper Methods
    private bool ShouldProcessTile()
    {
        if (_failCount > 0)
        {
            _failCount--;
            return false;
        }
        return true;
    }

    private void SetFailCount()
    {
        _failCount = WorldGen.genRand.Next(
            (int)_config.FailRangeMinMax.X,
            (int)_config.FailRangeMinMax.Y
        );
    }

    private bool IsSuitablePlantingSpot(int x, int y)
    {
        if (!IsWithinSafeBounds(x, y))
            return false;

        try
        {
            return Main.tile[x, y].HasTile == true &&
                   Main.tile[x, y].BlockType == BlockType.Solid;
        }
        catch
        {
            return false;
        }
    }

    private bool IsWithinSafeBounds(int x, int y)
    {
        return x >= SAFE_DISTANCE &&
               x < _maxWidth - SAFE_DISTANCE &&
               y >= SAFE_DISTANCE &&
               y < _maxHeight - SAFE_DISTANCE;
    }
    #endregion

    #region Types
    private enum PlantType
    {
        Cactus,
        PalmTree
    }
    #endregion
}