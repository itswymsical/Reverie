using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Canopy;

public class TeakWoodSaplingTile : ModTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileID.Sets.TreeSapling[Type] = true;
        TileID.Sets.CommonSapling[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);

        TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;

        TileObjectData.newTile.StyleHorizontal = true;

        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.StyleMultiplier = 3;

        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 2;

        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.newTile.CoordinateHeights = new[]
        {
            16,
            18
        };

        TileObjectData.newTile.Origin = new Point16(0, 1);

        TileObjectData.newTile.LavaDeath = true;

        TileObjectData.newTile.DrawFlipHorizontal = true;
        TileObjectData.newTile.UsesCustomCanPlace = true;

        TileObjectData.newTile.AnchorValidTiles = new[]
        {
            ModContent.TileType<LoamGrassTile>()
        };

        TileObjectData.addTile(Type);

        AdjTiles = [TileID.Saplings];

        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Sapling"));
    }
}