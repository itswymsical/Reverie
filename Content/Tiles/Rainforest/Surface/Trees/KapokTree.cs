using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace Reverie.Content.Tiles.Rainforest.Surface.Trees;

/// <summary>
/// Fixed Kapok Tree implementation with proper trunk framing and curve logic
/// </summary>
public class KapokTree : ModTile
{
    public const int FrameSize = 22;

    // Tree configuration - Fixed to be more reasonable
    public virtual int TreeHeight => WorldGen.genRand.Next(20, 45); // More reasonable range
    public virtual int MaxCurveDistance => 3; // Reduced curve for better connection

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileAxe[Type] = true;
        Main.tileBlockLight[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateWidth = FrameSize - 2;
        TileObjectData.newTile.CoordinateHeights = [FrameSize - 2];
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, 1, 0);
        TileObjectData.newTile.AnchorValidTiles = [
            ModContent.TileType<CanopyGrassTile>(),
            ModContent.TileType<WoodgrassTile>(),
            ModContent.TileType<OxisolTile>()
        ];
        TileObjectData.newTile.AnchorAlternateTiles = [Type];

        TileID.Sets.IsATreeTrunk[Type] = true;
        TileID.Sets.IsShakeable[Type] = true;
        TileID.Sets.GetsDestroyedForMeteors[Type] = true;
        TileID.Sets.GetsCheckedForLeaves[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        AddMapEntry(new Color(101, 142, 44), Language.GetText("MapObject.Tree"));
        DustType = DustID.RichMahogany;
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
            factor = Main.GameUpdateCount * 0.01;

        return (float)Math.Sin(factor + i * 0.1f + j * 0.05f) * 0.3f;
    }

    /// <summary>
    /// Get proper frame coordinates for trunk segment
    /// </summary>
    private Point GetTrunkFrame(int heightFromBase, int totalHeight)
    {
        int frameX, frameY = 0;

        if (heightFromBase == 0)
        {
            // Root/stump at base - use frame 3
            frameX = 3;
        }
        else if (heightFromBase >= totalHeight - 1)
        {
            // Top of tree - use end textures (frames 4+)
            frameX = WorldGen.genRand.Next(4, 11); // Assuming frames 4-10 are end textures
        }
        else
        {
            // Middle trunk - use middle trunk textures (frames 0-2)
            frameX = WorldGen.genRand.Next(3); // 0-2
        }

        return new Point(frameX, frameY);
    }

    /// <summary>
    /// Create the tree structure with proper framing
    /// </summary>
    protected virtual void CreateTree(int i, int j, int height)
    {
        List<Point> treeTiles = new List<Point>(); // Track placed tiles

        // Create trunk from bottom to top - straight up
        for (int h = 0; h < height; h++)
        {
            int currentY = j - h;

            if (WorldGen.InWorld(i, currentY))
            {
                // Check if there's already a tile at this position (prevent overlap)
                var existingTile = Framing.GetTileSafely(i, currentY);
                if (existingTile.HasTile && existingTile.TileType == Type)
                    continue; // Skip if already placed

                WorldGen.PlaceTile(i, currentY, Type, true);
                var tile = Framing.GetTileSafely(i, currentY);

                if (tile.HasTile && tile.TileType == Type)
                {
                    // Get proper frame for this trunk segment
                    Point frame = GetTrunkFrame(h, height);
                    tile.TileFrameX = (short)(frame.X * FrameSize);
                    tile.TileFrameY = (short)(frame.Y * FrameSize);

                    treeTiles.Add(new Point(i, currentY));
                }
            }
        }

        // Network sync for placed tiles
        if (Main.netMode != NetmodeID.SinglePlayer && treeTiles.Count > 0)
        {
            // Find bounds of placed tiles
            int minY = treeTiles.Min(p => p.Y);
            int maxY = treeTiles.Max(p => p.Y);

            NetMessage.SendTileSquare(-1, i, minY, 1, maxY - minY + 1, TileChangeType.None);
        }
    }

    /// <summary>
    /// Custom drawing for tree parts
    /// </summary>
    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (!IsTreeTop(i, j))
            return;

        var texture = ModContent.Request<Texture2D>(Texture).Value;
        var position = new Vector2(i, j) * 16 - Main.screenPosition;

        float sway = GetSway(i, j);
        float rotation = sway * 0.13f;

        var canopySource = new Rectangle(0, 32, 324, 294);
        var canopyOrigin = new Vector2(canopySource.Width / 2, canopySource.Height / 2);

        Vector2 canopyPosition = position + new Vector2(218, 68); // Center on tile

        spriteBatch.Draw(texture, canopyPosition, canopySource, Lighting.GetColor(i, j), rotation, canopyOrigin, 1f, SpriteEffects.None, 0f);
    }

    /// <summary>
    /// Handle tree destruction and shaking
    /// </summary>
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail && WorldGen.genRand.NextBool(3))
        {
            OnShakeTree(i, j);
        }
    }

    protected virtual void OnShakeTree(int i, int j)
    {
        var drop = new WeightedRandom<int>();

        drop.Add(ItemID.None, 0.7f);
        drop.Add(ItemID.Acorn, 0.2f);
        drop.Add(ItemID.Wood, 0.08f);
        drop.Add(ItemID.LifeCrystal, 0.02f);

        var position = new Vector2(i, j - 2) * 16;
        int dropType = (int)drop;
        if (dropType > ItemID.None)
            Item.NewItem(null, new Rectangle((int)position.X, (int)position.Y, 16, 16), dropType);

        GrowEffects(i, j);
    }

    protected virtual void GrowEffects(int i, int j)
    {
        var center = new Vector2(i, j) * 16f + new Vector2(8);

        for (int g = 0; g < 10; g++)
        {
            var leaf = Dust.NewDustDirect(center + Main.rand.NextVector2Unit() * 30f, 0, 0,
                DustID.GrassBlades, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-1f, 1f));
            leaf.fadeIn = 1.2f;
            leaf.scale = 1.1f;
        }
    }

    /// <summary>
    /// Improved tree growth method
    /// </summary>
    public static bool GrowKapokTree(int i, int j)
    {
        // Find ground level more carefully
        int groundY = j;
        for (int check = 0; check < 10; check++)
        {
            var tile = Framing.GetTileSafely(i, groundY + 1);
            if (WorldGen.SolidOrSlopedTile(tile))
                break;
            groundY++;
        }

        var instance = ModContent.GetInstance<KapokTree>();
        var height = instance.TreeHeight;

        // More lenient area checking - just check the general area
        bool canPlace = true;

        // Check basic vertical clearance
        for (int checkY = groundY - height; checkY < groundY; checkY++)
        {
            if (!WorldGen.InWorld(i, checkY))
            {
                canPlace = false;
                break;
            }

            // Check a small area around the trunk line for major obstructions
            for (int checkX = i - 2; checkX <= i + 2; checkX++)
            {
                var tile = Framing.GetTileSafely(checkX, checkY);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    canPlace = false;
                    break;
                }
            }

            if (!canPlace) break;
        }

        if (canPlace)
        {
            // Clear any existing tiles at the base
            WorldGen.KillTile(i, groundY);

            instance.CreateTree(i, groundY, height);

            if (WorldGen.PlayerLOS(i, groundY))
                instance.GrowEffects(i, groundY);

            return true;
        }

        return false;
    }
}