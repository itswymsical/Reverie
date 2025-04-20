namespace Reverie.Content.Tiles.Canopy;

public class BramblerootTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        DustType = DustID.t_LivingWood;
        MineResist = 1.2f;
        HitSound = SoundID.Dig;

        AddMapEntry(new Color(162, 124, 92));
    }
    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 2 : 4;
    }
}