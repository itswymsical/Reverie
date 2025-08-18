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

        TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);

        TileObjectData.newTile.CoordinateWidth = 18;
        TileObjectData.newTile.CoordinateHeights = new int[] { 18, 18 };
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.addTile(Type);
        MineResist = 2.5f;

        AddMapEntry(new Color(156, 87, 170), CreateMapEntryName());
        DustType = DustID.Ebonwood;
        HitSound = SoundID.Dig;

    }
}
