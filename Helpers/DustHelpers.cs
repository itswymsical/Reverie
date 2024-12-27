using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Helpers
{
    internal static partial class Helper
    {
        public static void SpawnDustCloud(Vector2 position, int width, int height, int type, int amount = 10, float speedX = 0, float speedY = 0, int alpha = 0, Color c = default, float scale = 1f)
        {
            for (int i = 0; i < amount; ++i)
            {
                Dust.NewDust(position, width, height, type, speedX, speedY, alpha, c, scale);
            }
        }
        public static bool IsPointInsideRadius(int x, int y, int centerX, int centerY, int horizontalRadius, int verticalRadius)
        {
            float dx = (x - centerX);
            float dy = (y - centerY);
            return (dx * dx) / (horizontalRadius * horizontalRadius) + (dy * dy) / (verticalRadius * verticalRadius) <= 1;
        }
        /// <summary>
        /// Uses cellular automation rules to create a dust effect.
        /// </summary>
        /// <param name="cX"></param>
        /// <param name="cY"></param>
        /// <param name="hR"></param>
        /// <param name="vR"></param>
        /// <param name="density"></param>
        /// <param name="iterations"></param>
        /// <param name="dustType"></param>
        public static void SpawnCellDust(int cX, int cY, int hR, int vR, int density, int iterations, int dustType, int alpha, float size, Color color)
        {
            bool[,] grid = new bool[hR * 2, vR * 2];
            for (int x = 0; x < hR * 2; x++)
            {
                for (int y = 0; y < vR * 2; y++)
                {
                    if (IsPointInsideRadius(x + cX - hR, y + cY - vR, cX, cY, hR, vR))
                    {
                        grid[x, y] = Main.rand.Next(100) < density;
                    }
                    else
                    {
                        grid[x, y] = false;
                    }
                }
            }
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                grid = PerformCellularAutomataStep(grid, hR * 2, vR * 2);
            }
            for (int x = 0; x < hR * 2; x++)
            {
                for (int y = 0; y < vR * 2; y++)
                {
                    if (grid[x, y])
                    {
                        int worldX = cX - hR + x;
                        int worldY = cY - vR + y;
                        Dust.NewDust(new Vector2(worldX, worldY), hR, vR, dustType, Alpha: alpha, Scale: size, newColor: color);

                    }
                }
            }
        }
        private static bool[,] PerformCellularAutomataStep(bool[,] map, int width, int height)
        {
            bool[,] newMap = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int solidNeighbors = CountSolidNeighbors(map, x, y, width, height);

                    // The rules of cellular automata
                    if (solidNeighbors > 4)
                        newMap[x, y] = true; // Tile becomes solid
                    else if (solidNeighbors < 4)
                        newMap[x, y] = false; // Tile becomes empty
                    else
                        newMap[x, y] = map[x, y]; // Remains the same
                }
            }

            return newMap;
        }
        private static int CountSolidNeighbors(bool[,] map, int x, int y, int width, int height)
        {
            int count = 0;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }

                    int neighborX = x + i;
                    int neighborY = y + j;

                    if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                    {
                        if (map[neighborX, neighborY])
                        {
                            count++;
                        }
                    }
                    else
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}