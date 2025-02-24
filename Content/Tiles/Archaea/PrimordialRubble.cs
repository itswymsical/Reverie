using Reverie.Content.Items.Archaea;
using Terraria.GameContent;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Archaea;

public abstract class PrimordialRubble1x1Base : ModTile
{
    public override string Texture => "Reverie/Assets/Textures/Tiles/Archaea/PrimordialRubble1x1";

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileObsidianKill[Type] = true;

        DustType = DustID.Stone;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(152, 171, 198));
    }
}
public abstract class PrimordialRubble2x1Base : ModTile
{
    public override string Texture => "Reverie/Assets/Textures/Tiles/Archaea/PrimordialRubble2x1";

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileObsidianKill[Type] = true;

        DustType = DustID.Stone;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.DrawYOffset = 2;

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(152, 171, 198));
    }
}
public abstract class PrimordialRubble3x2Base : ModTile
{
    public override string Texture => "Reverie/Assets/Textures/Tiles/Archaea/PrimordialRubble3x2";

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileObsidianKill[Type] = true;

        DustType = DustID.Stone;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(152, 171, 198));
    }
}

public class PrimordialRubble1x1Fake : PrimordialRubble1x1Base
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<PrimordialSandItem>(), Type, 0, 1, 2, 3, 4, 5);

        RegisterItemDrop(ModContent.ItemType<PrimordialSandItem>());
    }
}

public class PrimordialRubble1x1Natural : PrimordialRubble1x1Base
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        TileObjectData.GetTileData(Type, 0).LavaDeath = false;
    }

    public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
    {
        wormChance = 8;
    }
}

public class PrimordialRubble2x1Fake : PrimordialRubble2x1Base
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<PrimordialSandItem>(), Type, 0, 1, 2, 3, 4, 5);

        RegisterItemDrop(ModContent.ItemType<PrimordialSandItem>());
    }
}

public class PrimordialRubble2x1Natural : PrimordialRubble2x1Base
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        TileObjectData.GetTileData(Type, 0).LavaDeath = false;
    }

    public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
    {
        wormChance = 6;
    }
}

public class PrimordialRubble3x2Fake : PrimordialRubble3x2Base
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<PrimordialSandItem>(), Type, 0, 1, 2, 3, 4, 5);

        RegisterItemDrop(ModContent.ItemType<PrimordialSandItem>());
    }
}

public class PrimordialRubble3x2Natural : PrimordialRubble3x2Base
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        TileObjectData.GetTileData(Type, 0).LavaDeath = false;
    }

    public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
    {
        wormChance = 6;
    }
}