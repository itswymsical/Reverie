using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.AssailantStronghold;

public class EbonwoodBarricadeTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolidTop[Type] = true;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;

        TileID.Sets.TouchDamageImmediate[Type] = 15;
        TileID.Sets.TouchDamageBleeding[Type] = true;

        // Start with Style4x2 as base but modify for 4x4
        TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
        //TileObjectData.newTile.Width = 4;
        //TileObjectData.newTile.Height = 2;
        //TileObjectData.newTile.Origin = new Point16(2, 1);
        TileObjectData.newTile.CoordinateWidth = 18;
        TileObjectData.newTile.CoordinateHeights = new int[] { 18, 18 };
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.addTile(Type);
        MineResist = 2.5f;

        AddMapEntry(new Color(194, 110, 45));
        DustType = DustID.Ebonwood;
        HitSound = SoundID.Dig;

    }
}
