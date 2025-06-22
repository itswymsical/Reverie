using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Canopy;

public class WardenAltarTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
        TileObjectData.newTile.Width = 6;
        TileObjectData.newTile.Height = 6;

        TileObjectData.newTile.Origin = new Point16(0, 0);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 18];

        TileObjectData.addTile(Type);

        DustType = 39;
        AddMapEntry(Color.Brown);
    }
    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
    
    //public override bool RightClick(int i, int j)
    //{
    //    if (NPC.AnyNPCs(ModContent.NPCType<WoodenWarden>()))
    //        return false;
    //    else
    //    {
    //        NPC.NewNPC(default, i * 16, j * 16, ModContent.NPCType<WoodenWarden>());
    //        SoundEngine.PlaySound(SoundID.Roar, new Vector2(i * 16, j * 16));
    //        return true;
    //    }
    //}
    public override void MouseOver(int i, int j)
    {
        base.MouseOver(i, j);
        Main.LocalPlayer.cursorItemIconEnabled = true;
        Main.LocalPlayer.cursorItemIconID = ItemID.GoldenKey;
    }
}