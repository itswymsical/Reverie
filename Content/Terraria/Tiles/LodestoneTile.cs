using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Tiles
{
    public class LodestoneTile : ModTile
    {
        public override string Texture => Assets.PlaceholderTexture;
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][TileID.Stone] = true;
            Main.tileBlockLight[Type] = true;
            DustType = DustID.Tin;
            MineResist = 4f;
            MinPick = 35;
            HitSound = SoundID.Tink;

            AddMapEntry(new Color(64, 62, 59));
        }
    }
}
