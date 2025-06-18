using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Rainforest.Surface.Trees;

public abstract class CustomTree : ModTile
{
    public const int FRAME_SIZE = 22;

    // Static collections for managing tree instances
    private static readonly HashSet<Point16> DrawPoints = [];
    private static readonly Dictionary<Point, Point16> AnchorCache = [];
    private static readonly Dictionary<Point, int> HeightCache = [];

    // Abstract properties that derived classes must implement
    public abstract int MinHeight { get; }
    public abstract int MaxHeight { get; }
    public abstract int[] ValidAnchorTiles { get; }

    // Virtual properties with default implementations
    public virtual int CanopyStartOffset => 4;
    public virtual bool UsesPalmTreeFraming => false;
    public virtual TreeTypes TreeType => TreeTypes.Custom;

    #region Static Growth System

    /// <summary>
    /// Attempts to grow a custom tree of type T at the given coordinates
    /// </summary>
    public static bool GrowTree<T>(int i, int j) where T : CustomTree
    {
        while (!WorldGen.SolidOrSlopedTile(Framing.GetTileSafely(i, j + 1)))
            j++; // Find the ground

        var instance = ModContent.GetInstance<T>();
        return GrowTreeFromInstance(i, j, instance);
    }

    private static bool GrowTreeFromInstance(int i, int j, CustomTree instance)
    {
        var height = instance.DetermineTreeHeight(i, j, false);

        if (WorldGen.InWorld(i, j) && AreaClear(i, j - (height - 1), instance.GetTreeWidth(), height))
        {
            WorldGen.KillTile(i, j); // Kill the sapling
            instance.CreateTree(i, j, height);

            if (WorldGen.PlayerLOS(i, j))
                instance.GrowEffects(i, j, height);

            return true;
        }

        return false;
    }

    private static bool AreaClear(int x, int y, int width, int height)
    {
        for (var i = x; i < x + width; i++)
        {
            for (var j = y; j < y + height; j++)
            {
                if (!WorldGen.InWorld(i, j)) return false;

                var tile = Framing.GetTileSafely(i, j);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    return false;
            }
        }
        return true;
    }

    #endregion

    #region Tree Data Management

    /// <summary>
    /// Gets the anchor (base) position of a tree tile
    /// </summary>
    public static Point16 GetAnchorPosition(int x, int y, int treeType)
    {
        var cacheKey = new Point(x, y);
        if (AnchorCache.ContainsKey(cacheKey))
            return AnchorCache[cacheKey];

        // Find the base of the tree by going down
        var anchorY = y;
        while (anchorY < Main.maxTilesY - 1)
        {
            var current = Framing.GetTileSafely(x, anchorY);
            var below = Framing.GetTileSafely(x, anchorY + 1);

            // Stop when we find the anchor (tree tile above non-tree tile)
            if (current.HasTile && current.TileType == treeType)
            {
                if (!below.HasTile || below.TileType != treeType)
                {
                    break; // Found the anchor
                }
            }
            anchorY++;
        }

        // Find the leftmost tile of the tree at this level
        var anchorX = x;
        while (anchorX > 0)
        {
            var left = Framing.GetTileSafely(anchorX - 1, anchorY);
            if (!left.HasTile || left.TileType != treeType)
                break;
            anchorX--;
        }

        var result = new Point16(anchorX, anchorY);
        AnchorCache[cacheKey] = result;
        return result;
    }

    /// <summary>
    /// Gets the total height of a tree at given coordinates
    /// </summary>
    public static int GetTreeHeightAt(int x, int y, int treeType)
    {
        var cacheKey = new Point(x, y);
        if (HeightCache.ContainsKey(cacheKey))
            return HeightCache[cacheKey];

        // Find the base of the tree
        var anchor = GetAnchorPosition(x, y, treeType);

        // Count upward to find total height
        var height = 0;
        int checkY = anchor.Y;
        while (checkY >= 0)
        {
            var tile = Framing.GetTileSafely(anchor.X, checkY);
            if (!tile.HasTile || tile.TileType != treeType)
                break;

            height++;
            checkY--;
        }

        HeightCache[cacheKey] = height;
        return height;
    }

    /// <summary>
    /// Gets the relative position of a tile within its tree structure
    /// </summary>
    public Point GetRelativePosition(int x, int y)
    {
        var anchor = GetAnchorPosition(x, y, Type);
        return new Point(x - anchor.X, anchor.Y - y); // Note: Y is flipped
    }

    #endregion

    #region Tile Setup and Configuration

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileAxe[Type] = true;
        Main.tileBlockLight[Type] = true; // Allow custom light handling

        // Basic tile object data
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateWidth = FRAME_SIZE - 2;
        TileObjectData.newTile.CoordinateHeights = [FRAME_SIZE - 2];
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.StyleMultiplier = 3;
        TileObjectData.newTile.StyleWrapLimit = 3 * 4;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, 1, 0);
        TileObjectData.newTile.AnchorValidTiles = ValidAnchorTiles;
        TileObjectData.newTile.AnchorAlternateTiles = [Type];

        TileID.Sets.IsATreeTrunk[Type] = true;
        TileID.Sets.IsShakeable[Type] = true;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;
        TileID.Sets.GetsDestroyedForMeteors[Type] = true;
        TileID.Sets.GetsCheckedForLeaves[Type] = true;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        // Let derived classes configure specific settings
        ConfigureTreeSettings();

        TileObjectData.addTile(Type);
    }

    /// <summary>
    /// Override this to configure tree-specific settings
    /// </summary>
    protected virtual void ConfigureTreeSettings()
    {
        AddMapEntry(new Color(101, 142, 44), Language.GetText("MapObject.Tree"));
        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    #endregion

    #region Height and Growth Logic

    /// <summary>
    /// Determines the height of this tree based on various factors
    /// </summary>
    public virtual int DetermineTreeHeight(int x, int y, bool isWorldGen)
    {
        var baseHeight = WorldGen.genRand.Next(MinHeight, MaxHeight + 1);

        if (isWorldGen)
        {
            // World generation can create more varied heights
            baseHeight += GetWorldGenHeightModifier(x, y);
        }

        // Apply environmental modifiers
        baseHeight += GetEnvironmentalHeightModifier(x, y);

        return Math.Clamp(baseHeight, MinHeight, MaxHeight + GetMaxHeightBonus());
    }

    protected virtual int GetWorldGenHeightModifier(int x, int y)
    {
        // Default implementation: sine wave variation
        var variation = (float)Math.Sin(x * 0.01f) * 3f;
        return (int)variation;
    }

    protected virtual int GetEnvironmentalHeightModifier(int x, int y)
    {
        return 0; // Override in derived classes for specific behavior
    }

    protected virtual int GetMaxHeightBonus()
    {
        return 5; // Allow trees to grow up to 5 tiles taller than normal max
    }

    #endregion

    #region Tree Creation and Structure

    /// <summary>
    /// Gets the width of this tree type (default 1 tile)
    /// </summary>
    public virtual int GetTreeWidth() => 1;

    /// <summary>
    /// Creates the tree structure at the given location
    /// </summary>
    protected virtual void CreateTree(int i, int j, int height)
    {
        var treeWidth = GetTreeWidth();

        for (var h = 0; h < height; h++)
        {
            for (var w = 0; w < treeWidth; w++)
            {
                var x = i + w;
                var y = j - h;

                if (WorldGen.InWorld(x, y))
                {
                    WorldGen.PlaceTile(x, y, Type, true);
                    var tile = Framing.GetTileSafely(x, y);

                    if (tile.HasTile && tile.TileType == Type)
                    {
                        SetTreeFraming(x, y, w, h, height, treeWidth);
                    }
                }
            }
        }

        PlaceCanopy(i, j, height);

        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendTileSquare(-1, i, j + 1 - height, treeWidth, height, TileChangeType.None);
    }

    /// <summary>
    /// Sets the framing for a tree tile. Override for custom framing logic.
    /// </summary>
    protected virtual void SetTreeFraming(int x, int y, int relativeX, int relativeY, int totalHeight, int treeWidth)
    {
        var tile = Framing.GetTileSafely(x, y);

        if (UsesPalmTreeFraming)
        {
            // Palm tree style framing
            SetPalmTreeFraming(tile, x, y, relativeX, relativeY, totalHeight, treeWidth);
        }
        else
        {
            // Standard tree framing
            SetStandardTreeFraming(tile, x, y, relativeX, relativeY, totalHeight, treeWidth);
        }
    }

    private void SetPalmTreeFraming(Tile tile, int x, int y, int relativeX, int relativeY, int totalHeight, int treeWidth)
    {
        var frameX = (short)(WorldGen.genRand.Next(3) * FRAME_SIZE);

        if (ShouldUseAlternateFrames(x, y))
        {
            frameX += (short)(3 * FRAME_SIZE);
        }

        // Use row 0 for the base, row 1 for everything else (the trunk).
        short frameY = (relativeY == 0) ? (short)0 : (short)(1 * FRAME_SIZE);

        tile.TileFrameX = frameX;
        tile.TileFrameY = frameY;
    }

    protected virtual void PlaceCanopy(int i, int j, int height) { }

    private void SetStandardTreeFraming(Tile tile, int x, int y, int relativeX, int relativeY, int totalHeight, int treeWidth)
    {
        // Standard tree framing logic
        var frameX = WorldGen.genRand.Next(3) * FRAME_SIZE; // Random trunk variation
        var frameY = 0;

        // Different frames for different parts of the tree
        if (relativeY == 0) // Base
            frameX = WorldGen.genRand.Next(0, 3) * FRAME_SIZE;
        else if (relativeY >= totalHeight - CanopyStartOffset) // Canopy
            frameX = WorldGen.genRand.Next(3, 6) * FRAME_SIZE;
        else // Trunk
            frameX = WorldGen.genRand.Next(0, 3) * FRAME_SIZE;

        tile.TileFrameX = (short)frameX;
        tile.TileFrameY = (short)frameY;
    }

    /// <summary>
    /// Determines if alternate frames should be used (for palm tree style variants)
    /// </summary>
    protected virtual bool ShouldUseAlternateFrames(int x, int y)
    {
        return false; // Override in derived classes
    }

    #endregion

    #region Effects and Behavior

    /// <summary>
    /// Called when the tree grows, override for custom effects
    /// </summary>
    protected virtual void GrowEffects(int i, int j, int height)
    {
        // Default: create some leaf particles
        for (var h = 0; h < Math.Min(height, 5); h++)
        {
            var center = new Vector2(i, j - h) * 16f + new Vector2(8);

            for (var g = 0; g < 3; g++)
            {
                var leaf = Dust.NewDustDirect(center, 16, 16, DustID.GrassBlades,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-1f, 1f));
                leaf.fadeIn = 1.2f;
                leaf.scale = 1.1f;
            }
        }
    }

    /// <summary>
    /// Override for custom light modification
    /// </summary>
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        var relative = GetRelativePosition(i, j);
        var treeHeight = GetTreeHeightAt(i, j, Type);

        // Default: canopy blocks some light
        if (relative.Y <= CanopyStartOffset && treeHeight > MinHeight + 3)
        {
            r *= 0.6f;
            g *= 0.6f;
            b *= 0.6f;
        }
    }

    /// <summary>
    /// Override for custom shake behavior
    /// </summary>
    public virtual bool ShakeTree(int i, int j, ref bool createLeaves)
    {
        createLeaves = true;
        return true; // Allow vanilla drops
    }

    #endregion

    #region Tile Behavior Overrides

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!fail)
        {
            // Tree was successfully destroyed
            var relative = GetRelativePosition(i, j);
            if (relative.Y > 0) // Not the base tile
            {
                // Mark tile below as "chopped" if it's part of the same tree
                var below = Framing.GetTileSafely(i, j + 1);
                if (below.HasTile && below.TileType == Type)
                {
                    below.TileFrameX = (short)(WorldGen.genRand.Next(9, 12) * FRAME_SIZE);
                }
            }
        }
        else
        {
            // Tree resisted damage, shake it
            var leaves = false;
            ShakeTree(i, j, ref leaves);
        }
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        // Custom trees handle their own framing
        return false;
    }

    public override void RandomUpdate(int i, int j)
    {
        // Override for custom random effects (falling leaves, fruit drops, etc.)
        if (Main.rand.NextBool(200))
        {
            var relative = GetRelativePosition(i, j);
            if (relative.Y <= CanopyStartOffset)
            {
                // Create falling leaf effect
                var position = new Vector2(i * 16 + 8, j * 16 + 16);
                var leaf = Dust.NewDustDirect(position, 0, 0, DustID.GrassBlades, 0f, 1f);
                leaf.velocity.Y = 0.5f;
                leaf.velocity.X = Main.rand.NextFloat(-0.5f, 0.5f);
                leaf.fadeIn = 1.2f;
            }
        }
    }

    #endregion

    #region Static Utility Methods

    /// <summary>
    /// Clears all cached data (call after world generation)
    /// </summary>
    public static void ClearCache()
    {
        AnchorCache.Clear();
        HeightCache.Clear();
        DrawPoints.Clear();
    }

    /// <summary>
    /// Checks if a location is suitable for tree growth
    /// </summary>
    public static bool CanGrowAt(int x, int y, int width, int height, int[] validAnchors)
    {
        // Check anchor tile
        var anchor = Framing.GetTileSafely(x, y + 1);
        if (!anchor.HasTile || !validAnchors.Contains(anchor.TileType))
            return false;

        // Check clearance
        return AreaClear(x, y - (height - 1), width, height);
    }

    #endregion
}
