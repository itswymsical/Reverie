namespace Reverie.Content.Tiles.Sylvanwalde.Canopy;

public class BramblerootTile : ModTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        Main.tileMergeDirt[Type] = true;

        DustType = DustID.t_LivingWood;
        MineResist = 1.2f;
        HitSound = SoundID.Dig;

        AddMapEntry(new Color(162, 124, 92));
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 3;
    }
}