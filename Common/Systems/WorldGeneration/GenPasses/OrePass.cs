using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria;
using Reverie.Utilities;
using Terraria.ID;

namespace Reverie.Common.Systems.WorldGeneration.GenPasses
{
    public class OrePass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Growing minerals";
            FastNoiseLite ores = new();
            ores.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
            ores.SetFrequency(0.25f);
            ores.SetFractalType(FastNoiseLite.FractalType.FBm);
            ores.SetFractalOctaves(4);
            ores.SetFractalLacunarity(3f);
            ores.SetFractalGain(0.27f);
            ores.SetFractalWeightedStrength(0.12f);
            float placeThreshold = 0.1f;

            int posx = Main.maxTilesX;
            int posy = Main.UnderworldLayer;
            int[,] noise = new int[posx, posy];

            for (int x = 0; x < posx; x += 50)
            {
                for (int y = 0; y < posy; y += 50)
                {
                    float worldX = x;
                    float worldY = y;
                    noise[x, y] = (int)ores.GetNoise(worldX, worldY / 3);
                }
            }

            for (int x = 0; x < posx; x += 50)
            {
                for (int y = (int)GenVars.worldSurfaceLow; y < posy; y += 50)
                {
                    int worldX = x;
                    int worldY = y;
                    if (noise[x, y] > placeThreshold)
                    {
                        Tile tile = Main.tile[worldX, worldY];

                        if (worldY < GenVars.worldSurfaceLow)
                        {
                            if (tile.TileType == TileID.Dirt || tile.TileType == TileID.Stone)
                            {
                                tile.HasTile = true;
                                tile.TileType = TileID.Copper;
                            }
                            else if (tile.TileType == TileID.Sand || tile.TileType == TileID.HardenedSand || tile.TileType == TileID.Mud)
                            {
                                tile.HasTile = true;
                                tile.TileType = TileID.Tin;
                            }
                        }

                        else if (worldY >= GenVars.worldSurfaceLow && worldY < GenVars.rockLayerHigh)
                        {
                            if (tile.TileType == TileID.Stone || tile.TileType == TileID.Sandstone || tile.TileType == TileID.Mud ||
                                tile.TileType == TileID.IceBlock || tile.TileType == TileID.Marble || tile.TileType == TileID.Granite)
                            {
                                tile.HasTile = true;
                                tile.TileType = Main.rand.NextBool() ? TileID.Iron : TileID.Lead;
                            }
                        }

                        else if (worldY >= GenVars.rockLayerLow && worldY < Main.UnderworldLayer)
                        {
                            if (tile.TileType == TileID.Stone || tile.TileType == TileID.Mud || tile.TileType == TileID.Sandstone ||
                                tile.TileType == TileID.IceBlock || tile.TileType == TileID.Ash)
                            {
                                tile.HasTile = true;
                                tile.TileType = Main.rand.NextBool() ? TileID.Silver : TileID.Tungsten;
                            }

                            if (tile.TileType == TileID.Stone || tile.TileType == TileID.Mud || tile.TileType == TileID.Ash)
                            {
                                tile.HasTile = true;
                                tile.TileType = Main.rand.NextBool() ? TileID.Gold : TileID.Platinum;
                            }
                        }
                    }
                }
            }
        }
    }
}