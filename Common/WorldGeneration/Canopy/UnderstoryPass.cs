using Reverie.lib;
using Reverie.Utilities;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Canopy;

public class UndergroundRainforestConfiguration
{
    public int ShapeVariation { get; set; } = 7; // Increased for more horizontal spread
    public float EdgeSmoothing { get; set; } = 0.215f; // Reduced for more variation
    public int MaxDepth { get; set; } = (int)(Main.maxTilesY * 0.615f);
    public float CaveFrequency { get; set; } = 0.087f;
    public float CaveThreshold { get; set; } = 0.25f;
    public float TunnelFrequency { get; set; } = 0.065f;
    public float TunnelThreshold { get; set; } = 0.35f;
    public float HorizontalExpansionFactor { get; set; } = 0.009f;
}

public class RainforestBounds
{
    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int SurfaceY { get; set; }
    public int DepthY { get; set; }
    public bool IsValid => MaxX > MinX && MinX > 0;

    public Rectangle ToRectangle() => new Rectangle(MinX, SurfaceY, MaxX - MinX, DepthY - SurfaceY);
}

public class UnderstoryPass : GenPass
{
    #region Fields
    private readonly UndergroundRainforestConfiguration _config;
    private RainforestBounds _rainforestBounds;
    private JungleBounds _jungleBounds;
    private FastNoiseLite _caveNoise;
    private FastNoiseLite _densityNoise;
    private FastNoiseLite _tunnelNoise; // New OpenSimplex2 noise for tunnels

    private int[] _leftEdges;
    private int[] _rightEdges;
    #endregion

    public UnderstoryPass() : base("Canopy Understory", 150f)
    {
        _config = new UndergroundRainforestConfiguration();
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Analyzing jungle boundaries...";

        _jungleBounds = JungleDetection.DetectJungleBoundaries();

        if (!_jungleBounds.IsValid)
        {
            progress.Message = "No jungle detected - skipping Canopy generation";
            return;
        }

        progress.Message = "Calculating Canopy placement...";
        CalculateRainforestBounds();

        if (!_rainforestBounds.IsValid)
        {
            progress.Message = "Insufficient space for Canopy - skipping generation";
            return;
        }

        progress.Message = "Growing Canopy Understory...";
        InitializeNoiseGenerators();
        GenerateRainforestUnderground(progress);
    }

    #region Rainforest Placement Logic
    private void CalculateRainforestBounds()
    {
        // First, try to detect existing surface rainforest tiles for perfect alignment
        _rainforestBounds = DetectExistingSurfaceRainforest();

        if (_rainforestBounds.IsValid)
        {
            // Found existing surface rainforest - align underground with it
            return;
        }

        // Fallback: Calculate bounds using same logic as surface generation
        CalculateRainforestBoundsFromJungle();
    }

    private RainforestBounds DetectExistingSurfaceRainforest()
    {
        var bounds = new RainforestBounds
        {
            MinX = Main.maxTilesX,
            MaxX = 0,
            SurfaceY = Main.maxTilesY,
            DepthY = 0
        };

        // Scan around world surface for rainforest tiles
        int scanStartY = Math.Max(50, (int)Main.worldSurface - 100);
        int scanEndY = Math.Min((int)Main.worldSurface + 100, Main.maxTilesY - 100);

        for (int x = 200; x < Main.maxTilesX - 200; x += 2) // Sample every 2 tiles for efficiency
        {
            for (int y = scanStartY; y < scanEndY; y += 3)
            {
                if (WorldGen.InWorld(x, y) && IsRainforestSurfaceTile(x, y))
                {
                    // Verify this is a significant rainforest area
                    if (ValidateRainforestArea(x, y, 15))
                    {
                        bounds.MinX = Math.Min(bounds.MinX, x);
                        bounds.MaxX = Math.Max(bounds.MaxX, x);
                        bounds.SurfaceY = Math.Min(bounds.SurfaceY, y);
                        bounds.DepthY = Math.Max(bounds.DepthY, y);
                    }
                }
            }
        }

        // Validate and set proper underground depth
        if (bounds.IsValid && (bounds.MaxX - bounds.MinX) >= 100)
        {
            // Expand slightly for underground coverage
            bounds.MinX = Math.Max(bounds.MinX - 10, 200);
            bounds.MaxX = Math.Min(bounds.MaxX + 10, Main.maxTilesX - 200);

            // Set proper underground depth starting from detected surface
            int avgSurfaceY = (bounds.SurfaceY + bounds.DepthY) / 2;
            bounds.SurfaceY = Math.Max(avgSurfaceY, (int)Main.worldSurface - 50);
            bounds.DepthY = bounds.SurfaceY + _config.MaxDepth;

            return bounds;
        }

        return new RainforestBounds(); // Invalid - no surface rainforest found
    }

    private bool IsRainforestSurfaceTile(int x, int y)
    {
        if (!WorldGen.InWorld(x, y)) return false;

        Tile tile = Main.tile[x, y];
        return tile.HasTile && (tile.TileType == TileID.ClayBlock ||
                               tile.WallType == WallID.FlowerUnsafe ||
                               (tile.WallType == WallID.MudUnsafe && tile.WallColor == PaintID.DeepLimePaint));
    }

    private bool ValidateRainforestArea(int centerX, int centerY, int radius = 15)
    {
        int rainforestTiles = 0;
        int totalTiles = 0;

        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (WorldGen.InWorld(x, y))
                {
                    totalTiles++;
                    if (IsRainforestSurfaceTile(x, y))
                    {
                        rainforestTiles++;
                    }
                }
            }
        }

        return totalTiles > 0 && (float)rainforestTiles / totalTiles > 0.2f; // 20% rainforest tile threshold
    }

    private void CalculateRainforestBoundsFromJungle()
    {
        Rectangle jungleRect = _jungleBounds.ToRectangle();
        bool desertOnLeft = JungleDetection.IsDesertOnLeftSideOfJungle(jungleRect);

        int leftSpace = Math.Max(0, jungleRect.Left - 200);
        int rightSpace = Math.Max(0, (Main.maxTilesX - 200) - jungleRect.Right);

        bool placeOnLeft = !desertOnLeft;

        int minRequiredSpace = (int)(Main.maxTilesX / 16.33f);
        if (placeOnLeft && leftSpace < minRequiredSpace)
        {
            placeOnLeft = false;
        }
        else if (!placeOnLeft && rightSpace < minRequiredSpace)
        {
            placeOnLeft = true;
        }

        int rainforestWidth = WorldGen.genRand.Next((int)(Main.maxTilesX / 16.33f), (int)(Main.maxTilesX / 10.43f));
        int jungleOffset = -180;

        if (placeOnLeft)
        {
            int canopyRight = jungleRect.Left - jungleOffset;
            int canopyLeft = Math.Max(canopyRight - rainforestWidth, 200);

            _rainforestBounds = new RainforestBounds
            {
                MinX = canopyLeft,
                MaxX = canopyRight,
                SurfaceY = (int)Main.worldSurface,
                DepthY = (int)Main.worldSurface + _config.MaxDepth
            };
        }
        else
        {
            int rainforestLeft = jungleRect.Right + jungleOffset;
            int rainforestRight = Math.Min(rainforestLeft + rainforestWidth, Main.maxTilesX - 200);

            _rainforestBounds = new RainforestBounds
            {
                MinX = rainforestLeft,
                MaxX = rainforestRight,
                SurfaceY = (int)Main.worldSurface,
                DepthY = (int)Main.worldSurface + _config.MaxDepth
            };
        }

        if (_rainforestBounds.MaxX - _rainforestBounds.MinX < 100)
        {
            _rainforestBounds = new RainforestBounds();
        }
    }
    #endregion

    private void InitializeNoiseGenerators()
    {
        // Main cave structure noise
        _caveNoise = new FastNoiseLite(WorldGen.genRand.Next());
        _caveNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        _caveNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _caveNoise.SetFrequency(_config.CaveFrequency);
        _caveNoise.SetFractalOctaves(4);

        // Cave density variation noise
        _densityNoise = new FastNoiseLite(WorldGen.genRand.Next());
        _densityNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _densityNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _densityNoise.SetFrequency(0.075f);
        _densityNoise.SetFractalOctaves(3);

        _tunnelNoise = new FastNoiseLite(WorldGen.genRand.Next());
        _tunnelNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _tunnelNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _tunnelNoise.SetFrequency(_config.TunnelFrequency);
        _tunnelNoise.SetFractalOctaves(3);
    }

    #region Shape-Based Underground Generation
    private void GenerateRainforestUnderground(GenerationProgress progress)
    {
        progress.Message = "Initializing Canopy shape...";

        InitializeShapeArrays();

        progress.Message = "Generating underground structure...";

        GenerateRainforestShape(progress);

        progress.Message = "Carving Canopy caves...";

        ApplyRainforestShape(progress);
    }

    private void InitializeShapeArrays()
    {
        _leftEdges = new int[_config.MaxDepth];
        _rightEdges = new int[_config.MaxDepth];

        // Initialize with surface bounds
        _leftEdges[0] = _rainforestBounds.MinX;
        _rightEdges[0] = _rainforestBounds.MaxX;
    }

    private void GenerateRainforestShape(GenerationProgress progress)
    {
        int baseWidth = _rainforestBounds.MaxX - _rainforestBounds.MinX;
        int centerX = (_rainforestBounds.MinX + _rainforestBounds.MaxX) / 2;

        for (int depth = 1; depth < _config.MaxDepth; depth++)
        {
            progress.Set((double)depth / _config.MaxDepth * 0.3); // First 30% of progress

            // Calculate depth-based expansion factor for wider underground areas
            float depthRatio = (float)depth / _config.MaxDepth;
            float expansionFactor = 1.0f + (depthRatio * _config.HorizontalExpansionFactor);

            // Generate larger random variations for more horizontal spread
            int leftDrift = WorldGen.genRand.Next(-_config.ShapeVariation, _config.ShapeVariation + 1);
            int rightDrift = WorldGen.genRand.Next(-_config.ShapeVariation, _config.ShapeVariation + 1);

            // Apply drift to previous layer
            int newLeft = _leftEdges[depth - 1] + leftDrift;
            int newRight = _rightEdges[depth - 1] + rightDrift;

            // Apply smoothing (reduced for more variation)
            if (depth > 1)
            {
                newLeft = (int)(newLeft * (1f - _config.EdgeSmoothing) + _leftEdges[depth - 1] * _config.EdgeSmoothing);
                newRight = (int)(newRight * (1f - _config.EdgeSmoothing) + _rightEdges[depth - 1] * _config.EdgeSmoothing);
            }

            // Add stronger noise-based variation for organic shapes
            float noiseLeft = _densityNoise.GetNoise(_rainforestBounds.MinX, depth * 0.08f) * 4f; // Increased strength
            float noiseRight = _densityNoise.GetNoise(_rainforestBounds.MaxX, depth * 0.08f) * 4f;

            newLeft += (int)noiseLeft;
            newRight += (int)noiseRight;

            // Apply depth-based expansion from center
            int currentWidth = (int)((newRight - newLeft) * expansionFactor);
            int currentCenter = (newLeft + newRight) / 2;

            newLeft = currentCenter - currentWidth / 2;
            newRight = currentCenter + currentWidth / 2;

            // Constrain within reasonable bounds (more generous for wider spread)
            newLeft = Math.Max(newLeft, _rainforestBounds.MinX - (int)(baseWidth * expansionFactor * 0.35f));
            newRight = Math.Min(newRight, _rainforestBounds.MaxX + (int)(baseWidth * expansionFactor * 0.35f));

            // Ensure minimum width (increased for wider areas)
            int minWidth = Math.Max(baseWidth / 2, (int)(baseWidth * expansionFactor * 0.45f));
            if (newRight - newLeft < minWidth)
            {
                int center = (newLeft + newRight) / 2;
                int halfMinWidth = minWidth / 2;
                newLeft = center - halfMinWidth;
                newRight = center + halfMinWidth;
            }

            _leftEdges[depth] = newLeft;
            _rightEdges[depth] = newRight;
        }
    }

    private void ApplyRainforestShape(GenerationProgress progress)
    {
        for (int depth = 0; depth < _config.MaxDepth; depth++)
        {
            progress.Set(0.3 + (double)depth / _config.MaxDepth * 0.7); // Remaining 70% of progress

            int currentY = _rainforestBounds.SurfaceY + depth;
            if (!IsValidY(currentY)) continue;

            int leftBound = _leftEdges[depth];
            int rightBound = _rightEdges[depth];

            for (int x = leftBound; x < rightBound; x++)
            {
                if (!IsValidX(x)) continue;

                ProcessRainforestTile(x, currentY, depth);
            }
        }
    }

    private void ProcessRainforestTile(int x, int y, int depth)
    {
        if (!WorldGen.InWorld(x, y)) return;

        Tile tile = Main.tile[x, y];

        // Determine if we should carve a cave here using multiple noise layers
        bool shouldCarve = ShouldCarveAt(x, y, depth);

        if (shouldCarve)
        {
            // Carve cave
            tile.HasTile = false;
            tile.IsHalfBlock = false;
            tile.Slope = SlopeType.Solid;
        }
        else
        {
            if (!tile.HasTile)
            {
                tile.HasTile = true;
            }

            // Transform existing tiles or set new tile type
            if (ShouldTransformTile(tile) || !tile.HasTile)
            {
                SetRainforestTileType(tile, depth);
            }

            // Set appropriate walls
            if (depth <= 10 && (tile.WallType == WallID.None || tile.WallType == WallID.Stone))
            {
                tile.WallType = WallID.FlowerUnsafe;
                tile.WallColor = PaintID.DeepLimePaint;
            }
            else if (tile.WallType == WallID.None || tile.WallType == WallID.Stone)
            {
                tile.WallType = WallID.MudUnsafe;
            }
        }
    }

    private bool ShouldCarveAt(int x, int y, int depth)
    {
        // Get primary cave noise
        float caveValue = _caveNoise.GetNoise(x / 2.7f, y / 3.3f);

        // Get density variation
        float density = _densityNoise.GetNoise(x, y) * 0.5f + 0.5f; // Remap to 0-1

        // Get tunnel connection noise (OpenSimplex2)
        float tunnelValue = _tunnelNoise.GetNoise(x / 2.3f, y / 1.7f);

        // Create depth-based cave density (more caves deeper underground)
        float depthFactor = (float)depth / _config.MaxDepth;
        depthFactor = Math.Min(1.0f, depthFactor * 1.2f);

        // Calculate distance from biome edges for tapering
        float edgeFactor = CalculateEdgeFactor(x, depth);

        // Combine factors for final threshold
        float combinedFactor = edgeFactor * (0.6f + depthFactor * 0.4f);
        float caveThreshold = _config.CaveThreshold - (density * combinedFactor * 0.3f);

        // Check if we should carve based on main cave noise
        bool mainCave = caveValue > caveThreshold;

        // Check if we should carve tunnel connections
        bool tunnelCave = tunnelValue > _config.TunnelThreshold && depthFactor > 0.1f; // Only deeper areas

        // Combine both carving conditions
        return mainCave || tunnelCave;
    }

    private float CalculateEdgeFactor(int x, int depth)
    {
        int leftBound = _leftEdges[depth];
        int rightBound = _rightEdges[depth];
        int width = rightBound - leftBound;

        if (width <= 0) return 0f;

        float distFromLeft = (x - leftBound) / (float)width;
        float distFromRight = (rightBound - x) / (float)width;
        float minDistFromEdge = Math.Min(distFromLeft, distFromRight);

        float edgeTaperZone = 0.09f; // lower = wider spread
        if (minDistFromEdge < edgeTaperZone)
        {
            return (float)(0.5 * (1 + Math.Cos(Math.PI * (1 - minDistFromEdge / edgeTaperZone))));
        }

        return 1.0f;
    }

    private bool ShouldTransformTile(Tile tile)
    {
        return tile.TileType != TileID.LihzahrdBrick ||
               tile.TileType != TileID.LihzahrdAltar || tile.TileType != TileID.Sand || tile.TileType != TileID.Sandstone;
    }

    private void SetRainforestTileType(Tile tile, int depth)
    {
        if (depth <= _config.MaxDepth * 0.345f)
        {
            tile.TileType = TileID.ClayBlock;
        }
        else
        {
            tile.TileType = TileID.LivingWood;
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