using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Reverie.Common.Global;

namespace Reverie.Common.Players
{
    public class ShovelPlayer : ModPlayer
    {
        public void DigBlocks(int i, int j)
        {
            // -1 (left) , 0 (center), 1 (right)
            for (int num = -1; num < 2; num++)
            {
                if (num != 0) //skip center, the vanilla pick code will be used to break the center tile instead. dont ask why
                {
                    BreakTileIfValid(i / 16 + num, j / 16);
                    BreakTileIfValid(i / 16, j / 16 + num);
                }
            }
            //BreakTileIfValid(i / 16, j / 16);
        }
        
        private void BreakTileIfValid(int i, int j)
        {
            int digTile = Player.HeldItem.GetGlobalItem<ReverieGlobalItem>().digPower;

            if (!IsExcludedTile(i, j))
            {
                Player.PickTile(i, j, digTile);
                if (!IsSoftTile(i, j))
                {
                    Player.PickTile(i, j, digTile - (digTile / 2));
                }
            }
        }
        
        private bool IsExcludedTile(int i, int j)
        {
            int tileType = Main.tile[i, j].TileType;
            return Main.tileAxe[tileType] || Main.tileHammer[tileType];
        }
        private bool IsSoftTile(int i, int j)
        {
            int tileType = Main.tile[i, j].TileType;
            return TileID.Sets.CanBeDugByShovel[tileType];
        }
    }
}