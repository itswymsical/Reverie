using Reverie.Content.Tiles.Canopy;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace Reverie.Content.Tiles.Canopy.Trees;

/// <summary>
/// Abstract base class for all custom trees in the mod
/// </summary>
public abstract class TanglewoodTree : ModTile
{
    #region Virtual Properties - Override these in derived classes

    public virtual int FrameWidth => 16;

    public virtual int FrameHeight => 16;

    /// <summary>
    /// Maximum width the tree can span in tiles when fully grown
    /// </summary>
    public virtual int TreeWidth => 1;

    /// <summary>
    /// Minimum height for tree
    /// </summary>
    public virtual int MinHeight => 8;
    /// <summary>
    /// Maximum height the tree can reach in tiles when fully grown
    /// </summary>
    public virtual int MaxHeight => 25;

    /// <summary>
    /// Number of trunk texture variants available
    /// </summary>
    public virtual int TrunkTextureCount => 10;
    public virtual int WoodType => ItemID.Wood;
    private int TreeHeight => WorldGen.genRand.Next(MinHeight, MaxHeight);

    /// <summary>
    /// Valid anchor tiles for tree placement
    /// </summary>
    public virtual int[] ValidAnchorTiles => [
        ModContent.TileType<CanopyGrassTile>(),
        ModContent.TileType<WoodgrassTile>(),
    ];

    #endregion

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileAxe[Type] = true;
        Main.tileBlockLight[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateWidth = FrameWidth - 2;
        TileObjectData.newTile.CoordinateHeights = [FrameHeight - 2];
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, 1, 0);
        TileObjectData.newTile.AnchorValidTiles = ValidAnchorTiles;
        TileObjectData.newTile.AnchorAlternateTiles = [Type];

        TileID.Sets.IsATreeTrunk[Type] = true;
        TileID.Sets.IsShakeable[Type] = true;
        TileID.Sets.GetsDestroyedForMeteors[Type] = true;
        TileID.Sets.GetsCheckedForLeaves[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        AddMapEntry(new Color(151, 107, 75), Language.GetText("MapObject.Tree"));
        DustType = DustID.t_LivingWood;
        HitSound = SoundID.Dig;

        TileObjectData.addTile(Type);
    }

    /// <summary>
    /// Check for highest tile in the tree
    /// </summary>
    public bool IsTreeTop(int i, int j)
    {
        var current = Framing.GetTileSafely(i, j);
        if (!current.HasTile || current.TileType != Type)
            return false;

        var above = Framing.GetTileSafely(i, j - 1);
        return !above.HasTile || above.TileType != Type;
    }

    /// <summary>
    /// Get the horizontal sway for the tree top
    /// </summary>
    public static float GetSway(int i, int j, double factor = 0)
    {
        if (factor == 0)
            factor = Main.GameUpdateCount * Main.windPhysicsStrength;

        return (float)Math.Sin(factor + i * 0.1f + j * 0.01f);
    }

    /// <summary>
    /// Get proper frame coordinates for trunk segment - override for custom framing logic
    /// </summary>
    protected virtual Point GetTrunkFrame(int heightFromBase, int totalHeight)
    {
        int frameX, frameY = 0;

        if (heightFromBase == 0)
        {
            // Root/stump at base
            frameX = TrunkTextureCount - 1; // Last frame for base
        }
        else if (heightFromBase >= totalHeight - 1)
        {
            // Top of tree
            frameX = WorldGen.genRand.Next(TrunkTextureCount - 3, TrunkTextureCount - 1);
        }
        else
        {
            // Middle trunk segments
            frameX = WorldGen.genRand.Next(TrunkTextureCount - 3);
        }

        return new Point(frameX, frameY);
    }

    protected virtual void CreateTree(int i, int j, int height)
    {
        var treeTiles = new List<Point>();

        // Create trunk from bottom to top - straight up (override for curved/branching trees)
        for (var h = 0; h < height; h++)
        {
            var currentY = j - h;

            if (WorldGen.InWorld(i, currentY))
            {
                // Check if there's already a tile at this position
                var existingTile = Framing.GetTileSafely(i, currentY);
                if (existingTile.HasTile && existingTile.TileType == Type)
                    continue;

                WorldGen.PlaceTile(i, currentY, Type, true);
                var tile = Framing.GetTileSafely(i, currentY);

                if (tile.HasTile && tile.TileType == Type)
                {
                    var frame = GetTrunkFrame(h, height);
                    tile.TileFrameX = (short)(frame.X * FrameWidth);
                    tile.TileFrameY = (short)(frame.Y * FrameHeight);

                    treeTiles.Add(new Point(i, currentY));
                }
            }
        }

        // Network sync for placed tiles
        if (Main.netMode != NetmodeID.SinglePlayer && treeTiles.Count > 0)
        {
            var minY = treeTiles.Min(p => p.Y);
            var maxY = treeTiles.Max(p => p.Y);
            NetMessage.SendTileSquare(-1, i, minY, 1, maxY - minY + 1, TileChangeType.None);
        }
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        // Base implementation - override in derived classes for custom drawing
        if (!IsTreeTop(i, j))
            return;

        DrawTreeTop(i, j, spriteBatch);
    }

    protected virtual void DrawTreeTop(int i, int j, SpriteBatch spriteBatch)
    {
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail && WorldGen.genRand.NextBool(8))
        {
            OnShakeTree(i, j);
        }
        if (!fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i * 16, j * 16), i * 16, j * 16, 16, 16, WoodType, Main.rand.Next(1, 3));
        }
    }

    protected virtual void OnShakeTree(int i, int j)
    {
        var drop = GetTreeDrops();
        var position = new Vector2(i, j - 2) * 16;
        var dropType = (int)drop;

        if (dropType > ItemID.None)
            Item.NewItem(null, new Rectangle((int)position.X, (int)position.Y, 16, 16), dropType);

        GrowEffects(i, j);
    }

    protected virtual WeightedRandom<int> GetTreeDrops()
    {
        var drop = new WeightedRandom<int>();
        drop.Add(ItemID.None, 0.7f);
        drop.Add(ItemID.Acorn, 0.2f);
        drop.Add(ItemID.Wood, 0.08f);
        drop.Add(ItemID.LifeCrystal, 0.02f);
        return drop;
    }

    protected virtual void GrowEffects(int i, int j)
    {
        var center = new Vector2(i, j) * 16f + new Vector2(8);

        for (var g = 0; g < 10; g++)
        {
            var leaf = Dust.NewDustDirect(center + Main.rand.NextVector2Unit() * 30f, 0, 0,
                DustID.GrassBlades, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-1f, 1f));
            leaf.fadeIn = 1.2f;
            leaf.scale = 1.1f;
        }
    }

    protected virtual bool CanGrowTree(int i, int j, int height)
    {
        // Find ground level
        var groundY = j;
        for (var check = 0; check < 10; check++)
        {
            var tile = Framing.GetTileSafely(i, groundY + 1);
            if (WorldGen.SolidOrSlopedTile(tile))
                break;
            groundY++;
        }

        // Check vertical clearance
        for (var checkY = groundY - height; checkY < groundY; checkY++)
        {
            if (!WorldGen.InWorld(i, checkY))
                return false;

            // Check area around trunk for obstructions
            for (var checkX = i - TreeWidth / 2; checkX <= i + TreeWidth / 2; checkX++)
            {
                var tile = Framing.GetTileSafely(checkX, checkY);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    return false;
            }
        }

        return true;
    }

    public bool GrowTree(int i, int j)
    {
        var height = TreeHeight;

        if (!CanGrowTree(i, j, height))
            return false;

        // Find actual ground level
        var groundY = j;
        for (var check = 0; check < 10; check++)
        {
            var tile = Framing.GetTileSafely(i, groundY + 1);
            if (WorldGen.SolidOrSlopedTile(tile))
                break;
            groundY++;
        }

        // Clear any existing tiles at the base
        WorldGen.KillTile(i, groundY);

        CreateTree(i, groundY, height);

        if (WorldGen.PlayerLOS(i, groundY))
            GrowEffects(i, groundY);

        return true;
    }
}