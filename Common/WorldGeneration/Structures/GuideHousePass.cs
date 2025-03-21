using Terraria.DataStructures;
using StructureHelper;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Structures;

public class GuideHousePass(string name, float loadWeight) : GenPass(name, loadWeight)
{
    private const int STRUCTURE_WIDTH = 44;
    private const int STRUCTURE_HEIGHT = 24;
    private const int SPAWN_OFFSET_X = -10;
    private const int SPAWN_OFFSET_Y = 22;
    private const int GUIDE_OFFSET_X = 24;
    private const int GUIDE_OFFSET_Y = 20;
    private const int SURFACE_CHECK_HEIGHT = 22;

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Constructing Shelter";

        var placement = FindPlacement();
        PlaceGuideShelter(placement.X, placement.Y);
    }

    private static Point FindPlacement()
    {
        var worldCenter = Main.maxTilesX / 2;

        for (var attempt = 0; attempt < 1000; attempt++)
        {
            var x = worldCenter + WorldGen.genRand.Next(-200, 201);
            var startY = Main.spawnTileY - SURFACE_CHECK_HEIGHT;

            var validPoint = FindValidSurface(x, startY);
            if (validPoint.HasValue)
            {
                return validPoint.Value;
            }
        }

        // Fallback position
        return new Point(worldCenter, Main.spawnTileY - SURFACE_CHECK_HEIGHT);
    }

    private static Point? FindValidSurface(int x, int startY)
    {
        if (x - STRUCTURE_WIDTH / 2 < 50 || x + STRUCTURE_WIDTH / 2 > Main.maxTilesX - 50)
            return null;

        // Scan downward from our starting height
        for (var y = startY; y < startY + SURFACE_CHECK_HEIGHT * 2; y++)
        {
            if (IsSurfaceValid(x, y))
            {
                return new Point(x, y);
            }
        }

        return null;
    }

    private static bool IsSurfaceValid(int x, int y)
    {
        // Check if there's open space for the structure
        for (var i = x - STRUCTURE_WIDTH / 2; i <= x + STRUCTURE_WIDTH / 2; i++)
        {
            // Check the row where we'll place the structure
            for (var j = y; j < y + STRUCTURE_HEIGHT; j++)
            {
                if (WorldGen.SolidTile(i, j))
                    return false;
            }

            // Check for solid ground below the structure
            if (!WorldGen.SolidTile(i, y + STRUCTURE_HEIGHT))
                return false;
        }

        // Make sure we have enough solid ground below
        var solidGroundCount = 0;
        for (var i = x - STRUCTURE_WIDTH / 2; i <= x + STRUCTURE_WIDTH / 2; i++)
        {
            for (var j = y + STRUCTURE_HEIGHT; j < y + STRUCTURE_HEIGHT + 5; j++)
            {
                if (WorldGen.SolidTile(i, j))
                    solidGroundCount++;
            }
        }

        return solidGroundCount >= STRUCTURE_WIDTH * 3;
    }

    private static void PlaceGuideShelter(int x, int y)
    {
        var structureX = x - STRUCTURE_WIDTH / 2;
         Generator.GenerateStructure("Structures/LainesHouse", new Point16(structureX, y), Reverie.Instance);

        Main.spawnTileX = structureX + SPAWN_OFFSET_X;
        Main.spawnTileY = y + SPAWN_OFFSET_Y;

        var guideX = structureX + GUIDE_OFFSET_X;
        var guideY = y + GUIDE_OFFSET_Y;

        var guide = NPC.NewNPC(
            new EntitySource_WorldGen(),
            guideX * 16,
            guideY * 16,
            NPCID.Guide
        );

        if (guide >= 0 && guide < Main.maxNPCs)
        {
            Main.npc[guide].homeTileX = guideX;
            Main.npc[guide].homeTileY = guideY;
            Main.npc[guide].direction = -1;
            Main.npc[guide].homeless = false;
        }
    }
}