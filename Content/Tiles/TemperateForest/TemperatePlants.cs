using Reverie.Content.Tiles.Taiga;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.TemperateForest;

public class TemperatePlants : ModTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        Main.tileCut[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);

        TileObjectData.newTile.AnchorValidTiles =
        [
            ModContent.TileType<TemperateGrassTile>(),
            Type
        ];

        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.WaterDeath = false;

        TileObjectData.newTile.StyleHorizontal = true;

        TileObjectData.newTile.CoordinateHeights = [18];

        TileObjectData.addTile(Type);

        DustType = DustID.GrassBlades;
        HitSound = SoundID.Grass;

        AddMapEntry(Color.DarkGreen);
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

public class TemperateRockTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;

        TileID.Sets.BreakableWhenPlacing[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.Origin = new(2, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 3, 0);
        TileObjectData.newTile.AnchorValidTiles = [TileID.Stone];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(100, 92, 80));
        DustType = DustID.Stone;
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