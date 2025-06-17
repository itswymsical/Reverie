using Reverie.Content.Tiles.Rainforest.Surface;
using Terraria.DataStructures;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Rainforest;

public class CanopyLogTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<WoodgrassTile>(), ModContent.TileType<CanopyGrassTile>(), TileID.LivingWood];
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

public class CanopyRockTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<WoodgrassTile>(), ModContent.TileType<CanopyGrassTile>(), TileID.LivingWood];
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