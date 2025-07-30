using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Taiga;

public class TaigaPlants : ModTile
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
            ModContent.TileType<TaigaGrassTile>(),
        ];

        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.WaterDeath = false;

        TileObjectData.newTile.StyleHorizontal = true;


        TileObjectData.newTile.CoordinateHeights = [36];
        TileObjectData.newTile.DrawYOffset = -14;
        TileObjectData.addTile(Type);

        DustType = DustID.GrassBlades;
        HitSound = SoundID.Grass;

        AddMapEntry(new Color(108, 170, 122));
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

public class SnowTaigaPlants : ModTile
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
            ModContent.TileType<SnowTaigaGrassTile>(),
        ];

        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.WaterDeath = false;

        TileObjectData.newTile.StyleHorizontal = true;


        TileObjectData.newTile.CoordinateHeights = [36];
        TileObjectData.newTile.DrawYOffset = -14;
        TileObjectData.addTile(Type);

        DustType = DustID.SnowBlock;
        HitSound = SoundID.Grass;

        AddMapEntry(Color.AliceBlue);
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

public class CorruptTaigaPlants : ModTile
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
            ModContent.TileType<CorruptTaigaGrassTile>(),
        ];

        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.WaterDeath = false;

        TileObjectData.newTile.StyleHorizontal = true;


        TileObjectData.newTile.CoordinateHeights = [36];
        TileObjectData.newTile.DrawYOffset = -14;
        TileObjectData.addTile(Type);

        DustType = DustID.CorruptPlants;
        HitSound = SoundID.Grass;

        AddMapEntry(new Color(173, 155, 224));
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

public class CrimsonTaigaPlants : ModTile
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
            ModContent.TileType<CrimsonTaigaGrassTile>(),
        ];

        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.WaterDeath = false;

        TileObjectData.newTile.StyleHorizontal = true;


        TileObjectData.newTile.CoordinateHeights = [36];
        TileObjectData.newTile.DrawYOffset = -14;
        TileObjectData.addTile(Type);

        DustType = DustID.CrimsonPlants;
        HitSound = SoundID.Grass;

        AddMapEntry(new Color(255, 128, 128));
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

public class HallowTaigaPlants : ModTile
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
            ModContent.TileType<HallowTaigaGrassTile>(),
        ];

        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.WaterDeath = false;

        TileObjectData.newTile.StyleHorizontal = true;


        TileObjectData.newTile.CoordinateHeights = [36];
        TileObjectData.newTile.DrawYOffset = -14;
        TileObjectData.addTile(Type);

        DustType = DustID.HallowedPlants;
        HitSound = SoundID.Grass;

        AddMapEntry(Color.LightCyan);
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