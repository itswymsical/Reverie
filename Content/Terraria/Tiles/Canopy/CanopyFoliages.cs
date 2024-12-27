using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Humanizer;
using Terraria.DataStructures;

namespace Reverie.Content.Terraria.Tiles.Canopy
{
    public class CanopyLogFoliage : ModTile
    {
        public override string Texture => Assets.Terraria.Tiles.Canopy + Name;
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<Woodgrass>(), TileID.LivingWood];
            TileObjectData.newTile.LavaDeath = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 3;

            TileObjectData.newTile.Origin = new Point16(2, 0);
            TileObjectData.newTile.CoordinateHeights = [16, 16, 18];

            TileObjectData.addTile(Type);

            RegisterItemDrop(ItemID.Wood);
            DustType = 39;
            AddMapEntry(new Color(114, 81, 56));
        }
        public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
        {
            wormChance = 14;
            grassHopperChance = 10;
        }
    }
    public class CanopyRockFoliage : ModTile
    {
        public override string Texture => Assets.Terraria.Tiles.Canopy + Name;
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<Woodgrass>(), TileID.LivingWood];
            TileObjectData.newTile.LavaDeath = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 3;

            TileObjectData.newTile.Origin = new Point16(2, 0);
            TileObjectData.newTile.CoordinateHeights = [16, 16, 18];

            TileObjectData.addTile(Type);

            RegisterItemDrop(ItemID.StoneBlock);
            DustType = 39;
            AddMapEntry(Color.Gray);
        }
        public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
        {
            wormChance = 14;
            grassHopperChance = 10;
        }
    }
}