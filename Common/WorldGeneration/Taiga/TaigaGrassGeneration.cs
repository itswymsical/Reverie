using Reverie.Content.Tiles.Taiga;
using Reverie.lib;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.Taiga;

public class TaigaGrassGenPass : GenPass
{
    private static int grassSpread;

    public TaigaGrassGenPass() : base("Spreading Taiga Grass", 247.5f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        if (!WorldGen.remixWorldGen)
        {
            progress.Message = "Growing Taiga Grass";

            var left = (int)(GenVars.snowOriginLeft / 1.4f);
            var right = GenVars.snowOriginRight + 70;

            for (var x = left + 10; x < right - 10; x++)
            {
                var foundSurface = true;

                for (var y = 0; y < Main.worldSurface - 1.0; y++)
                {
                    if (Main.tile[x, y].HasTile)
                    {
                        if (foundSurface && Main.tile[x, y].TileType == (ushort)ModContent.TileType<PeatTile>())
                        {
                            try
                            {
                                grassSpread = 0;
                                SpreadGrass(x, y);
                            }
                            catch
                            {
                                grassSpread = 0;
                                SpreadGrass(x, y, 0, 2, repeat: false);
                            }
                        }

                        if (y > GenVars.worldSurfaceHigh)
                            break;

                        foundSurface = false;
                    }
                    else if (Main.tile[x, y].WallType == 0)
                    {
                        foundSurface = true;
                    }
                }
            }

            PlaceDecorations(left, right, progress);
        }
    }

    private void SpreadGrass(int x, int y, int direction = 0, int steps = 5, bool repeat = true)
    {
        if (steps <= 0 || ++grassSpread > 1000)
            return;

        if (!WorldGen.InWorld(x, y))
            return;

        var tile = Main.tile[x, y];
        var tiletype = Main.rand.NextBool(2) ? (ushort)ModContent.TileType<TaigaGrassTile>() : (ushort)ModContent.TileType<TaigaSnowGrassTile>();
        if (tile.HasTile && tile.TileType == (ushort)ModContent.TileType<PeatTile>())
        {
            if (!Main.tile[x, y - 1].HasTile || !Main.tile[x - 1, y].HasTile || !Main.tile[x + 1, y].HasTile)
            {
                //tile.TileType = tiletype;
                tile.TileType = (ushort)ModContent.TileType<TaigaSnowGrassTile>();
                if (repeat)
                {

                    if (direction != 1 && WorldGen.genRand.NextBool(3))
                        SpreadGrass(x - 1, y, 2, steps - 1, repeat);

                    if (direction != 2 && WorldGen.genRand.NextBool(3))
                        SpreadGrass(x + 1, y, 1, steps - 1, repeat);

                    if (direction != 3 && WorldGen.genRand.NextBool(3))
                        SpreadGrass(x, y - 1, 4, steps - 1, repeat);

                    if (direction != 4 && WorldGen.genRand.NextBool(3))
                        SpreadGrass(x, y + 1, 3, steps - 1, repeat);

                    if (WorldGen.genRand.NextBool(4))
                        SpreadGrass(x - 1, y - 1, 0, steps - 1, repeat);

                    if (WorldGen.genRand.NextBool(4))
                        SpreadGrass(x + 1, y - 1, 0, steps - 1, repeat);

                    if (WorldGen.genRand.NextBool(4))
                        SpreadGrass(x - 1, y + 1, 0, steps - 1, repeat);

                    if (WorldGen.genRand.NextBool(4))
                        SpreadGrass(x + 1, y + 1, 0, steps - 1, repeat);
                }
            }
        }
    }

    private void PlaceDecorations(int left, int right, GenerationProgress progress)
    {
        progress.Message = "Adding Taiga Plants";

        var noise = new FastNoiseLite(WorldGen.genRand.Next());
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.04f);

        var top = (int)(Main.worldSurface * 0.35);
        var bottom = (int)(Main.worldSurface * 1.2);

        var total = right - left;
        var current = 0;

        for (var x = left; x < right; x++)
        {
            progress.Set((float)current++ / total);

            var surfaceY = FindSurface(x, top, bottom);
            if (surfaceY <= 0) continue;

            var tile = Main.tile[x, surfaceY];
            var tileAbove = Main.tile[x, surfaceY - 1];

            if (tile.HasTile && tile.TileType == (ushort)ModContent.TileType<TaigaGrassTile>() && !tileAbove.HasTile)
            {
                var noiseValue = noise.GetNoise(x * 0.1f, surfaceY * 0.1f) * 0.5f + 0.5f;

                if (WorldGen.genRand.NextFloat() < 0.4f * noiseValue)
                {
                    PlaceTaigaPlant(x, surfaceY - 1);
                }
            }
        }
    }

    private int FindSurface(int x, int startY, int endY)
    {
        // Find the first solid block from top to bottom
        for (var y = startY; y < endY; y++)
        {
            if (Main.tile[x, y].HasTile && Main.tile[x, y].BlockType == BlockType.Solid)
            {
                return y;
            }
        }
        return -1;
    }

    private void PlaceTaigaPlant(int x, int y)
    {
        // Place a plant tile
        WorldGen.PlaceTile(x, y, (ushort)ModContent.TileType<TaigaPlants>(), style: 0, mute: true);

        // Set the frame to a random plant variant
        var plantTile = Main.tile[x, y];
        if (plantTile.HasTile)
        {
            plantTile.TileFrameY = 0;
            plantTile.TileFrameX = (short)(WorldGen.genRand.Next(10) * 18);

            // Update frames and send network message if needed
            WorldGen.SquareTileFrame(x, y, true);
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
            }
        }
    }

    private void PlaceTree(int x, int y)
    {
        // For now, use vanilla tree mechanics (will be replaced with custom spruce trees)
        WorldGen.PlaceTile(x, y, TileID.Saplings, style: 0, mute: true);
        WorldGen.GrowTree(x, y);
    }
}