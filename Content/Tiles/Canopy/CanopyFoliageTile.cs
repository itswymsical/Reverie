using Reverie.Content.Items.Botany;
using Reverie.Content.Tiles.TemperateForest;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Canopy;

public class CanopyFoliageTile : ModTile
{
    public override string Texture => $"Terraria/Images/Tiles_{TileID.JunglePlants2}";
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
            ModContent.TileType<RainforestGrassTile>(),
            Type
        ];

        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.WaterDeath = false;

        TileObjectData.newTile.StyleHorizontal = true;

        TileObjectData.newTile.CoordinateHeights = [36];
        TileObjectData.newTile.DrawYOffset = -14;
        TileObjectData.addTile(Type);

        DustType = DustID.JungleGrass;
        HitSound = SoundID.Grass;

        AddMapEntry(new Color(93, 106, 33));
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

public class CanopyFernTile : ModTile
{
    public override void SetStaticDefaults()
    {
        const int height = 26;

        Main.tileSolid[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.SettingsEnabled_TilesSwayInWind = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.BreakableWhenPlacing[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);

        TileObjectData.newTile.CoordinateWidth = 34;
        TileObjectData.newTile.CoordinateHeights = [height];

        TileObjectData.newTile.DrawYOffset = -(height - 18 - 2);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);

        TileObjectData.newTile.AnchorValidTiles =
        [
            ModContent.TileType<RainforestGrassTile>(),
        ];

        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(93, 106, 33));
        DustType = DustID.JungleGrass;
        HitSound = SoundID.Grass;

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
}
