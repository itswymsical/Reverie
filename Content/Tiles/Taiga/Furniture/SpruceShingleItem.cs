namespace Reverie.Content.Tiles.Taiga.Furniture;

public class SpruceShinglesItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Item.sellPrice(0);
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceShinglesTile>());
    }
}

public class SpruceShinglesTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        //Merge();

        TileID.Sets.BlockMergesWithMergeAllBlockOverride[Type] = true;
        Main.tileBlockLight[Type] = true;

        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;

        AddMapEntry(new Color(124, 91, 70));
        RegisterItemDrop(ModContent.ItemType<SpruceWoodItem>());
    }
    public void Merge(params int[] otherIds)
    {
        foreach (var id in otherIds)
        {
            Main.tileMerge[Type][id] = true;
            Main.tileMerge[id][Type] = true;
        }
    }
}
