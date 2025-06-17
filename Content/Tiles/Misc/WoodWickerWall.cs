
namespace Reverie.Content.Tiles
{
    public class WoodWickerWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;

            DustType = DustID.WoodFurniture;

            AddMapEntry(new Color(162, 121, 88));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}