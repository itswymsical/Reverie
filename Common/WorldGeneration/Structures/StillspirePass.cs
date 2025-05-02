using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Structures;
public class StillspirePass : GenPass
{
    public StillspirePass() : base("Reverie: Stillspire", 247.5f) { }

    private const int STRUCTURE_WIDTH = 89;
    private const int STRUCTURE_HEIGHT = 143;
    // How far below the absolute surface Y to start scanning for ground
    private const int SURFACE_SCAN_DEPTH = 50;
    // How far away from the biome edge to try placing the structure
    private const int MIN_EDGE_OFFSET = 10;
    private const int MAX_EDGE_OFFSET = 30;
    // Max attempts to find a valid placement spot per biome edge
    private const int MAX_PLACEMENT_ATTEMPTS_PER_EDGE = 500;
    // How much vertical variation is allowed in the ground across the structure's width
    private const int MAX_GROUND_VARIATION = 5;
    // Minimum number of solid tiles required under the structure base
    private const int MIN_SOLID_GROUND_TILES = STRUCTURE_WIDTH * 3; // Require at least 3 deep solid ground generally

    // Main method for the generation pass
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Whispering winds shape the land...";

        // Attempt to find a placement location
        Point? placement = FindPlacement();

        // If a valid placement was found, place the structure
        if (placement.HasValue)
        {
            // The PlaceSpire method expects the *center* X and the *target ground* Y
            PlaceSpire(placement.Value.X, placement.Value.Y);
        }
        else
        {
        }
    }

    /// <summary>
    /// Attempts to find a valid Point (X, Y) to place the center of the structure near a Corruption/Crimson biome edge.
    /// </summary>
    /// <returns>A Point representing the center X and target ground Y, or null if no suitable location is found.</returns>
    private Point? FindPlacement()
    {
        var random = WorldGen.genRand;
        var corruptionBiomes = FindCorruptionBiomes(); // Find all evil biome horizontal spans

        // Check if any corruption biomes were found
        if (corruptionBiomes.Count == 0)
        {
            // ModContent.GetInstance<YourMod>().Logger.Warn("No Corruption or Crimson biomes detected during Stillspire placement scan.");
            return null; // Or return a default fallback position if desired
                         // return new Point(Main.maxTilesX / 2, (int)Main.worldSurface - 10);
        }

        // Create a list of potential edges (left and right for each biome)
        var potentialEdges = new List<(int x, bool isLeftEdge)>();
        foreach (var biome in corruptionBiomes)
        {
            potentialEdges.Add((biome.Left, true));  // Left edge
            potentialEdges.Add((biome.Right, false)); // Right edge
        }

        // Shuffle the list of edges to try them in a random order
        potentialEdges = [.. potentialEdges.OrderBy(e => random.Next())];

        // Try each edge until a valid spot is found
        foreach (var (edgeX, isLeftEdge) in potentialEdges)
        {
            for (var attempt = 0; attempt < MAX_PLACEMENT_ATTEMPTS_PER_EDGE; attempt++)
            {
                // Calculate a target X slightly outside the biome edge
                int offset = random.Next(MIN_EDGE_OFFSET, MAX_EDGE_OFFSET + 1);
                int targetX = isLeftEdge ? edgeX - offset : edgeX + offset;

                // Clamp targetX to be within safe world bounds, considering structure width
                targetX = Math.Clamp(targetX, STRUCTURE_WIDTH / 2 + 10, Main.maxTilesX - STRUCTURE_WIDTH / 2 - 10);

                // Define the vertical range to search for a valid surface
                // Start slightly above the general world surface and scan down
                int startY = (int)GenVars.worldSurfaceLow - 10; // Start a bit above the lowest possible surface
                int endY = (int)Main.worldSurface + SURFACE_SCAN_DEPTH; // Scan down to a reasonable depth

                // Attempt to find a valid Y coordinate (ground level) at this targetX
                Point? validPoint = FindValidSurfacePoint(targetX, startY, endY);
                if (validPoint.HasValue)
                {
                    return validPoint.Value; // Found a spot!
                }
            }
        }

        // If no spot was found after checking all edges and attempts
        // ModContent.GetInstance<YourMod>().Logger.Warn($"Failed to find valid placement near any biome edge after {potentialEdges.Count * MAX_PLACEMENT_ATTEMPTS_PER_EDGE} attempts.");
        return null; // Fallback: No suitable location found
    }

    /// <summary>
    /// Scans the world horizontally near the surface to find Corruption and Crimson biome regions.
    /// </summary>
    /// <returns>A list of Rectangles, where each Rectangle's X and Width define the horizontal span of a biome.</returns>
    private List<Rectangle> FindCorruptionBiomes()
    {
        var biomes = new List<Rectangle>();
        // Define the set of tiles that indicate an evil biome (Corruption or Crimson)
        var evilTiles = new HashSet<int> {
                // Corruption
                TileID.Ebonstone, TileID.CorruptGrass, TileID.Ebonsand, TileID.CorruptIce,
                TileID.CorruptHardenedSand, TileID.CorruptSandstone, TileID.CorruptThorns,
                // Crimson
                TileID.Crimstone, TileID.CrimsonGrass, TileID.Crimsand, TileID.FleshIce,
                TileID.CrimsonHardenedSand, TileID.CrimsonSandstone, TileID.CrimsonThorns
                // Add other relevant tiles if needed (e.g., Demon Altars, Crimson Altars if desired)
            };

        bool inBiome = false;
        int biomeStartX = 0;
        int scanStartY = (int)GenVars.worldSurfaceLow - 10; // Start scan slightly above lowest surface
        int scanEndY = (int)Main.worldSurface + SURFACE_SCAN_DEPTH; // Scan down a bit

        // Scan horizontally across the world, avoiding the extreme edges
        for (int x = 10; x < Main.maxTilesX - 10; x++)
        {
            bool foundEvilInColumn = false;
            // Scan vertically in the current column to check for any evil tile
            for (int y = scanStartY; y < scanEndY; y++)
            {
                // Ensure y is within world bounds
                if (y < 0 || y >= Main.maxTilesY) continue;

                Tile tile = Main.tile[x, y];
                // Check if the tile exists and is one of the defined evil tiles
                if (tile != null && tile.HasTile && evilTiles.Contains(tile.TileType))
                {
                    foundEvilInColumn = true;
                    break; // Found an evil tile in this column, no need to check further down
                }
            }

            // State machine to track biome start and end
            if (!inBiome && foundEvilInColumn)
            {
                // Entering a new biome region
                inBiome = true;
                biomeStartX = x;
            }
            else if (inBiome && !foundEvilInColumn)
            {
                // Exiting a biome region
                inBiome = false;
                int biomeWidth = x - biomeStartX;
                // Only register the biome if it's reasonably wide
                if (biomeWidth > 15) // Minimum width threshold
                {
                    // Add the detected biome span. Y and Height are arbitrary here, only X and Width matter.
                    biomes.Add(new Rectangle(biomeStartX, scanStartY, biomeWidth, 1));
                }
            }
        }

        // Handle the case where a biome extends to the edge of the scanned area
        if (inBiome)
        {
            int biomeWidth = Main.maxTilesX - 10 - biomeStartX;
            if (biomeWidth > 15)
            {
                biomes.Add(new Rectangle(biomeStartX, scanStartY, biomeWidth, 1));
            }
        }

        return biomes;
    }

    /// <summary>
    /// Searches vertically at a given X-coordinate to find the highest solid ground suitable for placing the structure.
    /// </summary>
    /// <param name="x">The central X-coordinate to check.</param>
    /// <param name="startY">The Y-coordinate to start scanning downwards from.</param>
    /// <param name="endY">The Y-coordinate to stop scanning downwards.</param>
    /// <returns>A Point (x, y) where y is the ground level, or null if no valid surface is found.</returns>
    private Point? FindValidSurfacePoint(int x, int startY, int endY)
    {
        // Iterate downwards from startY
        for (int y = startY; y < endY; y++)
        {
            // Check if the surface at (x, y) is suitable for the structure
            if (IsSurfaceValidForStructure(x, y))
            {
                // Found a valid ground level
                return new Point(x, y);
            }
        }
        // No valid surface found in the specified range
        return null;
    }

    /// <summary>
    /// Checks if the ground surface centered at (x, y) is suitable for the structure.
    /// Considers flatness, clearance above, and solid ground below.
    /// </summary>
    /// <param name="centerX">The potential center X-coordinate of the structure.</param>
    /// <param name="targetSurfaceY">The potential Y-coordinate of the ground surface directly below the center.</param>
    /// <returns>True if the surface is valid, false otherwise.</returns>
    private bool IsSurfaceValidForStructure(int centerX, int targetSurfaceY)
    {
        // --- Basic Boundary Checks ---
        int structureStartX = centerX - STRUCTURE_WIDTH / 2;
        int structureEndX = centerX + STRUCTURE_WIDTH / 2;
        if (structureStartX < 10 || structureEndX > Main.maxTilesX - 10 || targetSurfaceY < 50 || targetSurfaceY > Main.maxTilesY - STRUCTURE_HEIGHT - 20)
        {
            return false; // Too close to world edges or too high/low
        }

        // --- Determine Actual Ground Levels Across Structure Width ---
        var groundLevels = new int[STRUCTURE_WIDTH + 1];
        bool groundFoundEverywhere = true;

        for (var i = 0; i <= STRUCTURE_WIDTH; i++)
        {
            int currentX = structureStartX + i;
            int groundY = targetSurfaceY; // Start searching from the target Y

            // Search downwards from targetSurfaceY to find the first solid tile
            int searchAttempts = 0;
            while (searchAttempts < SURFACE_SCAN_DEPTH && // Limit search depth
                   (groundY >= Main.maxTilesY || !WorldGen.SolidTile(currentX, groundY) && !Main.tile[currentX, groundY].HasTile && Main.tile[currentX, groundY].LiquidType == LiquidID.Water)) // Allow placing on water surface if needed, treat as non-solid for finding ground
            {
                // Allow solid tiles OR tiles that are not solid but have liquid (like water surface)
                // Need to refine this if placing *on* water isn't desired.
                // Let's simplify: just find the first solid tile or active tile.
                groundY++;
                searchAttempts++;

                // Check if the tile is solid OR just active (like platforms)
                if (groundY < Main.maxTilesY && (WorldGen.SolidTile(currentX, groundY) || Main.tile[currentX, groundY].HasTile))
                {
                    break; // Found the ground for this column
                }
            }


            // If we scanned too deep without finding solid ground, invalidate this spot
            if (searchAttempts >= SURFACE_SCAN_DEPTH || groundY >= Main.maxTilesY)
            {
                groundFoundEverywhere = false;
                break; // No point checking further columns
            }

            groundLevels[i] = groundY;
        }

        if (!groundFoundEverywhere)
        {
            return false; // Failed to find ground under the entire structure width
        }

        // --- Check Ground Flatness ---
        int minGround = groundLevels.Min();
        int maxGround = groundLevels.Max();
        if (maxGround - minGround > MAX_GROUND_VARIATION)
        {
            return false; // Ground is too uneven
        }

        // --- Check Clearance Above Ground ---
        // Use the *lowest* ground point (minGround) as the reference for placing the structure base.
        int structureTopY = minGround - STRUCTURE_HEIGHT;
        for (var i = 0; i <= STRUCTURE_WIDTH; i++)
        {
            int currentX = structureStartX + i;
            // Check for obstructions in the space the structure will occupy
            for (int y = structureTopY; y < minGround; y++) // Check up to (but not including) the ground level
            {
                if (y < 0) continue; // Skip checks above the world
                if (y >= Main.maxTilesY) return false; // Should not happen if initial checks pass

                Tile tile = Main.tile[currentX, y];
                if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType]) // Check for solid tiles
                {
                    // Optionally allow specific non-solid tiles if needed
                    return false; // Obstruction found
                }
                // Consider checking for liquids too if structure shouldn't be in liquid
            }
        }

        // --- Check Solid Ground Below ---
        int solidGroundCount = 0;
        for (var i = 0; i <= STRUCTURE_WIDTH; i++)
        {
            int currentX = structureStartX + i;
            int groundY = groundLevels[i]; // Use the specific ground level for this column

            // Check a few tiles directly below the determined ground level
            for (var j = 0; j < 5; j++) // Check 5 tiles deep
            {
                int checkY = groundY + j;
                if (checkY >= Main.maxTilesY) break; // Stop if we go below the world

                Tile tile = Main.tile[currentX, checkY];
                if (tile != null && tile.HasTile && WorldGen.SolidTile(tile)) // Check specifically for solid tiles
                {
                    solidGroundCount++;
                }
            }
        }

        // Check if enough solid ground exists beneath the structure footprint
        if (solidGroundCount < MIN_SOLID_GROUND_TILES)
        {
            return false; // Not enough foundational support
        }

        // If all checks passed, the surface is valid
        return true;
    }


    /// <summary>
    /// Places the Stillspire structure using StructureHelper.
    /// </summary>
    /// <param name="centerX">The X-coordinate for the center of the structure.</param>
    /// <param name="groundY">The Y-coordinate of the ground level where the structure should be placed.</param>
    private void PlaceSpire(int centerX, int groundY)
    {
        // --- Recalculate the minimum ground level across the structure width ---
        // This ensures the structure base sits correctly even on slightly uneven ground found by IsSurfaceValidForStructure.
        var groundLevels = new int[STRUCTURE_WIDTH + 1];
        int structureStartX = centerX - STRUCTURE_WIDTH / 2;
        for (var i = 0; i <= STRUCTURE_WIDTH; i++)
        {
            int currentX = structureStartX + i;
            int currentGroundY = groundY; // Start at the initially found ground level

            // Scan down *again* briefly to confirm the topmost solid tile,
            // in case the initial scan stopped slightly above it.
            int searchAttempts = 0;
            while (searchAttempts < 10 && currentGroundY < Main.maxTilesY - 1)
            {
                if (WorldGen.SolidTile(currentX, currentGroundY) || Main.tile[currentX, currentGroundY].HasTile) break;
                currentGroundY++;
                searchAttempts++;
            }
            // If still no ground found here (shouldn't happen if IsValid passed), use original groundY
            groundLevels[i] = (searchAttempts < 10) ? currentGroundY : groundY;
        }
        int minGroundLevel = groundLevels.Min(); // Find the lowest point on the ground across the width

        // --- Calculate Final Placement Coordinates ---
        // StructureHelper places based on the top-left corner.
        int placementX = centerX - STRUCTURE_WIDTH / 2;
        // Place the structure so its bottom aligns with the lowest ground point found.
        int placementY = minGroundLevel - STRUCTURE_HEIGHT;

        // --- Generate Structure ---
        // The Point16 should be the top-left corner.
        // The "+ 4" offset was in the original code, its purpose is unclear without seeing the structure file.
        // It might be adjusting for empty space at the bottom of the structure file or a desired visual offset.
        // Keep it for now, but consider reviewing the structure's origin point.
        int finalPlacementY = placementY + 4;

        Point16 topLeftPlacementPoint = new Point16(placementX, finalPlacementY);

        StructureHelper.API.Generator.GenerateStructure("Structures/Stillspire", topLeftPlacementPoint, Instance);
    }
}
