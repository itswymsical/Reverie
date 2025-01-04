using StructureHelper;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Systems.WorldGeneration.WoodlandCanopy
{
    public class StarterHousePass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Constructing a Home";
            int structureWidth = 66;
            int structureHeight = 46;
            int searchRadius = 30;

            for (int x = Main.spawnTileX - searchRadius; x < Main.spawnTileX + searchRadius; x++)
            {
                for (int y = 0; y < Main.spawnTileY - 20; y++)
                {
                    if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == TileID.Grass)
                    {
                        if (IsAreaClear(x, y, structureWidth, structureHeight))
                        {
                            int treePos = y - structureHeight + 32;
                            int structureX = x - structureWidth;
                            Generator.GenerateStructure("Structures/SpawnHouse", new Point16(structureX, treePos), Reverie.Instance);
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
                        return false;
                    }
                }
            }
            return true;
        }

    }
}