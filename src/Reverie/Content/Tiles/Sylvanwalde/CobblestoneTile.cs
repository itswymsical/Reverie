namespace Reverie.Content.Tiles;

public class CobblestoneTile : ModTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        
        Main.tileMerge[TileID.Mud][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<LoamTile>()] = true;
        
        TileID.Sets.NeedsGrassFramingDirt[ModContent.TileType<LoamTile>()] = Type;

        DustType = DustID.Stone;
        MineResist = 1.25f;
        HitSound = SoundID.Tink;

        AddMapEntry(new Color(101, 76, 109));
    }
    
    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 3;
    }
}