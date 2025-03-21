using Reverie.Common.Subworlds.Sylvanwalde;
using StructureHelper;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.Archaea.Generation;

public class SylvanPlantConfiguration
{
    public float TreeSpawnChance { get; set; } = 5f;
    public Vector2 FailRangeMinMax { get; set; } = new(5, 10);
}

public class SylvanPlantPass : GenPass
{
    private const int SAFE_DISTANCE = 1;

    private readonly PlantConfiguration _config;
    private int _failCount;
    private readonly int _maxWidth;
    private readonly int _maxHeight;


    #region Initialization
    public SylvanPlantPass() : base("[Sylvan] Plants", 77f)
    {
        _config = new PlantConfiguration();
        _failCount = 0;

        var subworld = SubworldSystem.Current as SylvanSub;
        _maxWidth = subworld?.Width ?? Main.maxTilesX;
        _maxHeight = subworld?.Height ?? Main.maxTilesY;
    }
    #endregion



    #region Core Generation
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Growing Plants";
        GeneratePlants(progress);
    }

    private void GeneratePlants(GenerationProgress progress)
    {
        for (var x = SAFE_DISTANCE; x < _maxWidth - SAFE_DISTANCE; x++)
        {
            progress.Set(x / (float)_maxWidth);

            if (!ShouldProcessTile())
                continue;

            if (WorldGen.genRand.NextFloat() < _config.TreeSpawnChance)
                PlaceFoliages(x);
        }
    }
    #endregion

    #region Plant Placement
    private void PlaceFoliages(int x)
    {
        int surfaceLimit = (int)(_maxHeight * 0.4);

        for (var y = SAFE_DISTANCE; y < surfaceLimit; y++)
        {
            if (!IsSuitablePlantingSpot(x, y))
                continue;

            if (!IsWithinSafeBounds(x, y))
                return;

            try
            {
                WorldGen.PlaceTile(x, y - 1, TileID.Saplings, mute: true);
                WorldGen.GrowTree(x, y - 1);
            }
            catch (Exception ex)
            {
                Logging.PublicLogger.Debug($"Failed to place tree at {x}, {y}: {ex.Message}");
            }

            SetFailCount();
            break;
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
}
