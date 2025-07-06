// Credits: Original code by Spirit Reforged (GabeHasWon)
using Reverie.Content.Tiles.Canopy.Trees;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Reverie.Common.Tiles;

/// <summary> Applies the effects of fertilizer to <see cref="TanglewoodTree"/> saplings. </summary>
internal class FertilizerGlobalProjectile : GlobalProjectile
{
    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.type == ProjectileID.Fertilizer;

    public override void AI(Projectile projectile)
    {
        Point start = projectile.TopLeft.ToTileCoordinates();
        Point end = projectile.BottomRight.ToTileCoordinates();

        for (int x = start.X; x < end.X + 1; x++)
        {
            for (int y = start.Y; y < end.Y + 1; y++)
            {
                if (!WorldGen.InWorld(x, y))
                    continue;

                var t = Main.tile[x, y];

                if (TileLoader.GetTile(t.TileType) is SaplingTile)
                {
                    //TanglewoodTree.GrowTree(x, y);
                    WorldGen.GrowTree(x, y);
                }
            }
        }
    }
}
public abstract class SaplingTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawFlipHorizontal = true;
        TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.StyleMultiplier = 3;
        PreAddObjectData();
        TileObjectData.addTile(Type);

        TileID.Sets.TreeSapling[Type] = true; //Will break on tile update if this is true
        TileID.Sets.CommonSapling[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        AddMapEntry(new Color(170, 120, 100), Language.GetText("MapObject.Sapling"));

        DustType = DustID.WoodFurniture;
        AdjTiles = [TileID.Saplings];
    }

    /// <summary> Called before <see cref="TileObjectData.addTile"/> is automatically called in <see cref="SetStaticDefaults"/>.<br/>
    /// Use this to modify object data without needing to override SetStaticDefaults.<para/>
    /// <see cref="TileObjectData.AnchorValidTiles"/> must be set here for the sapling to work. </summary>
    public abstract void PreAddObjectData();
    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override void RandomUpdate(int i, int j)
    {
        if (Main.rand.NextBool(8) && WorldGen.GrowTree(i, j) && WorldGen.PlayerLOS(i, j))
            WorldGen.TreeGrowFXCheck(i, j);
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects)
    {
        if (i % 2 == 0)
            effects = SpriteEffects.FlipHorizontally;
    }
}