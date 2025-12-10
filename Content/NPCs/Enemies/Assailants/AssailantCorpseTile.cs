using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace Reverie.Content.NPCs.Enemies.Assailants;

public class AssailantCorpseTile : ModTile
{

    public override void SetStaticDefaults()
    {
        const int HEIGHT = 28;

        Main.tileSolid[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);

        TileObjectData.newTile.CoordinateWidth = 50;
        TileObjectData.newTile.CoordinateHeights = [HEIGHT];

        TileObjectData.newTile.DrawYOffset = -(HEIGHT - 20);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(88, 88, 88));
        DustType = DustID.Blood;
        HitSound = SoundID.NPCDeath12;

    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 3 : 6;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        if (i % 2 == 1)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }
}