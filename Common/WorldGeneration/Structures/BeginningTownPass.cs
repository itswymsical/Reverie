using System.Linq;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Structures;

public class BeginningTownPass : GenPass
{
    public BeginningTownPass() : base("[Reverie] Beginning Town", 247.5f) { }

    private const int STRUCTURE_WIDTH = 108;
    private const int STRUCTURE_HEIGHT = 42;

    private const int SURFACE_CHECK_HEIGHT = 22;

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Constructing Town";

        var placement = FindPlacement();
        PlaceTown(placement.X, placement.Y + 8);
    }

    private static Point FindPlacement()
    {
        var worldCenter = Main.maxTilesX / 2;

        for (var attempt = 0; attempt < 1000; attempt++)
        {
            var x = worldCenter + WorldGen.genRand.Next(150, 200);
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
        // First, find the actual ground level for each column
        var groundLevels = new int[STRUCTURE_WIDTH + 1];

        for (var i = 0; i <= STRUCTURE_WIDTH; i++)
        {
            int groundY = y;
            // Search downward to find the first solid ground
            while (groundY < y + STRUCTURE_HEIGHT + 10 && !WorldGen.SolidTile(x - STRUCTURE_WIDTH / 2 + i, groundY))
            {
                groundY++;
            }

            // If we went too far down or didn't find ground, this location isn't valid
            if (groundY >= y + STRUCTURE_HEIGHT + 10)
                return false;

            groundLevels[i] = groundY;
        }

        // Check if the ground variation is acceptable (not too steep)
        int minGround = groundLevels.Min();
        int maxGround = groundLevels.Max();
        if (maxGround - minGround > 5) // Allow a maximum of 5 tile variation
            return false;

        // Now check that there's enough empty space above the highest ground level
        for (var i = 0; i <= STRUCTURE_WIDTH; i++)
        {
            int columnX = x - STRUCTURE_WIDTH / 2 + i;
            int groundY = groundLevels[i];

            // Check for empty space above the ground
            for (var j = groundY - STRUCTURE_HEIGHT; j < groundY; j++)
            {
                if (WorldGen.SolidTile(columnX, j))
                    return false;
            }
        }

        // Check that we have enough solid ground beneath
        int solidGroundCount = 0;
        for (var i = 0; i <= STRUCTURE_WIDTH; i++)
        {
            int columnX = x - STRUCTURE_WIDTH / 2 + i;
            int groundY = groundLevels[i];

            for (var j = groundY; j < groundY + 5; j++)
            {
                if (WorldGen.SolidTile(columnX, j))
                    solidGroundCount++;
            }
        }

        return solidGroundCount >= STRUCTURE_WIDTH * 3;
    }

    private static void PlaceTown(int x, int y)
    {
        var groundLevels = new int[STRUCTURE_WIDTH + 1];
        for (var i = 0; i <= STRUCTURE_WIDTH; i++)
        {
            int groundY = y;
            while (groundY < y + STRUCTURE_HEIGHT + 10 && !WorldGen.SolidTile(x - STRUCTURE_WIDTH / 2 + i, groundY))
            {
                groundY++;
            }
            groundLevels[i] = groundY;
        }

        int minGroundLevel = groundLevels.Min();

        var structureX = x - STRUCTURE_WIDTH / 2;
        var structureY = minGroundLevel - STRUCTURE_HEIGHT;

        // Optional: You can push it even deeper by adding a constant value
        // e.g., structureY += 3; // Push 3 tiles deeper into the ground

        StructureHelper.API.Generator.GenerateStructure("Structures/BeginningTown", new Point16(structureX, structureY + 4), Instance);

        int spawnX = structureX - 88;
        int spawnColumnIndex = Math.Max(0, Math.Min(STRUCTURE_WIDTH, 48));
        int spawnGroundLevel = groundLevels[spawnColumnIndex];
        int spawnY = spawnGroundLevel - 3;
        while (spawnY > structureY && WorldGen.SolidTile(spawnX, spawnY))
        {
            spawnY--;
        }
        Main.spawnTileX = spawnX;
        Main.spawnTileY = spawnY;

        GetNPCs(structureX, structureY);
    }

    private static void GetNPCs(int structureX, int y)
    {
        // adding to Y moves them down

        var guideX = structureX + 60;
        var guideY = y + 33;
        PlaceNPC(NPCID.Guide, guideX, guideY, -1);

        var merchantX = structureX + 94;
        var merchantY = y + 19;
        PlaceNPC(NPCID.Merchant, merchantX, merchantY, -1);

        var demoX = structureX + 94;
        var demoY = y + 27;
        PlaceNPC(NPCID.Demolitionist, demoX, demoY, -1);

        var nurseX = structureX + 35;
        var nurseY = y + 24;
        PlaceNPC(NPCID.Nurse, nurseX, nurseY, -1);
    }

    private static void PlaceNPC(int npcType, int tileX, int tileY, int direction)
    {
        var entity = NPC.NewNPC(
            new EntitySource_WorldGen(),
            tileX * 16,
            tileY * 16,
            npcType
        );

        if (entity >= 0 && entity < Main.maxNPCs)
        {
            Main.npc[entity].homeTileX = tileX;
            Main.npc[entity].homeTileY = tileY;
            Main.npc[entity].direction = direction;
            Main.npc[entity].homeless = false;
        }
    }
}