using Reverie.Common.Subworlds.Sylvanwalde.Generation.DruidsGarden;
using Reverie.Content.Tiles.Sylvanwalde;
using Reverie.lib;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.Sylvanwalde.Generation;

public class SylvanConfiguration
{
    public float NoiseFrequency { get; set; } = 0.002f;
    public int BaseHeightOffset { get; set; } = 15;
    public int HillVariation { get; set; } = 49;
    public int LoamDepth { get; set; } = (int)(Main.maxTilesY / 1.5f);

    public int TunnelCount { get; set; } = 6;
    public double TunnelDistance { get; set; } = 120.0;
    public double MainTunnelSize { get; set; } = 5.0;
    public int BranchesPerTunnel { get; set; } = 4;

}

public class SylvanTerrainPass : GenPass
{
    #region Constants
    private const float SURFACE_LAYER_RATIO = 1f / 4f;
    #endregion

    #region Fields
    private readonly SylvanConfiguration _config;
    private FastNoiseLite groundNoise;
    private int _surfaceHeight;
    private int _highestPeak = int.MaxValue;
    private int _lowestValley = 0;
    private List<Point> _tunnelEntrances = new List<Point>();
    #endregion

    #region Public Properties
    public int SurfaceHeight => _surfaceHeight;
    public int HighestPeak => _highestPeak;
    public int LowestValley => _lowestValley;
    public List<Point> TunnelEntrances => _tunnelEntrances;
    public SylvanConfiguration Config => _config;
    public float GetSurfaceLayerRatio() => SURFACE_LAYER_RATIO;
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
        progress.Set(0.6f);

        progress.Message = "Adding Surface Details";
        GenerateSurfaceRocks();
        progress.Set(0.8f);

        progress.Message = "Creating Root Tunnel Networks";
        CreateRootTunnels();
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

    private void GenerateSurfaceRocks()
    {
        var mediumFormationCount = (int)(Main.maxTilesX * 0.02f); // Medium rock formations - 50% fewer
        var smallFormationCount = (int)(Main.maxTilesX * 0.03f);  // Small rock clusters - Less than half

        HashSet<int> usedPositions = new HashSet<int>();

        for (var i = 0; i < mediumFormationCount; i++)
        {
            int x;
            int attempts = 0;
            do
            {
                x = WorldGen.genRand.Next(30, Main.maxTilesX - 30);
                attempts++;
            } while (IsPositionTooClose(x, usedPositions, 95) && attempts < 50);

            usedPositions.Add(x);
            var surfaceY = FindSurfaceHeight(x);

            if (surfaceY > 0)
            {
                WorldGen.TileRunner(
                    x, surfaceY,
                    WorldGen.genRand.Next(5, 9),
                    WorldGen.genRand.Next(3, 5),
                    (ushort)ModContent.TileType<CobblestoneTile>(),
                    true
                );
            }
        }

        for (var i = 0; i < smallFormationCount; i++)
        {
            int x;
            int attempts = 0;
            do
            {
                x = WorldGen.genRand.Next(20, Main.maxTilesX - 20);
                attempts++;
            } while (IsPositionTooClose(x, usedPositions, 75) && attempts < 50);

            usedPositions.Add(x);
            var surfaceY = FindSurfaceHeight(x);

            if (surfaceY > 0)
            {
                WorldGen.TileRunner(
                    x, surfaceY,
                    WorldGen.genRand.Next(2, 5),
                    WorldGen.genRand.Next(1, 3),
                    (ushort)ModContent.TileType<CobblestoneTile>(),
                    true
                );
            }
        }

        var embeddedCount = (int)(Main.maxTilesX * 0.015f);
        for (var i = 0; i < embeddedCount; i++)
        {
            int x;
            int attempts = 0;
            do
            {
                x = WorldGen.genRand.Next(20, Main.maxTilesX - 20);
                attempts++;
            } while (IsPositionTooClose(x, usedPositions, 70) && attempts < 50); // Minimum distance of 70 tiles

            usedPositions.Add(x);
            var surfaceY = FindSurfaceHeight(x);

            if (surfaceY > 0)
            {
                var hillX = x;
                var hillY = surfaceY;

                for (var j = 0; j < 10; j++)
                {
                    var checkX = x + WorldGen.genRand.Next(-20, 21);
                    if (checkX > 0 && checkX < Main.maxTilesX)
                    {
                        var checkY = FindSurfaceHeight(checkX);
                        if (checkY > 0 && checkY < hillY)
                        {
                            hillX = checkX;
                            hillY = checkY;
                        }
                    }
                }

                if (hillY < surfaceY - 3)
                {
                    WorldGen.TileRunner(
                        hillX, hillY + 2, 
                        WorldGen.genRand.Next(5, 8),
                        WorldGen.genRand.Next(2, 5),
                        (ushort)ModContent.TileType<CobblestoneTile>(),
                        true
                    );
                }
            }
        }
    }

    #endregion
    #region Root Tunnel System
    private void CreateRootTunnels()
    {
        _tunnelEntrances.Clear();

        // Create evenly spaced entrances across the world
        int spacing = Main.maxTilesX / (_config.TunnelCount + 1);

        for (int i = 0; i < _config.TunnelCount; i++)
        {
            // Calculate base position with some randomization
            int xPos = (i + 1) * spacing + WorldGen.genRand.Next(-spacing / 4, spacing / 4);

            // Find the surface at this position
            int yPos = FindSurfaceHeight(xPos);

            if (yPos > 0)
            {
                // Create an entrance (a small clearing at the surface)
                CreateTunnelEntrance(xPos, yPos);

                // Store the entrance position for later reference
                _tunnelEntrances.Add(new Point(xPos, yPos));

                // Create the main tunnel going downward
                CreateMainTunnel(xPos, yPos);
            }
        }
    }

    private void CreateTunnelEntrance(int x, int y)
    {
        // Create a small clearing at the entrance
        int entranceSize = WorldGen.genRand.Next(5, 9);

        for (int i = -entranceSize; i <= entranceSize; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                int clearX = x + i;
                int clearY = y + j;

                if (IsValidPosition(clearX, clearY))
                {
                    Main.tile[clearX, clearY].ClearTile();
                }
            }
        }
    }

    private void CreateMainTunnel(int x, int y)
    {
        // Main tunnel goes downward with some randomization
        double mainAngle = 1.57; // Approximately PI/2 (downward)
        double randomization = 0.3; // How much variation in angle

        // Add slight randomization to the angle
        mainAngle += WorldGen.genRand.NextDouble() * randomization - randomization / 2;

        // Create main tunnel using ShapeRoot
        ShapeRoot mainTunnel = new ShapeRoot(
            mainAngle,
            _config.TunnelDistance,
            _config.MainTunnelSize,
            _config.MainTunnelSize * 0.8 // Slightly smaller at the end
        );

        // Action to clear tiles
        ClearTileAction clearAction = new ClearTileAction();

        // Create the main tunnel
        mainTunnel.Perform(new Point(x, y), clearAction);

        // Create branch tunnels along the main tunnel
        CreateBranchTunnels(x, y, mainAngle);
    }

    private void CreateBranchTunnels(int startX, int startY, double mainAngle)
    {
        // Calculate how far apart branches should be
        double branchSpacing = _config.TunnelDistance / (_config.BranchesPerTunnel + 1);

        for (int i = 1; i <= _config.BranchesPerTunnel; i++)
        {
            // Calculate position along the main tunnel
            double distanceAlongTunnel = i * branchSpacing;

            // Calculate the position where the branch starts
            int branchX = startX + (int)(Math.Cos(mainAngle) * distanceAlongTunnel);
            int branchY = startY + (int)(Math.Sin(mainAngle) * distanceAlongTunnel);

            // Ensure we're still in valid position
            if (!IsValidPosition(branchX, branchY))
                continue;

            // Skip if the position is not already cleared (should be along the main tunnel)
            if (Main.tile[branchX, branchY].HasTile)
                continue;

            // Create branch tunnels in random directions (but not back up)
            double branchAngle;
            if (i % 2 == 0)
            {
                // Branch to the right
                branchAngle = mainAngle - 1.0 + WorldGen.genRand.NextDouble() * 0.5;
            }
            else
            {
                // Branch to the left
                branchAngle = mainAngle + 1.0 - WorldGen.genRand.NextDouble() * 0.5;
            }

            // Random length and size for branches
            double branchDistance = _config.TunnelDistance * (0.3 + WorldGen.genRand.NextDouble() * 0.3);
            double branchSize = _config.MainTunnelSize * (0.6 + WorldGen.genRand.NextDouble() * 0.2);

            // Create the branch tunnel
            ShapeRoot branchTunnel = new ShapeRoot(
                branchAngle,
                branchDistance,
                branchSize,
                branchSize * 0.5 // Taper toward the end
            );

            ClearTileAction clearAction = new ClearTileAction();
            branchTunnel.Perform(new Point(branchX, branchY), clearAction);
        }
    }
    #endregion

    #region Validation
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY;
    }

    private bool IsValidX(int x)
    {
        return x >= 0 && x < Main.maxTilesX;
    }

    private bool IsValidY(int y)
    {
        return y >= 0 && y < Main.maxTilesY;
    }

    private int CalculatePeakHeight(int x)
    {
        var noiseValue = groundNoise.GetNoise(x / 2, Main.maxTilesY / 2);
        var height = (int)(_surfaceHeight - _config.BaseHeightOffset + noiseValue * _config.HillVariation);
        return Math.Clamp(height, 0, Main.maxTilesY - 1);
    }

    private int FindSurfaceHeight(int x)
    {
        if (x < 0 || x >= Main.maxTilesX)
            return -1;

        int startY = (int)(Main.worldSurface * 0.5f);

        for (int y = startY; y < Main.maxTilesY - 5; y++)
        {
            if (Main.tile[x, y].HasTile && IsSolidTerrain(Main.tile[x, y].TileType))
            {
                return y;
            }
        }

        return -1;
    }

    private bool IsSolidTerrain(ushort tileType)
    {
        return tileType == (ushort)ModContent.TileType<LoamTile>() ||
               tileType == (ushort)ModContent.TileType<LoamGrassTile>();
    }

    private bool IsPositionTooClose(int x, HashSet<int> usedPositions, int minDistance)
    {
        foreach (var pos in usedPositions)
        {
            if (Math.Abs(x - pos) < minDistance)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}