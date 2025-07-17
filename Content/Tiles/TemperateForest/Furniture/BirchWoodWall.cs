using Reverie.Content.Dusts;

namespace Reverie.Content.Tiles.TemperateForest.Furniture;

public class BirchWoodWall : ModWall
{
    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;

        DustType = ModContent.DustType<BirchDust>();

        AddMapEntry(new Color(107, 89, 84));
        RegisterItemDrop(ModContent.ItemType<BirchWallItem>());

    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}