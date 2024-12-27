using Reverie.Content.Terraria.Tiles.Canopy;
using Reverie.Content.Tiles.Canopy;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Reverie.Common.Systems.WorldGeneration.WoodlandCanopy
{
    public class CanopyGeneration : ModSystem
    {
        public static int trunkX;

        public static int spawnX = (Main.maxTilesX / 2) + (Main.maxTilesX / 128);
        public static int spawnY = (int)Main.worldSurface;

        public static int trunkBottom = (spawnY - 10) + ((int)Main.worldSurface / 2);

        public static int canopyX = spawnX + 40;
        public static int canopyY = trunkBottom + (Main.maxTilesY / 6);

        public static int canopyRadiusH = (int)(Main.maxTilesX * 0.047f);
        public static int canopyRadiusV = (int)(Main.maxTilesY * 0.18f);

        public static int treeWood = TileID.LivingWood;
        public static int treeWall = WallID.LivingWoodUnsafe;
        public static int canopyWall = WallID.DirtUnsafe4;
        public static int treeLeaves = TileID.LeafBlock;
        public static int canopyBlock = ModContent.TileType<Woodgrass>();
        public static int canopyVines = ModContent.TileType<CanopyVine>();
    }
}