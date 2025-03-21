using Reverie.Common.Subworlds.Archaea.Generation;
using Reverie.Content.Tiles.Sylvanwalde.Canopy;
using Reverie.lib;
using Reverie.Utilities;

using StructureHelper;

using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;

using static Reverie.Common.Subworlds.Sylvanwalde.Generation.WoodlandCanopy.CanopyGeneration;

namespace Reverie.Common.Subworlds.Sylvanwalde.Generation.WoodlandCanopy
{
    public class CanopyPass : GenPass
    {
        public CanopyPass() : base("[Sylvan] Canopy", 247.43f)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Generating Woodland Canopy";

            for (var x = canopyX - canopyRadiusH; x <= canopyX + canopyRadiusH; x++)
            {
                for (var y = canopyY - canopyRadiusV; y <= canopyY + canopyRadiusV; y++)
                {
                    if (WorldGenUtils.GenerateCanopyShape(x, y, canopyX, canopyY, width: canopyRadiusH, height: canopyRadiusV / 4,
                        curveFrequency: .04f, curveAmplitude: canopyRadiusH / 8, thornHeight: 180, thornWidth: 24))
                    {
                        var tile = Main.tile[x, y];
                        WorldGen.KillWall(x, y);
                        WorldGen.PlaceWall(x, y, canopyWall);
                        tile.TileType = (ushort)treeWood;
                        tile.HasTile = true;
                    }
                    // Tracking generation progress.
                    progress.Set((float)((x - (canopyX - canopyRadiusH)) * 2 * canopyRadiusV + (y - (canopyY - canopyRadiusV))) / (2 * canopyRadiusH * (2 * canopyRadiusV)));
                }
            }
            CarveRoots();
        }
        public void CarveRoots()
        {
            var roots = new FastNoiseLite(Main.ActiveWorldFileData.Seed);
            roots.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
            roots.SetFractalType(FastNoiseLite.FractalType.Ridged);
            roots.SetFractalGain(0.435f);
            roots.SetFractalOctaves(3); // Since this is fractal noise, our noise map is almost infinite.
                                        // '<' value zooms out of the noise grid (many, small caves), '>' value zooms in (bigger caves)
            roots.SetFrequency(0.021f); //i think this a good value.

            var posx = canopyRadiusH * 2;
            var posy = canopyRadiusV * 2;
            var threshold = 0.33f;

            var noiseData = new float[posx, posy];

            for (var x = 0; x < posx; x++)
            {
                for (var y = 0; y < posy; y++)
                {
                    var worldX = x + (canopyX - canopyRadiusH);
                    var worldY = y + (canopyY - canopyRadiusV);

                    noiseData[x, y] = roots.GetNoise(worldX / 2, worldY - worldY / 4);
                }
            }

            for (var x = 0; x < posx; x++)
            {
                for (var y = 0; y < posy; y++)
                {
                    var worldX = x + (canopyX - canopyRadiusH);
                    var worldY = y + (canopyY - canopyRadiusV);

                    if (WorldGenUtils.GenerateCanopyShape(worldX, worldY, canopyX, canopyY, width: canopyRadiusH, height: canopyRadiusV,
                       curveFrequency: .04f, curveAmplitude: canopyRadiusH / 4, thornHeight: 120, thornWidth: 12))
                    {
                        if (noiseData[x, y] > threshold)
                            WorldGen.KillTile(worldX, worldY);

                        if (noiseData[x, y] < threshold * 0.14f)
                        {
                            WorldGen.PlaceTile(worldX, worldY, TileID.Dirt, forced: true);
                        }
                    }
                }
            }
            for (var x2 = canopyX - canopyRadiusH; x2 <= canopyX + canopyRadiusH; x2++)
            {
                for (var y2 = canopyY + 50 - canopyRadiusV; y2 <= canopyY + 50 + canopyRadiusV; y2++)
                {
                    if (WorldGenUtils.GenerateCanopyShape(x2, y2, canopyX, canopyY, canopyRadiusH, canopyRadiusV, 0.04f, canopyRadiusH / 4, 100, 15))
                    {
                        if (WorldGen.genRand.NextBool(160))
                            WorldGen.OreRunner(x2, y2, 5, 7, (ushort)ModContent.TileType<AlluviumOreTile>());
                    }
                }
            }
            //WorldGenUtils.GenerateCellNoise_Walls(canopyX, canopyY, canopyRadiusH, canopyRadiusV, 48, 10);
            var direction = Main.rand.Next(0, 1);
            var offset = Main.rand.Next(130);
            var houseX = direction == 1 ? canopyX + offset : canopyX - offset;
            var houseY = canopyY - 180;

            if (WorldGenUtils.GenerateCanopyShape(houseX, houseY, canopyX, canopyY, width: canopyRadiusH, height: canopyRadiusV,
                curveFrequency: 0.04f, curveAmplitude: canopyRadiusH / 4, thornWidth: 100, thornHeight: 15))
            {
                Generator.GenerateStructure("Structures/StumpyHouse", new Point16(houseX, houseY), Instance);

                var houseWidth = 31;
                var houseHeight = 32;

                var spawnX = houseX + (int)(houseWidth * 0.57);
                var spawnY = houseY + (int)(houseHeight * 0.85);

                //var stumpy = NPC.NewNPC(new EntitySource_WorldGen(), spawnX * 16, spawnY * 16, ModContent.NPCType<Stumpy>());

                //Main.npc[stumpy].homeTileX = spawnX;
                //Main.npc[stumpy].homeTileY = spawnY;

                GetAdjustedFloorPosition(spawnX, spawnY);
            }
        }

        private static Point GetAdjustedFloorPosition(int x, int y)
        {
            var num = x - 1;
            var num2 = y - 2;
            var isEmpty = false;
            var hasFloor = false;
            while (!isEmpty && num2 > Main.spawnTileY - 10)
            {
                Scan3By3(num, num2, out isEmpty, out hasFloor);
                if (!isEmpty)
                    num2--;
            }

            while (!hasFloor && num2 < Main.spawnTileY + 10)
            {
                Scan3By3(num, num2, out isEmpty, out hasFloor);
                if (!hasFloor)
                    num2++;
            }

            return new Point(num + 1, num2 + 2);
        }

        private static void Scan3By3(int topLeftX, int topLeftY, out bool isEmpty, out bool hasFloor)
        {
            isEmpty = true;
            hasFloor = false;
            for (var i = 0; i < 3; i++)
            {
                var num = 0;
                while (num < 3)
                {
                    var i2 = topLeftX + i;
                    var j = topLeftY + num;
                    if (!WorldGen.SolidTile(i2, j))
                    {
                        num++;
                        continue;
                    }

                    goto IL_001e;
                }

                continue;
            IL_001e:
                isEmpty = false;

                break;
            }

            for (var k = 0; k < 3; k++)
            {
                var i3 = topLeftX + k;
                var j2 = topLeftY + 3;
                if (WorldGen.SolidTile(i3, j2))
                {
                    hasFloor = true;
                    break;
                }
            }
        }
    }
}
