namespace Reverie.Content.Tiles;

public class LoamTile : ModTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        
        Main.tileMergeDirt[Type] = true;
        
        Main.tileMerge[TileID.Mud][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<LoamGrassTile>()] = true;
        
        DustType = DustID.Mud;
        MineResist = 1.05f;
        HitSound = SoundID.Dig;

        AddMapEntry(new Color(132, 114, 97));
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 3;
    }
}