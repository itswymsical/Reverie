namespace Reverie.Content.Tiles;

public class LoamGrassTile : ModTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        
        Main.tileMerge[Type][ModContent.TileType<LoamTile>()] = true;
        
        TileID.Sets.NeedsGrassFramingDirt[ModContent.TileType<LoamTile>()] = Type;
        
        DustType = DustID.JungleGrass;
        MineResist = 1.05f;
        HitSound = SoundID.Dig;

        AddMapEntry(new Color(98, 134, 13));
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 3;
    }
}