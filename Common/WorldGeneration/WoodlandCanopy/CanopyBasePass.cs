using Reverie.Content.Tiles.Rainforest.Surface;
using Reverie.lib;
using Terraria.GameContent.Biomes;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.WoodlandCanopy;

public class CanopyConfiguration
{
    public float HillFrequency { get; set; } = 0.0175f;
    public int BaseHeightOffset { get; set; } = 0;
    public int HillHeightVariation { get; set; } = 62;
    public int CanopyDepth { get; set; } = (int)(Main.maxTilesY / 6.7647f);
}

public class JungleBounds
{
    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int SurfaceY { get; set; }
    public int Height { get; set; }
    public bool IsValid => MaxX > MinX && MinX > 0;

    public Rectangle ToRectangle() => new Rectangle(MinX, SurfaceY, MaxX - MinX, Height);
}

public class CanopyBasePass : GenPass
{
    #region Fields
    private readonly CanopyConfiguration _config;
    private FastNoiseLite _hillNoise;
    private int _canopyLeft, _canopyRight;
    private JungleBounds _jungleBounds;
    #endregion

    public CanopyBasePass() : base("Rainforest Overworld", 80f)
    {
        _config = new CanopyConfiguration();
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Analyzing biome boundaries...";

        Initialize();

        // Multi-layered jungle detection approach
        _jungleBounds = DetectJungleBoundaries();

        if (!_jungleBounds.IsValid)
        {
            progress.Message = "Failed to detect jungle boundaries - skipping Rainforest generation";
            return;
        }

        progress.Message = "Growing Rainforest Biome adjacent to jungle...";
        CalculateCanopyBounds();
        GenerateTerrain(progress);
    }

    private void Initialize()
    {
        _hillNoise = new FastNoiseLite();
        _hillNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        _hillNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _hillNoise.SetFractalOctaves(3);
        _hillNoise.SetFrequency(_config.HillFrequency);
    }

    #region Jungle & Desert Detection
    private JungleBounds DetectJungleBoundaries()
    {
        // Layer 1: Validate GenVars jungle coordinates
        JungleBounds bounds = ValidateGenVarsJungle();
        if (bounds.IsValid)
        {
            return bounds;
        }

        // Layer 2: Comprehensive tile scanning
        bounds = ScanForJungleBiome();
        if (bounds.IsValid)
        {
            return bounds;
        }

        // Layer 3: Positional estimation based on Terraria's generation rules
        return EstimateJungleLocation();
    }

    private JungleBounds ValidateGenVarsJungle()
    {
        var bounds = new JungleBounds();

        // Validate GenVars coordinates with bounds checking
        if (GenVars.jungleMinX > 0 && GenVars.jungleMaxX > GenVars.jungleMinX &&
            GenVars.jungleMaxX < Main.maxTilesX - 200)
        {
            bounds.MinX = GenVars.jungleMinX;
            bounds.MaxX = GenVars.jungleMaxX;
            bounds.SurfaceY = (int)Main.worldSurface;
            bounds.Height = 400;

            // Verify by checking if area actually contains jungle tiles
            if (ValidateJungleArea(bounds.MinX + (bounds.MaxX - bounds.MinX) / 2, bounds.SurfaceY + 50))
            {
                return bounds;
            }
        }

        return new JungleBounds(); // Invalid
    }

    private JungleBounds ScanForJungleBiome()
    {
        var bounds = new JungleBounds
        {
            MinX = Main.maxTilesX,
            MaxX = 0,
            SurfaceY = (int)Main.worldSurface,
            Height = 400
        };

        int scanStartY = (int)Main.worldSurface;
        int scanEndY = Math.Min((int)Main.worldSurface + 200, Main.maxTilesY - 100);

        // Scan for jungle tiles in surface area
        for (int x = 200; x < Main.maxTilesX - 200; x += 5) // Sample every 5 tiles for efficiency
        {
            for (int y = scanStartY; y < scanEndY; y += 3)
            {
                if (WorldGen.InWorld(x, y) && IsJungleTile(x, y))
                {
                    // Verify this is a significant jungle area, not just scattered tiles
                    if (ValidateJungleArea(x, y, 25))
                    {
                        bounds.MinX = Math.Min(bounds.MinX, x);
                        bounds.MaxX = Math.Max(bounds.MaxX, x);
                    }
                }
            }
        }

        // Expand bounds slightly to ensure we capture the full biome
        if (bounds.IsValid)
        {
            bounds.MinX = Math.Max(bounds.MinX - 50, 200);
            bounds.MaxX = Math.Min(bounds.MaxX + 50, Main.maxTilesX - 200);
        }

        return bounds;
    }

    private JungleBounds EstimateJungleLocation()
    {
        // Jungle typically spawns opposite side from Dungeon
        bool jungleOnLeft = Main.dungeonX > Main.maxTilesX / 2;
        int estimatedCenter = jungleOnLeft ? Main.maxTilesX / 4 : (Main.maxTilesX * 3) / 4;
        int estimatedWidth = Main.maxTilesX / 5; // Jungle typically spans ~20% of world width

        return new JungleBounds
        {
            MinX = Math.Max(estimatedCenter - estimatedWidth / 2, 200),
            MaxX = Math.Min(estimatedCenter + estimatedWidth / 2, Main.maxTilesX - 200),
            SurfaceY = (int)Main.worldSurface,
            Height = 400
        };
    }

    private bool IsJungleTile(int x, int y)
    {
        if (!WorldGen.InWorld(x, y)) return false;

        Tile tile = Main.tile[x, y];
        return tile.HasTile && (tile.TileType == TileID.JungleGrass ||
                               tile.TileType == TileID.Mud ||
                               tile.TileType == TileID.JunglePlants ||
                               tile.TileType == TileID.JungleVines);
    }

    private bool ValidateJungleArea(int centerX, int centerY, int radius = 25)
    {
        int jungleTiles = 0;
        int totalTiles = 0;

        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (WorldGen.InWorld(x, y))
                {
                    Tile tile = Main.tile[x, y];
                    if (tile.HasTile)
                    {
                        totalTiles++;
                        if (IsJungleTile(x, y))
                        {
                            jungleTiles++;
                        }
                    }
                }
            }
        }

        return totalTiles > 0 && (float)jungleTiles / totalTiles > 0.25f; // 25% jungle tile threshold
    }
   
    private void CalculateCanopyBounds()
    {
        // Analyze jungle edges to determine optimal canopy placement
        Rectangle jungleRect = _jungleBounds.ToRectangle();
        int jungleCenter = (jungleRect.Left + jungleRect.Right) / 2;
        int worldCenter = Main.maxTilesX / 2;

        // Determine desert location to avoid conflicts
        bool desertOnLeft = IsDesertOnLeftSideOfJungle(jungleRect);

        int leftSpace = Math.Max(0, jungleRect.Left - 200);
        int rightSpace = Math.Max(0, (Main.maxTilesX - 200) - jungleRect.Right);

        // Primary logic: Place canopy opposite to desert
        bool placeOnLeft;

        if (desertOnLeft)
        {
            // Desert is on left side of jungle, prefer right side for canopy
            placeOnLeft = false;

            // But check if right side has sufficient space
            int minRequiredSpace = (int)(Main.maxTilesX / 20.33f); // Minimum canopy width
            if (rightSpace < minRequiredSpace)
            {
                // Not enough space on right, check if left is viable despite desert
                if (leftSpace >= minRequiredSpace)
                {
                    placeOnLeft = true; // Override desert avoidance due to space constraints
                }
                else
                {
                    throw new InvalidOperationException("Insufficient space for Rainforest biome - blocked by desert and limited world space");
                }
            }
        }
        else
        {
            // Desert is on right side of jungle (or not detected), prefer left side for canopy
            placeOnLeft = true;

            // Check if left side has sufficient space
            int minRequiredSpace = (int)(Main.maxTilesX / 20.33f);
            if (leftSpace < minRequiredSpace)
            {
                // Not enough space on left, check if right is viable
                if (rightSpace >= minRequiredSpace)
                {
                    placeOnLeft = false;
                }
                else
                {
                    throw new InvalidOperationException("Insufficient space for Rainforest biome - blocked by desert and limited world space");
                }
            }
        }

        // Fallback logic: If desert location is unclear, use original space-based logic
        if (!IsDesertClear())
        {
            if (Math.Abs(leftSpace - rightSpace) > 100)
            {
                placeOnLeft = leftSpace > rightSpace;
            }
            else
            {
                // If spaces are similar, place on side away from world center
                placeOnLeft = jungleCenter > worldCenter;
            }
        }

        // Calculate canopy dimensions
        int canopyWidth = WorldGen.genRand.Next((int)(Main.maxTilesX / 20.33f), (int)(Main.maxTilesX / 10.43f));
        int jungleOffset = -160; // Gap between jungle and canopy (increased to avoid desert overlap)

        // Position canopy with proper spacing analysis
        if (placeOnLeft)
        {
            _canopyRight = jungleRect.Left - jungleOffset;
            _canopyLeft = Math.Max(_canopyRight - canopyWidth, 200);

            // If there's not enough space, adjust the width
            if (_canopyLeft <= 200)
            {
                _canopyLeft = 200;
                canopyWidth = _canopyRight - _canopyLeft;
            }
        }
        else
        {
            _canopyLeft = jungleRect.Right + jungleOffset;
            _canopyRight = Math.Min(_canopyLeft + canopyWidth, Main.maxTilesX - 200);

            // If there's not enough space, adjust the width
            if (_canopyRight >= Main.maxTilesX - 200)
            {
                _canopyRight = Main.maxTilesX - 200;
                canopyWidth = _canopyRight - _canopyLeft;
            }
        }

        // Ensure minimum viable width
        if (canopyWidth < 100)
        {
            throw new InvalidOperationException("Insufficient space for Rainforest biome placement adjacent to jungle");
        }

        // Final validation: Check for conflicts with desert boundaries
        ValidateCanopyDesertSeparation();
    }

    private bool IsDesertOnLeftSideOfJungle(Rectangle jungleRect)
    {
        // Try multiple GenVars for desert location
        int desertX = -1;

        // Primary check: UndergroundDesertLocation
        if (GenVars.UndergroundDesertLocation.X > 0 && GenVars.UndergroundDesertLocation.X < Main.maxTilesX)
        {
            desertX = (int)GenVars.UndergroundDesertLocation.X;
        }
        // Fallback: UndergroundDesertHiveLocation  
        else if (GenVars.UndergroundDesertHiveLocation.X > 0 && GenVars.UndergroundDesertHiveLocation.X < Main.maxTilesX)
        {
            desertX = (int)GenVars.UndergroundDesertHiveLocation.X;
        }
        // Last resort: Use world layout heuristics
        else
        {
            // Desert typically spawns on same side as dungeon in many worlds
            // This is a rough estimation when GenVars fail
            bool dungeonOnLeft = Main.dungeonX < Main.maxTilesX / 2;
            desertX = dungeonOnLeft ? Main.maxTilesX / 4 : (Main.maxTilesX * 3) / 4;
        }

        if (desertX == -1) return false; // Could not determine desert location

        int jungleCenter = (jungleRect.Left + jungleRect.Right) / 2;
        return desertX < jungleCenter; // Desert is to the left of jungle center
    }

    private bool IsDesertClear()
    {
        // Check if we have reliable desert location data
        return (GenVars.UndergroundDesertLocation.X > 0 && GenVars.UndergroundDesertLocation.X < Main.maxTilesX) ||
               (GenVars.UndergroundDesertHiveLocation.X > 0 && GenVars.UndergroundDesertHiveLocation.X < Main.maxTilesX);
    }

    private void ValidateCanopyDesertSeparation()
    {
        // Ensure adequate separation between canopy and desert
        int desertX = -1;

        if (GenVars.UndergroundDesertLocation.X > 0)
            desertX = (int)GenVars.UndergroundDesertLocation.X;
        else if (GenVars.UndergroundDesertHiveLocation.X > 0)
            desertX = (int)GenVars.UndergroundDesertHiveLocation.X;

        if (desertX > 0)
        {
            int minSeparation = 200; // Minimum tiles between canopy and desert

            // Check for overlap or insufficient separation
            if ((_canopyLeft <= desertX + minSeparation && _canopyRight >= desertX - minSeparation))
            {
                Instance.Logger.Warn($"Rainforest biome ({_canopyLeft}-{_canopyRight}) may be too close to desert (center: {desertX})");

                // Attempt to adjust bounds if possible
                if (desertX < _canopyLeft) // Desert is to the left
                {
                    _canopyLeft = Math.Min(_canopyLeft + minSeparation, Main.maxTilesX - 300);
                }
                else // Desert is to the right
                {
                    _canopyRight = Math.Max(_canopyRight - minSeparation, 300);
                }
            }
        }
    }
    #endregion

    private void GenerateTerrain(GenerationProgress progress)
    {
        progress.Message = "Preparing canopy terrain foundation...";

        // Step 1: Find edge heights for contouring
        int leftEdgeHeight = GetTerrainHeightAt(_canopyLeft - 1);
        int rightEdgeHeight = GetTerrainHeightAt(_canopyRight + 1);

        progress.Message = "Clearing existing terrain...";

        // Step 2: Clear the entire canopy area
        ClearArea();

        progress.Message = "Generating canopy terrain with noise...";

        // Step 3: Generate base terrain heights using noise
        int[] terrainHeights = GenerateTerrainHeights(leftEdgeHeight, rightEdgeHeight);

        progress.Message = "Building canopy columns...";

        // Step 4: Fill terrain columns based on generated heights
        int totalColumns = _canopyRight - _canopyLeft;
        for (int i = 0; i < totalColumns; i++)
        {
            int x = _canopyLeft + i;
            progress.Set((double)i / totalColumns);

            FillCanopyColumn(x, terrainHeights[i]);
        }
    }

    private void PlaceTerrain(int x, int y, int surfaceY)
    {
        Tile tile = Main.tile[x, y];
        tile.HasTile = true;

        int depthFromSurface = y - surfaceY;
        if (depthFromSurface <= 1)
        {
            tile.HasTile = false;
            tile.WallType = WallID.FlowerUnsafe;
            tile.WallColor = PaintID.LimePaint;
        }
        else if (depthFromSurface <= 10)
        {
            tile.TileType = (ushort)ModContent.TileType<OxisolTile>();
            tile.WallType = WallID.MudUnsafe;
        }
        else if (depthFromSurface <= _config.CanopyDepth * 0.02f)
        {
            tile.TileType = TileID.ClayBlock;
        }
        else if (depthFromSurface <= _config.CanopyDepth * 0.12f)
        {
            tile.TileType = TileID.Mud;
        }
        //else if (depthFromSurface <= _config.CanopyDepth * 0.65f)
        //{
        //    tile.TileType = (ushort)ModContent.TileType<OxisolTile>();
        //}
        else
        {
            tile.TileType = (ushort)ModContent.TileType<OxisolTile>();
        }
    }

    #region Helper Methods
    private int GetTerrainHeightAt(int x)
    {
        if (!WorldGen.InWorld(x, 0)) return (int)Main.worldSurface - 200;

        // Scan downward to find the surface
        for (int y = (int)Main.worldSurface - 250; y < Main.maxTilesY - 300; y++)
        {
            if (WorldGen.InWorld(x, y) && Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
            {
                return y;
            }
        }

        return (int)Main.worldSurface; // Fallback
    }

    private void ClearArea()
    {
        // Clear from much higher up to ensure all existing terrain is removed
        int clearStartY = Math.Max(0, (int)Main.worldSurface - 300); // Clear much higher above surface
        int clearEndY = (int)Main.worldSurface + 100; // Only clear down to where we'll place surface, not deep underground

        for (int x = _canopyLeft; x < _canopyRight; x++)
        {
            for (int y = clearStartY; y < Math.Min(Main.maxTilesY, clearEndY); y++)
            {
                if (WorldGen.InWorld(x, y))
                {
                    Main.tile[x, y].ClearTile();
                }
            }
        }
    }

    private int[] GenerateTerrainHeights(int leftEdgeHeight, int rightEdgeHeight)
    {
        int width = _canopyRight - _canopyLeft;
        int[] heights = new int[width];

        // Calculate base height, but ensure it's reasonable relative to world surface
        int baseHeight = (leftEdgeHeight + rightEdgeHeight) / 2;

        // Make sure we're generating terrain at a proper surface level
        int targetSurfaceHeight = (int)Main.worldSurface - _config.BaseHeightOffset;
        baseHeight = Math.Min(baseHeight, targetSurfaceHeight);

        // Ensure base height isn't too deep - we want visible hills
        baseHeight = Math.Max(baseHeight, (int)Main.worldSurface - 180);

        for (int i = 0; i < width; i++)
        {
            int x = _canopyLeft + i;
            float normalizedPosition = (float)i / (width - 1); // 0.0 to 1.0 across the biome

            // Generate noise-based height variation with more intensity
            float primaryNoise = _hillNoise.GetNoise(x * 0.5f, 0f);
            float secondaryNoise = _hillNoise.GetNoise(x * 1.2f, 100f) * 0.3f;
            float combinedNoise = primaryNoise + secondaryNoise;

            // Increase height variation for more prominent terrain
            int noiseHeight = (int)(combinedNoise * _config.HillHeightVariation * 1.5f); // 50% more variation

            // Apply edge tapering for smooth transitions
            float taperFactor = GetTaperFactor(normalizedPosition, width);
            int taperedNoiseHeight = (int)(noiseHeight * taperFactor);

            // Blend with edge heights for smooth contouring
            int contourHeight = GetContourHeight(normalizedPosition, baseHeight, leftEdgeHeight, rightEdgeHeight, taperFactor);

            // Combine contoured base with tapered noise
            heights[i] = contourHeight + taperedNoiseHeight;

            // Ensure reasonable bounds - allow for more height variation
            heights[i] = Math.Clamp(heights[i],
                Math.Min(leftEdgeHeight, rightEdgeHeight) - 100, // Allow deeper valleys
                Math.Max(leftEdgeHeight, rightEdgeHeight) + 60);  // Allow higher peaks
        }

        return heights;
    }

    private int GetContourHeight(float normalizedPosition, int baseHeight, int leftEdgeHeight, int rightEdgeHeight, float taperFactor)
    {
        // In the center, use base height
        if (taperFactor >= 0.8f) // Reduced threshold for more center area
        {
            return baseHeight;
        }

        // Near edges, blend towards edge heights more gradually
        float edgeInfluence = (1f - taperFactor) * 0.7f; // Reduce edge influence for smoother transitions

        if (normalizedPosition < 0.5f)
        {
            // Closer to left edge
            float leftInfluence = (0.5f - normalizedPosition) * 2f * edgeInfluence;
            return (int)(baseHeight * (1f - leftInfluence) + leftEdgeHeight * leftInfluence);
        }
        else
        {
            // Closer to right edge
            float rightInfluence = (normalizedPosition - 0.5f) * 2f * edgeInfluence;
            return (int)(baseHeight * (1f - rightInfluence) + rightEdgeHeight * rightInfluence);
        }
    }

    private float GetTaperFactor(float normalizedPosition, int width)
    {
        // Create taper zones at edges (20% of biome width or 100 tiles max)
        float taperZone = Math.Min(100f, width * 0.2f) / width;

        float distanceFromEdge;
        if (normalizedPosition < taperZone)
        {
            // Left edge taper
            distanceFromEdge = normalizedPosition / taperZone;
        }
        else if (normalizedPosition > 1f - taperZone)
        {
            // Right edge taper
            distanceFromEdge = (1f - normalizedPosition) / taperZone;
        }
        else
        {
            // Center area - full strength
            return 1f;
        }

        // Smooth cosine curve for natural tapering
        return (float)(0.5 * (1 + Math.Cos(Math.PI * (1 - distanceFromEdge))));
    }

    private void FillCanopyColumn(int x, int surfaceY)
    {
        int maxDepth = surfaceY + _config.CanopyDepth;

        for (int y = surfaceY; y < maxDepth && y < Main.maxTilesY; y++)
        {
            if (!IsValidY(y)) continue;

            PlaceTerrain(x, y, surfaceY);
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