using Microsoft.Xna.Framework;
using Reverie.Utilities;
using StructureHelper;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using static Reverie.Common.Systems.WorldGeneration.WoodlandCanopy.CanopyGeneration;

namespace Reverie.Common.Systems.WorldGeneration.WoodlandCanopy
{
    public class ReverieTreePass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Surging energy through a Living Tree";
            trunkX = Math.Clamp(trunkX, 0, Main.maxTilesX - 1);

            int structureWidth = 104;
            int structureHeight = 170;
            int searchRadius = 40;

            for (int x = spawnX; x < spawnX + searchRadius; x++)
            {
                for (int y = 0; y < spawnY + 35; y++)
                {
                    if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == TileID.Grass)
                    {
                        // Found a grass tile, check if there's enough space
                        if (IsAreaClear(x, y, structureWidth, structureHeight))
                        {
                            // Calculate the position to place the structure
                            int treePos = y - structureHeight + 20; // Adjust this offset as needed
                            int structureX = x - 58; // Center the structure horizontally
                            Generator.GenerateStructure("Structures/ReverieTreeStruct", new Point16(structureX, treePos), Reverie.Instance);
                            GrowTrunkChasm(x);
                            return;
                        }
                    }
                }
            }       
        }

        private static bool IsAreaClear(int x, int y, int width, int height)
        {
            for (int i = x - width / 2; i < x + width / 2; i++)
            {
                for (int j = y - height; j < y; j++)
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
            int trunkWidth = 23;
            int treePos = (int)Main.worldSurface - 224;
            const float TRUNK_CURVE_FREQUENCY = 0.0765f;
            const int TRUNK_CURVE_AMPLITUDE = 4;

            for (int y = Main.spawnTileY + 14; y < trunkBottom; y++)
            {
                int currentTrunkWidth = trunkWidth + (y % 5 == 0 ? 2 : 0);
                int curveOffset = (int)(Math.Sin(y * TRUNK_CURVE_FREQUENCY) * TRUNK_CURVE_AMPLITUDE);

                int leftBound = i - currentTrunkWidth / 2 + curveOffset;
                int rightBound = i + currentTrunkWidth / 2 + curveOffset;

                for (int x = leftBound; x <= rightBound; x++)
                {
                    WorldGen.KillWall(x, y);
                    WorldGen.PlaceTile(x, y, treeWood, mute: true, forced: true);

                }
            }
            for (int y2 = Main.spawnTileY + 14; y2 < trunkBottom; y2++)
            {
                int tunnelTrunkWidth = trunkWidth / 2 + (y2 % 5 == 0 ? 2 : 0);
                int tunnelOffset = (int)(Math.Sin(y2 * TRUNK_CURVE_FREQUENCY) * TRUNK_CURVE_AMPLITUDE);
                int leftBound = i - tunnelTrunkWidth / 2 + tunnelOffset;
                int rightBound = i + tunnelTrunkWidth / 2 + tunnelOffset;
                for (int x2 = leftBound; x2 <= rightBound; x2++)
                {
                    WorldGen.KillTile(x2, y2);
                    WorldGen.PlaceWall(x2, y2, treeWall);
                }
            }
        }
    }
}