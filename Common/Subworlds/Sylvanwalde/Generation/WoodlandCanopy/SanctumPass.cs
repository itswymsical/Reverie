using Reverie.Utilities;
using Terraria.IO;

using Terraria.WorldBuilding;
using static Reverie.Common.Subworlds.Sylvanwalde.Generation.WoodlandCanopy.CanopyGeneration;

namespace Reverie.Common.Subworlds.Sylvanwalde.Generation.WoodlandCanopy
{
    public class SanctumPass : GenPass
    {
        public SanctumPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Shrouding a temple in roots";
            var templeX = canopyX;
            var templeY = canopyY + canopyY / 3;
            var templeH = canopyRadiusH;
            var templeV = canopyRadiusV / 3;

            for (var x = templeX - templeH; x <= templeX + templeH; x++)
            {
                for (var y = templeY - templeV; y <= templeY + templeV; y++)
                {
                    if (WorldGenUtils.GenerateTrapezoid(x, y, templeX, templeY, templeH, templeV))
                    {
                        var tile = Main.tile[x, y];
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
