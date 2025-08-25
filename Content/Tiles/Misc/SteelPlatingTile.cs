using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Misc
{
    public class SteelPlatingTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;

            Main.tileMerge[TileID.Stone][Type] = true;
            Main.tileMerge[Type][TileID.Stone] = true;

            Main.tileMerge[TileID.GrayBrick][Type] = true;
            Main.tileMerge[Type][TileID.GrayBrick] = true;

            Main.tileBlockLight[Type] = true;

            DustType = DustID.Iron;
            MineResist = 1f;
            HitSound = SoundID.Tink;

            AnimationFrameHeight = 90;

            AddMapEntry(new Color(93, 70, 61));
        }
        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int yPos = j % 2;
            frameYOffset = yPos * AnimationFrameHeight;
        }
    }
}
