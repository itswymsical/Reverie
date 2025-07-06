using Reverie.Content.Items.Tiles.TemperateForest;

namespace Reverie.Content.Tiles.TemperateForest.Furniture;

public class BirchWoodTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[TileID.WoodBlock][Type] = true;
        Main.tileMerge[Type][TileID.WoodBlock] = true;

        Main.tileBlockLight[Type] = true;

        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;

        AddMapEntry(new Color(137, 119, 104));
        RegisterItemDrop(ModContent.ItemType<BirchWoodItem>());
    }
}
