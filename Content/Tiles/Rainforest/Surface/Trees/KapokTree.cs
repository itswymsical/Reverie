using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.UI;

namespace Reverie.Content.Tiles.Rainforest.Surface.Trees;

public class KapokTree : CustomTree
{
    // Tree configuration
    public override int MinHeight => 32;
    public override int MaxHeight => 58;
    public override int CanopyStartOffset => 4;
    public override bool UsesPalmTreeFraming => true; // Use palm tree framing style
    public override TreeTypes TreeType => TreeTypes.Palm;

    public override int[] ValidAnchorTiles => [
        ModContent.TileType<CanopyGrassTile>(),
        ModContent.TileType<WoodgrassTile>(),
        ModContent.TileType<OxisolTile>()
    ];

    public override int GetTreeWidth() => 1; // Kapok trees are 2 tiles wide

    protected override void ConfigureTreeSettings()
    {
        AddMapEntry(new Color(101, 142, 44), Language.GetText("MapObject.Tree"));
        DustType = DustID.RichMahogany;
        HitSound = SoundID.Dig;
    }

    protected override int GetEnvironmentalHeightModifier(int x, int y)
    {
        var modifier = 0;

        // Height increases near water
        if (IsNearWater(x, y, 20))
            modifier += 3;

        // Height decreases if crowded
        if (HasNearbyTallTrees(x, y, 15))
            modifier -= 2;

        // Height increases in deeper areas
        if (y > Main.worldSurface + 50)
            modifier += 1;

        return modifier;
    }

    protected override int GetMaxHeightBonus()
    {
        return 10; // Ancient Kapoks can be significantly taller
    }

    protected override bool ShouldUseAlternateFrames(int x, int y)
    {
        // Use alternate frames for trees grown on Woodgrass (oasis variant)
        var anchor = GetAnchorPosition(x, y, Type);
        var anchorTile = Framing.GetTileSafely(anchor.X, anchor.Y + 1);
        return anchorTile.TileType == ModContent.TileType<WoodgrassTile>();
    }

    protected override void PlaceCanopy(int i, int j, int height)
    {
        var canopyX = i;
        var canopyY = j - height;

        Main.NewText($"Trying to place canopy at ({canopyX}, {canopyY})", Color.Cyan);

        if (WorldGen.InWorld(canopyX, canopyY))
        {
            // Place the canopy tile
            bool placed = WorldGen.PlaceTile(canopyX, canopyY, ModContent.TileType<KapokCanopyTile>(), true);
            Main.NewText($"Tile placed: {placed}", placed ? Color.Green : Color.Red);

            if (placed)
            {
                // Manually create tile entity since PlaceInWorld might not be called
                var entity = new KapokCanopyEntity();
                int entityID = entity.Place(canopyX, canopyY);
                Main.NewText($"Entity ID: {entityID}", entityID != -1 ? Color.Green : Color.Red);

                if (entityID != -1)
                {
                    entity.canopyStyle = WorldGen.genRand.Next(3);
                    entity.treeHeight = height;
                    Main.NewText($"Entity configured: style {entity.canopyStyle}", Color.Yellow);
                }
            }

            // Network sync
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendTileSquare(-1, canopyX, canopyY, 1);
            }
        }
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!fail)
        {
            // If this is the base of the tree, find and remove the canopy
            var relative = GetRelativePosition(i, j);
            if (relative.Y == 0) // Base tile
            {
                var treeHeight = GetTreeHeightAt(i, j, Type);
                var canopyPos = new Point(i, j - treeHeight);

                if (WorldGen.InWorld(canopyPos.X, canopyPos.Y))
                {
                    var canopyTile = Framing.GetTileSafely(canopyPos.X, canopyPos.Y);
                    if (canopyTile.HasTile && canopyTile.TileType == ModContent.TileType<KapokCanopyTile>())
                    {
                        WorldGen.KillTile(canopyPos.X, canopyPos.Y);
                    }
                }
            }
        }

        base.KillTile(i, j, ref fail, ref effectOnly, ref noItem);
    }

    public override bool ShakeTree(int i, int j, ref bool createLeaves)
    {
        var treeHeight = GetTreeHeightAt(i, j, Type);

        if (WorldGen.genRand.NextBool(10))
        {
            int dropType = ItemID.Acorn; // Default drop

            // Taller trees have better drops
            if (treeHeight > 20 && WorldGen.genRand.NextBool(3))
                dropType = ItemID.Wood; // TODO: Replace with Kapok Wood

            // Ancient Kapoks drop rare items
            if (treeHeight > 25 && WorldGen.genRand.NextBool(20))
                dropType = ItemID.LifeCrystal; // TODO: Replace with Kapok Fruit

            Item.NewItem(null, new Rectangle(i * 16, j * 16, 16, 16), dropType);
        }

        createLeaves = true;
        return true;
    }

    // Utility methods specific to Kapok trees
    private static bool IsNearWater(int x, int y, int range)
    {
        for (var checkX = x - range; checkX <= x + range; checkX++)
        {
            for (var checkY = y - range; checkY <= y + range; checkY++)
            {
                if (WorldGen.InWorld(checkX, checkY))
                {
                    var tile = Framing.GetTileSafely(checkX, checkY);
                    if (tile.LiquidAmount > 0) return true;
                }
            }
        }
        return false;
    }

    private static bool HasNearbyTallTrees(int x, int y, int range)
    {
        for (var checkX = x - range; checkX <= x + range; checkX++)
        {
            for (var checkY = y - range; checkY <= y + range; checkY++)
            {
                if (WorldGen.InWorld(checkX, checkY))
                {
                    var tile = Framing.GetTileSafely(checkX, checkY);
                    if (tile.HasTile && tile.TileType == ModContent.TileType<KapokTree>())
                    {
                        var height = GetTreeHeightAt(checkX, checkY, ModContent.TileType<KapokTree>());
                        if (height > 20) return true;
                    }
                }
            }
        }
        return false;
    }
}