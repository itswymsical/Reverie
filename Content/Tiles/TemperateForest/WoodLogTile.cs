using Reverie.Content.Items.Botany;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.TemperateForest;

public class WoodLogTile : ModTile
{
    public override void SetStaticDefaults()
    {
        const int height = 44;

        Main.tileSolid[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;

        TileID.Sets.BreakableWhenPlacing[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);

        TileObjectData.newTile.CoordinateWidth = 82;
        TileObjectData.newTile.CoordinateHeights = [height, 0];
        
        TileObjectData.newTile.DrawYOffset = -(height - 36);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<TemperateGrassTile>()];

        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(145, 92, 61));
        DustType = DustID.t_LivingWood;
        HitSound = SoundID.Dig;

        RegisterItemDrop(ItemID.Wood);
    }

    public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
    {
        wormChance = 18;
        grassHopperChance = 12;
    }
    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 8 : 12;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        if (i % 2 == 1)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!fail && !effectOnly)
        {
            if (Main.rand.NextBool(6))
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<MagnoliaItem>(), Main.rand.Next(1, 2));
            else
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.Wood, Main.rand.Next(2, 7));
        }
    }
}
