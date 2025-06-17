
namespace Reverie.Content.Tiles;

public class TombBrickWall : ModWall
{
    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;

        DustType = DustID.Stone;

        AddMapEntry(new Color(71, 69, 71));
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}