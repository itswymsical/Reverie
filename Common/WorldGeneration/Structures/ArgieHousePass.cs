using Reverie.Content.NPCs.Special;
using System.Linq;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Structures;

public class ArgieHousePass : GenPass
{
    public ArgieHousePass() : base("[Reverie] Spore House", 30f) { }

    private const int STRUCTURE_WIDTH = 20;
    private const int STRUCTURE_HEIGHT = 34;

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "generating a spore house";

        var placement = FindPlacement();
        PlaceHouse(placement.X, placement.Y);
    }

    private static Point FindPlacement()
    {
        // Search the entire world for mushroom biomes
        for (var attempt = 0; attempt < 2000; attempt++)
        {
            var x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
            var y = WorldGen.genRand.Next(100, Main.maxTilesY - 200);

            if (IsInMushroomBiome(x, y))
            {
                var validPoint = FindValidMushroomSurface(x, y);
                if (validPoint.HasValue)
                {
                    return validPoint.Value;
                }
            }
        }

        // Fallback: search systematically if random search fails
        return FindMushroomBiomeSystematic() ?? new Point(Main.maxTilesX / 2, Main.maxTilesY / 2);
    }

    private static bool IsInMushroomBiome(int x, int y)
    {
        int mushroomTileCount = 0;
        int checkRadius = 25;

        // Check area around the point for mushroom biome tiles
        for (int i = x - checkRadius; i <= x + checkRadius; i++)
        {
            for (int j = y - checkRadius; j <= y + checkRadius; j++)
            {
                if (i < 0 || i >= Main.maxTilesX || j < 0 || j >= Main.maxTilesY)
                    continue;

                var tile = Main.tile[i, j];
                if (tile.HasTile)
                {
                    // Count mushroom grass, mud near mushroom areas, and mushroom blocks
                    if (tile.TileType == TileID.MushroomGrass ||
                        tile.TileType == TileID.MushroomBlock ||
                        (tile.TileType == TileID.Mud && HasNearbyMushroomTiles(i, j)))
                    {
                        mushroomTileCount++;
                    }
                }
            }
        }

        return mushroomTileCount >= 15; // Decent mushroom biome density
    }

    private static bool HasNearbyMushroomTiles(int x, int y)
    {
        for (int i = x - 3; i <= x + 3; i++)
        {
            for (int j = y - 3; j <= y + 3; j++)
            {
                if (i < 0 || i >= Main.maxTilesX || j < 0 || j >= Main.maxTilesY)
                    continue;

                var tile = Main.tile[i, j];
                if (tile.HasTile && tile.TileType == TileID.MushroomGrass)
                    return true;
            }
        }
        return false;
    }

    private static Point? FindMushroomBiomeSystematic()
    {
        // Systematic search as backup - cover full world depth
        for (int x = 100; x < Main.maxTilesX - 100; x += 50)
        {
            for (int y = 100; y < Main.rockLayer; y += 30)
            {
                if (IsInMushroomBiome(x, y))
                {
                    var validPoint = FindValidMushroomSurface(x, y);
                    if (validPoint.HasValue)
                        return validPoint.Value;
                }
            }
        }
        return null;
    }

    private static Point? FindValidMushroomSurface(int centerX, int centerY)
    {
        // Search in a small area around the mushroom biome center
        for (int xOffset = -20; xOffset <= 20; xOffset += 5)
        {
            int x = centerX + xOffset;

            if (x - STRUCTURE_WIDTH / 2 < 50 || x + STRUCTURE_WIDTH / 2 > Main.maxTilesX - 50)
                continue;

            // Find surface level near this X coordinate
            int surfaceY = FindSurfaceLevel(x, centerY);
            if (surfaceY > 0 && IsSurfaceValid(x, surfaceY))
            {
                return new Point(x, surfaceY);
            }
        }
        return null;
    }

    private static int FindSurfaceLevel(int x, int startY)
    {
        // Search up and down from starting point to find a good surface
        // For underground areas, look for cavern floors
        for (int yOffset = -50; yOffset <= 50; yOffset++)
        {
            int y = startY + yOffset;
            if (y < 10 || y >= Main.maxTilesY - 10)
                continue;

            // Look for transition from air to solid ground (cavern floor)
            if (!WorldGen.SolidTile(x, y - 1) && WorldGen.SolidTile(x, y))
            {
                // Check if there's enough air space above for the structure
                bool hasSpace = true;
                for (int checkY = y - STRUCTURE_HEIGHT; checkY < y; checkY++)
                {
                    if (WorldGen.SolidTile(x, checkY))
                    {
                        hasSpace = false;
                        break;
                    }
                }

                if (hasSpace)
                    return y;
            }
        }
        return -1;
    }

    private static bool IsSurfaceValid(int x, int y)
    {
        var groundLevels = new int[STRUCTURE_WIDTH + 1];

        for (var i = 0; i <= STRUCTURE_WIDTH; i++)
        {
            int groundY = y;
            // Search downward to find solid ground
            while (groundY < y + STRUCTURE_HEIGHT + 10 && !WorldGen.SolidTile(x - STRUCTURE_WIDTH / 2 + i, groundY))
            {
                groundY++;
            }

            if (groundY >= y + STRUCTURE_HEIGHT + 10)
                return false;

            groundLevels[i] = groundY;
        }

        // Check ground variation
        int minGround = groundLevels.Min();
        int maxGround = groundLevels.Max();
        if (maxGround - minGround > 5)
            return false;

        // Check for clear space above ground
        for (var i = 0; i <= STRUCTURE_WIDTH; i++)
        {
            int columnX = x - STRUCTURE_WIDTH / 2 + i;
            int groundY = groundLevels[i];

            for (var j = groundY - STRUCTURE_HEIGHT; j < groundY; j++)
            {
                if (WorldGen.SolidTile(columnX, j))
                    return false;
            }
        }

        // Verify solid foundation
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

    private static void PlaceHouse(int x, int y)
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

        StructureHelper.API.Generator.GenerateStructure("Structures/ArgieHouse", new Point16(structureX, structureY + 4), Instance);

        int spawnX = structureX - 88;
        int spawnColumnIndex = Math.Max(0, Math.Min(STRUCTURE_WIDTH, 48));
        int spawnGroundLevel = groundLevels[spawnColumnIndex];
        int spawnY = spawnGroundLevel - 3;
        while (spawnY > structureY && WorldGen.SolidTile(spawnX, spawnY))
        {
            spawnY--;
        }

        GetNPCs(structureX, structureY);
    }

    private static void GetNPCs(int structureX, int y)
    {
        var argieX = structureX + 10;
        var argieY = y + 20;            
        PlaceNPC(ModContent.NPCType<Argie>(), argieX, argieY, -1);
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