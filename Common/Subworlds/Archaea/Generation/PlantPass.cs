using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Systems.Subworlds.Archaea
{
    public class PlantPass : GenPass
    {
        public PlantPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Growing plants";
            int fail = 0;
            for (int i = 1; i < Main.maxTilesX - 1; i++)
            {
                progress.Set(i / Main.maxTilesX);

                if (fail > 0)
                {
                    fail--;
                    continue;
                }

                if (WorldGen.genRand.NextBool(5))
                {
                    for (int j = 1; j < Main.worldSurface; j++)
                    {
                        if (Main.tile[i, j].HasTile && Main.tile[i, j].BlockType == BlockType.Solid)
                        {
                            WorldGen.PlaceTile(i, j - 1, TileID.Cactus, mute: true);
                            //WorldGen.PlantCactus(i, j);
                            fail = WorldGen.genRand.Next(5, 10);
                            break;
                        }
                    }
                }

                if (WorldGen.genRand.NextBool(22))
                {
                    for (int j = 1; j < Main.worldSurface; j++)
                    {
                        if (Main.tile[i, j].HasTile && Main.tile[i, j].BlockType == BlockType.Solid)
                        {
                            WorldGen.PlaceTile(i, j - 1, TileID.Saplings, mute: true);
                            fail = WorldGen.genRand.Next(5, 10);
                            WorldGen.GrowPalmTree(i, j - 1);
                            break;
                        }
                    }
                }
            }
        }
    }
}