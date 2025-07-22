namespace Reverie.Content.Tiles.TemperateForest.Furniture;

public class BirchWoodTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        //Merge(TileID.WoodBlock, Type,
        //    TileID.Shadewood, TileID.Ebonwood,
        //    TileID.BorealWood, TileID.AshWood,
        //    TileID.SpookyWood, TileID.DynastyWood,
        //    TileID.PalmWood, TileID.Pearlwood,
        //    TileID.LivingWood, TileID.RichMahogany, TileID.LivingMahogany, 
        //    TileID.BambooBlock, TileID.LargeBambooBlock);
        TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
        Main.tileBlockLight[Type] = true;

        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;

        AddMapEntry(new Color(137, 119, 104));
        RegisterItemDrop(ModContent.ItemType<BirchWoodItem>());
    }
    public void Merge(params int[] otherIds)
    {
        foreach (int id in otherIds)
        {
            Main.tileMerge[Type][id] = true;
            Main.tileMerge[id][Type] = true;
        }
    }
}
