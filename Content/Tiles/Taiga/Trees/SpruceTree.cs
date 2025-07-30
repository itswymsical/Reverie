using Reverie.Content.Tiles.Canopy;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Utilities;
using Terraria.GameContent.Drawing;
using Terraria.GameContent;
using Reverie.Core.Tiles;

namespace Reverie.Content.Tiles.Taiga.Trees;

public class SpruceTree : CustomTree
{
    #region Properties
    public override int FrameWidth => 22;
    public override int FrameHeight => 22;
    public override int TreeWidth => 1;
    public override int MaxHeight => 30;
    public override int MinHeight => 12;
    public override int WoodType => ItemID.BorealWood;
    #endregion

    public override int[] ValidAnchorTiles => [
        ModContent.TileType<SnowTaigaGrassTile>(),
        ModContent.TileType<TaigaGrassTile>(),
        ModContent.TileType<CorruptTaigaGrassTile>(),
        ModContent.TileType<CrimsonTaigaGrassTile>(),
        ModContent.TileType<HallowTaigaGrassTile>(),
    ];

    private enum TrunkType
    {
        Large,
        Medium,
        Small
    }

    private TrunkType GetTrunkTypeForHeight(int heightFromBase, int totalHeight)
    {
        if (totalHeight <= 1) return TrunkType.Large;

        float progress = (float)(heightFromBase - 1) / Math.Max(1, totalHeight - 2);

        if (progress < 0.4f) return TrunkType.Large;
        if (progress < 0.75f) return TrunkType.Medium;
        return TrunkType.Small;
    }

    protected override Point GetTrunkFrame(int heightFromBase, int totalHeight)
    {
        int frameX = 0, frameY = 0;
        int variant = WorldGen.genRand.Next(3);

        if (heightFromBase == 0)
        {
            // Center stump with both roots as default during creation
            frameX = 3 + variant;
            frameY = 4;
        }
        else
        {
            var trunkType = GetTrunkTypeForHeight(heightFromBase, totalHeight);

            switch (trunkType)
            {
                case TrunkType.Large:
                    frameX = variant;
                    frameY = 2;
                    break;

                case TrunkType.Medium:
                    frameX = variant;
                    frameY = 1;
                    break;

                case TrunkType.Small:
                    frameX = 3 + variant;
                    frameY = 1;
                    break;
            }
        }

        return new Point(frameX, frameY);
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
            return;

        int frameX = tile.TileFrameX / FrameWidth;
        int frameY = tile.TileFrameY / FrameHeight;

        TrunkType? trunkType = null;

        if (frameY == 2 && frameX < 3)
            trunkType = TrunkType.Large;
        else if (frameY == 1 && frameX < 3)
            trunkType = TrunkType.Medium;
        else if (frameY == 1 && frameX >= 3 && frameX < 6)
            trunkType = TrunkType.Small;

        // Check if we're too close to the base
        if (trunkType.HasValue && (i + j) % 2 == 0 && !IsTooCloseToBase(i, j))
        {
            DrawFoliage(i, j, spriteBatch, trunkType.Value);
        }
    }

    private bool IsTooCloseToBase(int i, int j)
    {
        var below = Framing.GetTileSafely(i, j + 1);
        if (!below.HasTile || below.TileType != Type)
            return false;

        int belowFrameX = below.TileFrameX / FrameWidth;
        int belowFrameY = below.TileFrameY / FrameHeight;

        bool isCenterStump = belowFrameY >= 2 && belowFrameY <= 5 && belowFrameX >= 3;

        bool isRootStump = belowFrameY >= 5;

        return isCenterStump || isRootStump;
    }

    private void DrawFoliage(int i, int j, SpriteBatch spriteBatch, TrunkType trunkType)
    {
        var tile = Framing.GetTileSafely(i, j);

        Color color = Lighting.GetColor(i, j);

        if (!TileDrawing.IsVisible(tile))
            return;

        float rotation = GetSway(i, j) * .2f;

        var tileCenterScreen = new Vector2(i * 16 + 8, j * 16 + 8) - Main.screenPosition;

        var samplerState = Main.graphics.GraphicsDevice.SamplerStates[0];
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
            DepthStencilState.None, Main.Rasterizer);

        Texture2D foliageTexture;
        int leftOffset, rightOffset;

        int variant = (i + j) % 3;

        switch (trunkType)
        {
            case TrunkType.Large:
                foliageTexture = ModContent.Request<Texture2D>($"Reverie/Content/Tiles/Taiga/Trees/BranchesLarge_{variant}").Value;
                leftOffset = -4;
                rightOffset = 72;
                break;

            case TrunkType.Medium:
                foliageTexture = ModContent.Request<Texture2D>($"Reverie/Content/Tiles/Taiga/Trees/BranchesMedium_{variant}").Value;
                leftOffset = 12;
                rightOffset = 72;
                break;

            case TrunkType.Small:
                foliageTexture = ModContent.Request<Texture2D>($"Reverie/Content/Tiles/Taiga/Trees/BranchesSmall_{variant}").Value;
                leftOffset = 36;
                rightOffset = 70;
                break;

            default:
                foliageTexture = ModContent.Request<Texture2D>("Reverie/Content/Tiles/Taiga/Trees/BranchesSmall_0").Value;
                leftOffset = 36;
                rightOffset = 70;
                break;
        }

        // idk how or why this works but DO NOT TAMPER WITH IT!!!!
        int width = Main.screenWidth / 16;
        int height = (Main.screenHeight / 16) + 118;

        var leftPos = tileCenterScreen + new Vector2(width + leftOffset, height);
        spriteBatch.Draw(foliageTexture, leftPos, null, color, rotation, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        var rightPos = tileCenterScreen + new Vector2(width + rightOffset, height);
        spriteBatch.Draw(foliageTexture, rightPos, null, color, -rotation, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState,
            DepthStencilState.None, Main.Rasterizer, null);
    }


    private static readonly HashSet<Point> ValidatingTiles = new();

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
            return false;

        var pos = new Point(i, j);

        // Prevent recursive validation
        if (ValidatingTiles.Contains(pos))
            return false;

        ValidatingTiles.Add(pos);

        try
        {
            // Check if tile needs to break
            if (!ValidateAnchor(i, j))
            {
                WorldGen.KillTile(i, j);
                return false;
            }

            // Update frame for soft-cut
            UpdateFrameForSoftCut(i, j);
        }
        finally
        {
            ValidatingTiles.Remove(pos);
        }

        return false;
    }

    private void UpdateNeighborFrames(int i, int j)
    {
        // Update tiles above and below, but skip if already validating
        for (int dy = -1; dy <= 1; dy += 2)
        {
            if (WorldGen.InWorld(i, j + dy))
            {
                var pos = new Point(i, j + dy);
                if (ValidatingTiles.Contains(pos))
                    continue;

                var neighbor = Framing.GetTileSafely(i, j + dy);
                if (neighbor.HasTile && neighbor.TileType == Type)
                {
                    WorldGen.TileFrame(i, j + dy, resetFrame: false, noBreak: false);
                }
            }
        }
    }

    private void UpdateFrameForSoftCut(int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);

        // Check if this is a center stump
        bool isCenterStump = tile.TileFrameY >= 2 * FrameHeight &&
                           tile.TileFrameY <= 5 * FrameHeight &&
                           tile.TileFrameX >= 3 * FrameWidth;

        if (isCenterStump)
        {
            // Update center stump based on current root configuration
            UpdateCenterStumpFrame(i, j);
            return;
        }

        // Don't update other stumps (left/right roots)
        bool isRootStump = tile.TileFrameY >= 5 * FrameHeight;
        if (isRootStump) return;

        // Check neighbors for soft-cut requirement
        var above = Framing.GetTileSafely(i, j - 1);
        var below = Framing.GetTileSafely(i, j + 1);

        bool needsSoftCut = (!above.HasTile || above.TileType != Type) ||
                           (!below.HasTile || below.TileType != Type);

        if (!needsSoftCut) return;

        // Determine current trunk type from frame
        TrunkType currentType;
        if (tile.TileFrameY == 2 * FrameHeight && tile.TileFrameX < 3 * FrameWidth)
            currentType = TrunkType.Large;
        else if (tile.TileFrameY == 1 * FrameHeight && tile.TileFrameX < 3 * FrameWidth)
            currentType = TrunkType.Medium;
        else if (tile.TileFrameY == 1 * FrameHeight && tile.TileFrameX >= 3 * FrameWidth)
            currentType = TrunkType.Small;
        else
            return; // Already soft-cut or unknown

        // Apply soft-cut frame
        int variant = tile.TileFrameX / FrameWidth % 3;

        switch (currentType)
        {
            case TrunkType.Large:
                tile.TileFrameX = (short)(variant * FrameWidth);
                tile.TileFrameY = (short)(4 * FrameHeight);
                break;

            case TrunkType.Medium:
                tile.TileFrameX = (short)(variant * FrameWidth);
                tile.TileFrameY = 0;
                break;

            case TrunkType.Small:
                tile.TileFrameX = (short)((3 + variant) * FrameWidth);
                tile.TileFrameY = 0;
                break;
        }
    }

    private void UpdateCenterStumpFrame(int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);

        // Check current state of roots and above
        var leftTile = Framing.GetTileSafely(i - 1, j);
        var rightTile = Framing.GetTileSafely(i + 1, j);
        var aboveTile = Framing.GetTileSafely(i, j - 1);

        bool hasLeftRoot = leftTile.HasTile && leftTile.TileType == Type &&
                          leftTile.TileFrameY == 5 * FrameHeight;
        bool hasRightRoot = rightTile.HasTile && rightTile.TileType == Type &&
                           rightTile.TileFrameY == 6 * FrameHeight;
        bool hasAbove = aboveTile.HasTile && aboveTile.TileType == Type;

        // Keep variant consistent
        int variant = tile.TileFrameX / FrameWidth % 3;

        // Update frame based on current configuration
        if (!hasAbove)
        {
            // No tiles above - use soft-cut variants (cols 6-8)
            if (hasLeftRoot && hasRightRoot)
            {
                // Both roots soft-cut - row 4, cols 6-8
                tile.TileFrameX = (short)((6 + variant) * FrameWidth);
                tile.TileFrameY = (short)(4 * FrameHeight);
            }
            else if (!hasLeftRoot && hasRightRoot)
            {
                // Right side connects to root (left is open) - row 2, cols 6-8
                tile.TileFrameX = (short)((6 + variant) * FrameWidth);
                tile.TileFrameY = (short)(2 * FrameHeight);
            }
            else if (hasLeftRoot && !hasRightRoot)
            {
                // Left side connects to root (right is open) - row 3, cols 6-8
                tile.TileFrameX = (short)((6 + variant) * FrameWidth);
                tile.TileFrameY = (short)(3 * FrameHeight);
            }
            else
            {
                // No roots soft-cut - row 5, cols 6-8
                tile.TileFrameX = (short)((6 + variant) * FrameWidth);
                tile.TileFrameY = (short)(5 * FrameHeight);
            }
        }
        else
        {
            // Has tiles above - use normal variants (cols 3-5)
            if (hasLeftRoot && hasRightRoot)
            {
                // Both roots - row 4, cols 3-5
                tile.TileFrameX = (short)((3 + variant) * FrameWidth);
                tile.TileFrameY = (short)(4 * FrameHeight);
            }
            else if (!hasLeftRoot && hasRightRoot)
            {
                // Right side connects to root (left is open) - row 2, cols 3-5
                tile.TileFrameX = (short)((3 + variant) * FrameWidth);
                tile.TileFrameY = (short)(2 * FrameHeight);
            }
            else if (hasLeftRoot && !hasRightRoot)
            {
                // Left side connects to root (right is open) - row 3, cols 3-5
                tile.TileFrameX = (short)((3 + variant) * FrameWidth);
                tile.TileFrameY = (short)(3 * FrameHeight);
            }
            else
            {
                // No roots - row 5, cols 3-5
                tile.TileFrameX = (short)((3 + variant) * FrameWidth);
                tile.TileFrameY = (short)(5 * FrameHeight);
            }
        }
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail && WorldGen.genRand.NextBool(8))
        {
            OnShakeTree(i, j);
        }

        if (!fail)
        {
            var tile = Framing.GetTileSafely(i, j);
            bool isCenterStump = tile.TileFrameY >= 2 * FrameHeight &&
                               tile.TileFrameY <= 5 * FrameHeight &&
                               tile.TileFrameX >= 3 * FrameWidth;
            bool isLeftRoot = tile.TileFrameY == 5 * FrameHeight && tile.TileFrameX < 3 * FrameWidth;
            bool isRightRoot = tile.TileFrameY == 6 * FrameHeight && tile.TileFrameX < 3 * FrameWidth;

            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                       WoodType, Main.rand.Next(1, 3));

            // If center stump destroyed, kill adjacent root stumps
            if (isCenterStump)
            {
                KillAdjacentRoots(i, j);
            }

            // If root destroyed, update center stump
            if (isLeftRoot || isRightRoot)
            {
                int centerX = isLeftRoot ? i + 1 : i - 1;
                if (WorldGen.InWorld(centerX, j))
                {
                    var centerTile = Framing.GetTileSafely(centerX, j);
                    if (centerTile.HasTile && centerTile.TileType == Type)
                    {
                        WorldGen.TileFrame(centerX, j, resetFrame: false, noBreak: false);
                    }
                }
            }

            // Update neighbors for soft-cut
            UpdateNeighborFrames(i, j);
        }
    }

    public override bool CanKillTile(int i, int j, ref bool blockDamaged)
    {
        var above = Framing.GetTileSafely(i, j - 1);
        if (above.HasTile && above.TileType == Type)
        {
            bool isLeftRoot = above.TileFrameY == 5 * FrameHeight && above.TileFrameX < 3 * FrameWidth;
            bool isRightRoot = above.TileFrameY == 6 * FrameHeight && above.TileFrameX < 3 * FrameWidth;
            bool isCenterStump = above.TileFrameY >= 2 * FrameHeight &&
                               above.TileFrameY <= 5 * FrameHeight &&
                               above.TileFrameX >= 3 * FrameWidth;

            if (isLeftRoot || isRightRoot || isCenterStump)
                return false;
        }

        return base.CanKillTile(i, j, ref blockDamaged);
    }

    public override bool CanExplode(int i, int j)
    {
        var above = Framing.GetTileSafely(i, j - 1);
        if (above.HasTile && above.TileType == Type)
        {
            bool isLeftRoot = above.TileFrameY == 5 * FrameHeight && above.TileFrameX < 3 * FrameWidth;
            bool isRightRoot = above.TileFrameY == 6 * FrameHeight && above.TileFrameX < 3 * FrameWidth;
            bool isCenterStump = above.TileFrameY >= 2 * FrameHeight &&
                               above.TileFrameY <= 5 * FrameHeight &&
                               above.TileFrameX >= 3 * FrameWidth;

            if (isLeftRoot || isRightRoot || isCenterStump)
                return false;
        }

        return base.CanExplode(i, j);
    }

    private bool IsRootStump(int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
            return false;

        bool isLeftRoot = tile.TileFrameY == 5 * FrameHeight && tile.TileFrameX < 3 * FrameWidth;
        bool isRightRoot = tile.TileFrameY == 6 * FrameHeight && tile.TileFrameX < 3 * FrameWidth;
        bool isCenterStump = tile.TileFrameY >= 2 * FrameHeight &&
                            tile.TileFrameY <= 5 * FrameHeight &&
                            tile.TileFrameX >= 3 * FrameWidth;

        return isLeftRoot || isRightRoot || isCenterStump;
    }

    private bool ValidateAnchor(int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
            return false;

        bool isLeftStump = tile.TileFrameY == 5 * FrameHeight && tile.TileFrameX < 3 * FrameWidth;
        bool isRightStump = tile.TileFrameY == 6 * FrameHeight && tile.TileFrameX < 3 * FrameWidth;

        if (isLeftStump || isRightStump)
        {
            int centerX = isLeftStump ? i + 1 : i - 1;

            if (!WorldGen.InWorld(centerX, j))
                return false;

            var centerTile = Framing.GetTileSafely(centerX, j);
            if (!centerTile.HasTile || centerTile.TileType != Type)
                return false;

            bool isCenterStump = centerTile.TileFrameY >= 2 * FrameHeight &&
                               centerTile.TileFrameY <= 5 * FrameHeight &&
                               centerTile.TileFrameX >= 3 * FrameWidth;

            return isCenterStump;
        }

        bool isCenterStump2 = tile.TileFrameY >= 2 * FrameHeight &&
                            tile.TileFrameY <= 5 * FrameHeight &&
                            tile.TileFrameX >= 3 * FrameWidth;

        if (isCenterStump2)
        {
            var below = Framing.GetTileSafely(i, j + 1);
            return below.HasTile && (below.TileType == Type || ValidAnchorTiles.Contains(below.TileType));
        }

        var belowTile = Framing.GetTileSafely(i, j + 1);

        if (belowTile.HasTile && belowTile.TileType == Type)
            return true;

        return belowTile.HasTile && ValidAnchorTiles.Contains(belowTile.TileType);
    }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;

        TileID.Sets.IsATreeTrunk[Type] = true;
    }

    private void KillAdjacentRoots(int i, int j)
    {
        // Check left
        var leftTile = Framing.GetTileSafely(i - 1, j);
        if (leftTile.HasTile && leftTile.TileType == Type &&
            leftTile.TileFrameY == 5 * FrameHeight && leftTile.TileFrameX < 3 * FrameWidth)
        {
            WorldGen.KillTile(i - 1, j);
        }

        // Check right
        var rightTile = Framing.GetTileSafely(i + 1, j);
        if (rightTile.HasTile && rightTile.TileType == Type &&
            rightTile.TileFrameY == 6 * FrameHeight && rightTile.TileFrameX < 3 * FrameWidth)
        {
            WorldGen.KillTile(i + 1, j);
        }
    }

    protected override void CreateTree(int i, int j, int height)
    {
        var treeTiles = new List<Point>();

        // Create center stump with frame based on available space
        bool canPlaceLeft = WorldGen.InWorld(i - 1, j) && !Framing.GetTileSafely(i - 1, j).HasTile;
        bool canPlaceRight = WorldGen.InWorld(i + 1, j) && !Framing.GetTileSafely(i + 1, j).HasTile;

        WorldGen.PlaceTile(i, j, Type, true);
        var centerTile = Framing.GetTileSafely(i, j);
        if (centerTile.HasTile && centerTile.TileType == Type)
        {
            int variant = WorldGen.genRand.Next(3);

            if (canPlaceLeft && canPlaceRight)
            {
                // Both roots
                centerTile.TileFrameX = (short)((3 + variant) * FrameWidth);
                centerTile.TileFrameY = (short)(4 * FrameHeight);
            }
            else if (!canPlaceLeft && canPlaceRight)
            {
                // Right side connects to root (left is blocked)
                centerTile.TileFrameX = (short)((3 + variant) * FrameWidth);
                centerTile.TileFrameY = (short)(2 * FrameHeight);
            }
            else if (canPlaceLeft && !canPlaceRight)
            {
                // Left side connects to root (right is blocked)
                centerTile.TileFrameX = (short)((3 + variant) * FrameWidth);
                centerTile.TileFrameY = (short)(3 * FrameHeight);
            }
            else
            {
                // No roots - only use this if absolutely no space
                centerTile.TileFrameX = (short)((3 + variant) * FrameWidth);
                centerTile.TileFrameY = (short)(5 * FrameHeight);
            }

            treeTiles.Add(new Point(i, j));
        }

        // Place root stumps
        if (canPlaceLeft)
        {
            WorldGen.PlaceTile(i - 1, j, Type, true);
            var leftStump = Framing.GetTileSafely(i - 1, j);
            if (leftStump.HasTile && leftStump.TileType == Type)
            {
                int leftVariant = WorldGen.genRand.Next(3);
                leftStump.TileFrameX = (short)(leftVariant * FrameWidth);
                leftStump.TileFrameY = (short)(5 * FrameHeight);
                treeTiles.Add(new Point(i - 1, j));
            }
        }

        if (canPlaceRight)
        {
            WorldGen.PlaceTile(i + 1, j, Type, true);
            var rightStump = Framing.GetTileSafely(i + 1, j);
            if (rightStump.HasTile && rightStump.TileType == Type)
            {
                int rightVariant = WorldGen.genRand.Next(3);
                rightStump.TileFrameX = (short)(rightVariant * FrameWidth);
                rightStump.TileFrameY = (short)(6 * FrameHeight);
                treeTiles.Add(new Point(i + 1, j));
            }
        }

        // Create trunk using base GetTrunkFrame logic
        for (var h = 1; h < height; h++)
        {
            var currentY = j - h;

            if (WorldGen.InWorld(i, currentY))
            {
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

        // Network sync
        if (Main.netMode != NetmodeID.SinglePlayer && treeTiles.Count > 0)
        {
            var minY = treeTiles.Min(p => p.Y);
            var maxY = treeTiles.Max(p => p.Y);
            var minX = treeTiles.Min(p => p.X);
            var maxX = treeTiles.Max(p => p.X);
            NetMessage.SendTileSquare(-1, minX, minY, maxX - minX + 1, maxY - minY + 1, TileChangeType.None);
        }
    }

    protected override WeightedRandom<int> GetTreeDrops()
    {
        var drop = new WeightedRandom<int>();
        drop.Add(ItemID.Peach, 0.008f);
        drop.Add(ItemID.Acorn, 0.025f);
        drop.Add(ItemID.Cherry, 0.008f);
        return drop;
    }

    protected override void GrowEffects(int i, int j)
    {
        var center = new Vector2(i, j) * 16f + new Vector2(11);

        for (var g = 0; g < 15; g++)
        {
            var needle = Dust.NewDustDirect(center + Main.rand.NextVector2Unit() * 40f, 0, 0,
                DustID.GrassBlades, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-2f, 0.5f));
            needle.fadeIn = 1.3f;
            needle.scale = 0.9f;
            needle.color = Color.DarkGreen;
        }
    }

    public static bool GrowSpruceTree(int i, int j)
    {
        var instance = ModContent.GetInstance<SpruceTree>();
        return instance.GrowTree(i, j);
    }
}