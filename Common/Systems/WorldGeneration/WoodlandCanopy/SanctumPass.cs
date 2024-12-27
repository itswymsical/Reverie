using Microsoft.Xna.Framework;
using Reverie.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Reverie.Common.Systems.WorldGeneration.WoodlandCanopy.CanopyGeneration;

namespace Reverie.Common.Systems.WorldGeneration.WoodlandCanopy
{
    public class SanctumPass : GenPass
    {
        public SanctumPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Shrouding a temple in roots";
            int templeX = canopyX;
            int templeY = canopyY + canopyY / 3;
            int templeH = canopyRadiusH;
            int templeV = canopyRadiusV / 3;

            for (int x = templeX - templeH; x <= templeX + templeH; x++)
            {
                for (int y = templeY - templeV; y <= templeY + templeV; y++)
                {
                    if (Helper.GenerateTrapezoid(x, y, templeX, templeY, templeH, templeV))
                    {
                        Tile tile = Main.tile[x, y];
                        tile.WallType = WallID.GrayBrick;
                        WorldGen.PlaceTile(x, y, TileID.GrayBrick, forced: true);
                        tile.TileColor = PaintID.BlackPaint;
                        tile.WallColor = PaintID.BlackPaint;
                    }
                    progress.Set((float)((x - (templeX - templeH)) * 2 * templeV + (y - (templeY - templeV))) / (2 * templeH * (2 * templeV)));
                }
            }
        }
    }
}
