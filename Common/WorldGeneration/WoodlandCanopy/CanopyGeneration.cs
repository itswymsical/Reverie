using Reverie.Content.Tiles.Canopy;

namespace Reverie.Common.WorldGeneration.WoodlandCanopy
{
    public class CanopyGeneration : ModSystem
    {
        public static int trunkX;

        public static int spawnX = Main.spawnTileX - 250;
        public static int spawnY = Main.spawnTileY;

        public static int trunkBottom = spawnY - 10 + (int)Main.worldSurface / 2;

        public static int canopyX = spawnX;
        public static int canopyY = trunkBottom + Main.maxTilesY / 6;

        public static int canopyRadiusH = (int)(Main.maxTilesX * 0.047f);
        public static int canopyRadiusV = (int)(Main.maxTilesY * 0.18f);

        public static int treeWood = TileID.LivingWood;
        public static int treeWall = WallID.LivingWoodUnsafe;
        public static int canopyWall = WallID.DirtUnsafe4;
        public static int treeLeaves = TileID.LeafBlock;
        public static int canopyBlock = ModContent.TileType<WoodgrassTile>();
        public static int canopyVines = ModContent.TileType<CanopyVineTile>();
    }
}