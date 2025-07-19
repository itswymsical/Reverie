using Reverie.Content.Items.Mycology;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Mycology;

public class ClusterclotCapTile_Spores : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileObsidianKill[Type] = true;

        DustType = DustID.CrimsonPlants;
        HitSound = SoundID.NPCDeath21;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);

        TileObjectData.addTile(Type);
        AddMapEntry(new Color(210, 103, 96));
    }
    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 3;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        base.SetSpriteEffects(i, j, ref spriteEffects);

        spriteEffects = i % 2 == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
    }
}

public class ClusterclotCapTile_Small : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;

        DustType = DustID.CrimsonPlants;
        const int HEIGHT = 22;
        HitSound = SoundID.NPCDeath21;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateWidth = 28;
        TileObjectData.newTile.CoordinateHeights = [HEIGHT];
        TileObjectData.newTile.DrawYOffset = -(HEIGHT - 18 - 2);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 5;

        TileObjectData.addTile(Type);
        RegisterItemDrop(ModContent.ItemType<ClusterclotCapItem>());
        AddMapEntry(new Color(210, 103, 96));
    }
    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 6;
    }
}

public class ClusterclotCapTile_Large : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileObsidianKill[Type] = true;

        DustType = DustID.CrimsonPlants;
        HitSound = SoundID.NPCDeath21;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.DrawYOffset = 2;

        TileObjectData.addTile(Type);
        RegisterItemDrop(ModContent.ItemType<ClusterclotCapItem>());
        AddMapEntry(new Color(210, 103, 96));
    }
    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 7;
    }
}
