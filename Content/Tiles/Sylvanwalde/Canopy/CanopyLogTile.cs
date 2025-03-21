using Terraria.DataStructures;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Sylvanwalde.Canopy;

public class CanopyLogTile : ModTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        Main.tileLighted[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);

        TileObjectData.newTile.AnchorValidTiles =
        [
            ModContent.TileType<WoodgrassTile>(),
            TileID.LivingWood
        ];

        TileObjectData.newTile.LavaDeath = true;

        TileObjectData.newTile.CoordinateHeights = [16, 16, 18];

        TileObjectData.newTile.Width = 5;
        TileObjectData.newTile.Height = 3;

        TileObjectData.newTile.Origin = new Point16(2, 0);

        TileObjectData.addTile(Type);

        RegisterItemDrop(ItemID.Wood);

        DustType = 39;

        AddMapEntry(new Color(114, 81, 56));
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 3;
    }

    public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
    {
        base.DropCritterChance(i, j, ref wormChance, ref grassHopperChance, ref jungleGrubChance);

        wormChance = 14;
        grassHopperChance = 10;
    }
}