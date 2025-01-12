using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using StructureHelper;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Systems.WorldGeneration.GenPasses
{
    public class GuideShelterPass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        private const int STRUCTURE_WIDTH = 66;
        private const int STRUCTURE_HEIGHT = 46;
        private const int SPAWN_OFFSET_X = 26;
        private const int SPAWN_OFFSET_Y = 22;
        private const int GUIDE_OFFSET_X = 35;
        private const int GUIDE_OFFSET_Y = 40;
        private const int SURFACE_CHECK_HEIGHT = 30; // How far up from worldSurface to check

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Constructing Shelter";

            Point placement = FindPlacement();
            //FlattenArea(placement.X, placement.Y);
            PlaceGuideShelter(placement.X, placement.Y); // Now using direct coordinates since Generator uses top-left origin
        }

        private Point FindPlacement()
        {
            int worldCenter = Main.maxTilesX / 2;

            for (int attempt = 0; attempt < 1000; attempt++)
            {
                int x = worldCenter + WorldGen.genRand.Next(-200, 201);
                // Start checking from higher up
                int startY = (int)Main.spawnTileY - SURFACE_CHECK_HEIGHT;

                Point? validPoint = FindValidSurface(x, startY);
                if (validPoint.HasValue)
                {
                    return validPoint.Value;
                }
            }

            // Fallback position
            return new Point(worldCenter, (int)Main.spawnTileY - SURFACE_CHECK_HEIGHT);
        }

        private Point? FindValidSurface(int x, int startY)
        {
            if (x - STRUCTURE_WIDTH / 2 < 50 || x + STRUCTURE_WIDTH / 2 > Main.maxTilesX - 50)
                return null;

            // Scan downward from our starting height
            for (int y = startY; y < startY + SURFACE_CHECK_HEIGHT * 2; y++)
            {
                if (IsSurfaceValid(x, y))
                {
                    return new Point(x, y);
                }
            }

            return null;
        }

        private bool IsSurfaceValid(int x, int y)
        {
            // Check if there's open space for the structure
            for (int i = x - STRUCTURE_WIDTH / 2; i <= x + STRUCTURE_WIDTH / 2; i++)
            {
                // Check the row where we'll place the structure
                for (int j = y; j < y + STRUCTURE_HEIGHT; j++)
                {
                    if (WorldGen.SolidTile(i, j))
                        return false;
                }

                // Check for solid ground below the structure
                if (!WorldGen.SolidTile(i, y + STRUCTURE_HEIGHT))
                    return false;
            }

            // Make sure we have enough solid ground below
            int solidGroundCount = 0;
            for (int i = x - STRUCTURE_WIDTH / 2; i <= x + STRUCTURE_WIDTH / 2; i++)
            {
                for (int j = y + STRUCTURE_HEIGHT; j < y + STRUCTURE_HEIGHT + 5; j++)
                {
                    if (WorldGen.SolidTile(i, j))
                        solidGroundCount++;
                }
            }

            return solidGroundCount >= (STRUCTURE_WIDTH * 3); // Ensure we have a good foundation
        }

        private void FlattenArea(int x, int y)
        {
            int startX = x - STRUCTURE_WIDTH / 6;
            int endX = x + STRUCTURE_WIDTH / 3;

            // Clear area for structure
            //for (int i = startX - 5; i <= endX + 5; i++)
            //{
            //    for (int j = y - 5; j <= y + STRUCTURE_HEIGHT + 8; j++)
            //    {
            //        WorldGen.KillTile(i, j);
            //        WorldGen.KillWall(i, j);

            //        // Create solid foundation below structure
            //        if (j >= y + STRUCTURE_HEIGHT)
            //        {
            //            WorldGen.PlaceTile(i, j, TileID.Dirt, forced: true);
            //        }
            //    }
            //}
        }

        private void PlaceGuideShelter(int x, int y)
        {
            int structureX = x - STRUCTURE_WIDTH / 2;
            Generator.GenerateStructure("Structures/GuideShelter", new Point16(structureX, y), Reverie.Instance);

            Main.spawnTileX = structureX + SPAWN_OFFSET_X;
            Main.spawnTileY = y + SPAWN_OFFSET_Y;

            int guideX = structureX + GUIDE_OFFSET_X;
            int guideY = y + GUIDE_OFFSET_Y;

            int guide = NPC.NewNPC(
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
}