namespace Reverie.Content.Tiles.Taiga.Furniture;

public class SpruceWoodItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Item.sellPrice(0);
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceWoodTile>());
    }
}

public class SpruceWoodTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Merge(TileID.WoodBlock, Type,
            TileID.Shadewood, TileID.Ebonwood,
            TileID.BorealWood, TileID.AshWood,
            TileID.SpookyWood, TileID.DynastyWood,
            TileID.PalmWood, TileID.Pearlwood,
            TileID.LivingWood, TileID.RichMahogany, TileID.LivingMahogany,
            TileID.BambooBlock, TileID.LargeBambooBlock, ModContent.TileType<PeatTile>());
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