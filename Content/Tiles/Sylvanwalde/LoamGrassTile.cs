﻿using Reverie.Content.Items.Sylvanwalde.Canopy;
using Reverie.Content.Tiles.Sylvanwalde.Canopy;
using Terraria.DataStructures;

namespace Reverie.Content.Tiles.Sylvanwalde;

public class LoamGrassTile : ModTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        
        Main.tileMerge[Type][ModContent.TileType<LoamTile>()] = true;
        Main.tileMerge[Type][ModContent.TileType<CobblestoneTile>()] = true;

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[ModContent.TileType<LoamTile>()] = Type;
        
        DustType = DustID.JungleGrass;
        MineResist = 1.05f;
        HitSound = SoundID.Dig;

        AddMapEntry(new Color(98, 134, 13));
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 3;
    }
    public override void RandomUpdate(int i, int j)
    {
        base.RandomUpdate(i, j);

        var tile = Framing.GetTileSafely(i, j);

        var tileAbove = Framing.GetTileSafely(i, j - 1);
        var tileBelow = Framing.GetTileSafely(i, j + 1);

        if (!WorldGen.genRand.NextBool() || tileAbove.HasTile || tile.LeftSlope || tile.RightSlope || tile.IsHalfBlock)
        {
            return;
        }

        WorldGen.PlaceTile(i, j - 1, (ushort)ModContent.TileType<CanopyFoliageTile>(), true);

        tileAbove.TileFrameY = 0;
        tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(10) * 18);

        WorldGen.SquareTileFrame(i, j + 1);

        NetMessage.SendTileSquare(-1, i, j - 1, 1);
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail || !Main.rand.NextBool(30))
        {
            return;
        }

        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<WoodgrassSeedsItem>());
    }
}