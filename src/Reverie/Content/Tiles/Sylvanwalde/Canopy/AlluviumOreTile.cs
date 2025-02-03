namespace Reverie.Content.Tiles.Canopy;

public class AlluviumOreTile : ModTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        Main.tileSolid[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileSpelunker[Type] = true;
        Main.tileBlockLight[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[Type][TileID.LivingWood] = true;
        Main.tileMerge[TileID.LivingWood][Type] = true;

        DustType = 151;
        MineResist = 1.35f;
        MinPick = 50;
        HitSound = SoundID.Tink;

        AddMapEntry(new Color(108, 187, 86));
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 3;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        base.ModifyLight(i, j, ref r, ref g, ref b);

        r = 0f;
        g = 0.14f;
        b = 0.12f;
    }
}