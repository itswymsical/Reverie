using StructureHelper;

using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;
using static Reverie.Common.Subworlds.Sylvanwalde.Generation.WoodlandCanopy.CanopyGeneration;

namespace Reverie.Common.Subworlds.Sylvanwalde.Generation.WoodlandCanopy
{
    public class ReverieTreePass: GenPass
    {
        public ReverieTreePass() : base("[Sylvan] Reverie Tree", 146.53f)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Surging energy through a Living Tree";
            trunkX = Math.Clamp(trunkX, 0, Main.maxTilesX - 1);

            var structureWidth = 104;
            var structureHeight = 170;
            var searchRadius = 40;

            for (var x = spawnX; x < spawnX + searchRadius; x++)
            {
                for (var y = 0; y < spawnY + 35; y++)
                {
                    if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == TileID.Grass)
                    {
                        // Found a grass tile, check if there's enough space
                        if (IsAreaClear(x, y, structureWidth, structureHeight))
                        {
                            // Calculate the position to place the structure
                            var treePos = y - structureHeight + 20; // Adjust this offset as needed
                            var structureX = x - 58; // Center the structure horizontally
                             Generator.GenerateStructure("Structures/ReverieTreeStruct", new Point16(structureX, treePos), Instance);
                            GrowTrunkChasm(x);
                            return;
                        }
                    }
                }
            }
        }

        private static bool IsAreaClear(int x, int y, int width, int height)
        {
            for (var i = x - width / 2; i < x + width / 2; i++)
            {
                for (var j = y - height; j < y; j++)
                {
                    if (i < 0 || i >= Main.maxTilesX || j < 0 || j >= Main.maxTilesY)
                    {
                        return false; // Out of world bounds
                    }

                    //if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType != TileID.LivingWood 
                    //    && Main.tile[i, j].TileType != TileID.LivingWood && Main.tile[i, j].TileType != TileID.LeafBlock)
                    //{
                    //    return false; // Area is not clear
                    //}
                }
            }
            return true; // Area is clear
        }

        public static void GrowTrunkChasm(int i)
        {
            var trunkWidth = 23;
            var treePos = (int)Main.worldSurface - 224;
            const float TRUNK_CURVE_FREQUENCY = 0.0765f;
            const int TRUNK_CURVE_AMPLITUDE = 4;

            for (var y = Main.spawnTileY + 14; y < trunkBottom; y++)
            {
                var currentTrunkWidth = trunkWidth + (y % 5 == 0 ? 2 : 0);
                var curveOffset = (int)(Math.Sin(y * TRUNK_CURVE_FREQUENCY) * TRUNK_CURVE_AMPLITUDE);

                var leftBound = i - currentTrunkWidth / 2 + curveOffset;
                var rightBound = i + currentTrunkWidth / 2 + curveOffset;

                for (var x = leftBound; x <= rightBound; x++)
                {
                    WorldGen.KillWall(x, y);
                    WorldGen.PlaceTile(x, y, treeWood, mute: true, forced: true);

                }
            }
            for (var y2 = Main.spawnTileY + 14; y2 < trunkBottom; y2++)
            {
                var tunnelTrunkWidth = trunkWidth / 2 + (y2 % 5 == 0 ? 2 : 0);
                var tunnelOffset = (int)(Math.Sin(y2 * TRUNK_CURVE_FREQUENCY) * TRUNK_CURVE_AMPLITUDE);
                var leftBound = i - tunnelTrunkWidth / 2 + tunnelOffset;
                var rightBound = i + tunnelTrunkWidth / 2 + tunnelOffset;
                for (var x2 = leftBound; x2 <= rightBound; x2++)
                {
                    WorldGen.KillTile(x2, y2);
                    WorldGen.PlaceWall(x2, y2, treeWall);
                }
            }
        }
    }
}