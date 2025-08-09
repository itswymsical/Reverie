using Reverie.Content.Tiles.Canopy;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Utilities;
using Terraria.GameContent.Drawing;
using Terraria.GameContent;
using Reverie.Core.Tiles;
using Reverie.Content.Tiles.Taiga.Furniture;

namespace Reverie.Content.Tiles.Taiga.Trees;

public class SpruceTree : CustomTree
{
    #region Properties
    public override int FrameWidth => 22;
    public override int FrameHeight => 22;
    public override int TreeWidth => 1;
    public override int MaxHeight => 30;
    public override int MinHeight => 12;
    public override int WoodType => ModContent.ItemType<SpruceWoodItem>();
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

    // Branch sprite regions on the sheet
    private static readonly Rectangle LargeBranchRegion = new(194, 0, 158, 238);
    private static readonly Rectangle MediumBranchRegion = new(354, 0, 126, 190);
    private static readonly Rectangle SmallBranchRegion = new(482, 0, 78, 118);

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

    private bool IsBranchFrame(int frameX, int frameY)
    {
        // Branches are placed at frameY 7+ (beyond normal trunk frames)
        return frameY >= 7;
    }

    private bool IsTrunkFrame(int frameX, int frameY)
    {
        // Large trunk
        if (frameY == 2 && frameX < 3) return true;
        // Medium trunk
        if (frameY == 1 && frameX < 3) return true;
        // Small trunk
        if (frameY == 1 && frameX >= 3 && frameX < 6) return true;

        return false;
    }

    private TrunkType? GetTrunkType(int frameX, int frameY)
    {
        if (frameY == 2 && frameX < 3) return TrunkType.Large;
        if (frameY == 1 && frameX < 3) return TrunkType.Medium;
        if (frameY == 1 && frameX >= 3 && frameX < 6) return TrunkType.Small;

        return null;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
            return;

        int frameX = tile.TileFrameX / FrameWidth;
        int frameY = tile.TileFrameY / FrameHeight;

        // Only draw branches for branch frames
        if (!IsBranchFrame(frameX, frameY))
            return;

        DrawBranch(i, j, spriteBatch, tile);
    }

    private void DrawBranch(int i, int j, SpriteBatch spriteBatch, Tile tile)
    {
        int frameX = tile.TileFrameX / FrameWidth;
        int frameY = tile.TileFrameY / FrameHeight;

        // Decode branch info from frame
        // frameY 7 = Large, 8 = Medium, 9 = Small
        // frameX 0-2 = left variants, 3-5 = right variants

        TrunkType branchSize;
        bool isRight;
        int variant;

        switch (frameY)
        {
            case 7:
                branchSize = TrunkType.Large;
                break;
            case 8:
                branchSize = TrunkType.Medium;
                break;
            case 9:
                branchSize = TrunkType.Small;
                break;
            default:
                return;
        }

        if (frameX < 3)
        {
            isRight = false;
            variant = frameX;
        }
        else
        {
            isRight = true;
            variant = frameX - 3;
        }

        Color color = Lighting.GetColor(i, j);
        if (!TileDrawing.IsVisible(tile))
            return;

        float rotation = GetSway(i, j) * 0.3f;
        var tileCenterScreen = new Vector2(i * 16 + 8, j * 16 + 8) - Main.screenPosition;

        var texture = ModContent.Request<Texture2D>("Reverie/Content/Tiles/Taiga/Trees/SpruceTree").Value;

        Rectangle sourceRect;
        Vector2 origin;
        Vector2 drawPos;

        switch (branchSize)
        {
            case TrunkType.Large:
                sourceRect = new Rectangle(
                    194 + (isRight ? 78 : 0),
                    variant * 60,
                    78, 60
                );
                origin = isRight ? new Vector2(6, 30) : new Vector2(72, 30);
                drawPos = tileCenterScreen;
                break;

            case TrunkType.Medium:
                sourceRect = new Rectangle(
                    354 + (isRight ? 62 : 0),
                    variant * 40,
                    62, 40
                );
                origin = isRight ? new Vector2(6, 20) : new Vector2(56, 20);
                drawPos = tileCenterScreen;
                break;

            case TrunkType.Small:
                sourceRect = new Rectangle(
                    482 + (isRight ? 38 : 0),
                    variant * 32,
                    38, 32
                );
                origin = isRight ? new Vector2(6, 16) : new Vector2(32, 16);
                drawPos = tileCenterScreen;
                break;

            default:
                return;
        }

        spriteBatch.Draw(texture, drawPos, sourceRect, color, rotation, origin, 1f, SpriteEffects.None, 0f);
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

    private static readonly HashSet<Point> ValidatingTiles = new();

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
            return false;

        var pos = new Point(i, j);

        if (ValidatingTiles.Contains(pos))
            return false;

        ValidatingTiles.Add(pos);

        try
        {
            if (!ValidateAnchor(i, j))
            {
                WorldGen.KillTile(i, j);
                return false;
            }

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

        int frameX = tile.TileFrameX / FrameWidth;
        int frameY = tile.TileFrameY / FrameHeight;

        // Skip branches
        if (IsBranchFrame(frameX, frameY))
            return;

        bool isCenterStump = tile.TileFrameY >= 2 * FrameHeight &&
                           tile.TileFrameY <= 5 * FrameHeight &&
                           tile.TileFrameX >= 3 * FrameWidth;

        if (isCenterStump)
        {
            UpdateCenterStumpFrame(i, j);
            return;
        }

        bool isRootStump = tile.TileFrameY >= 5 * FrameHeight;
        if (isRootStump) return;

        var above = Framing.GetTileSafely(i, j - 1);
        var below = Framing.GetTileSafely(i, j + 1);

        bool needsSoftCut = (!above.HasTile || above.TileType != Type) ||
                           (!below.HasTile || below.TileType != Type);

        if (!needsSoftCut) return;

        TrunkType currentType;
        if (tile.TileFrameY == 2 * FrameHeight && tile.TileFrameX < 3 * FrameWidth)
            currentType = TrunkType.Large;
        else if (tile.TileFrameY == 1 * FrameHeight && tile.TileFrameX < 3 * FrameWidth)
            currentType = TrunkType.Medium;
        else if (tile.TileFrameY == 1 * FrameHeight && tile.TileFrameX >= 3 * FrameWidth)
            currentType = TrunkType.Small;
        else
            return;

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

        var leftTile = Framing.GetTileSafely(i - 1, j);
        var rightTile = Framing.GetTileSafely(i + 1, j);
        var aboveTile = Framing.GetTileSafely(i, j - 1);

        bool hasLeftRoot = leftTile.HasTile && leftTile.TileType == Type &&
                          leftTile.TileFrameY == 5 * FrameHeight;
        bool hasRightRoot = rightTile.HasTile && rightTile.TileType == Type &&
                           rightTile.TileFrameY == 6 * FrameHeight;
        bool hasAbove = aboveTile.HasTile && aboveTile.TileType == Type;

        int variant = tile.TileFrameX / FrameWidth % 3;

        if (!hasAbove)
        {
            // Soft-cut variants
            if (hasLeftRoot && hasRightRoot)
            {
                tile.TileFrameX = (short)((6 + variant) * FrameWidth);
                tile.TileFrameY = (short)(4 * FrameHeight);
            }
            else if (!hasLeftRoot && hasRightRoot)
            {
                tile.TileFrameX = (short)((6 + variant) * FrameWidth);
                tile.TileFrameY = (short)(2 * FrameHeight);
            }
            else if (hasLeftRoot && !hasRightRoot)
            {
                tile.TileFrameX = (short)((6 + variant) * FrameWidth);
                tile.TileFrameY = (short)(3 * FrameHeight);
            }
            else
            {
                tile.TileFrameX = (short)((6 + variant) * FrameWidth);
                tile.TileFrameY = (short)(5 * FrameHeight);
            }
        }
        else
        {
            // Normal variants
            if (hasLeftRoot && hasRightRoot)
            {
                tile.TileFrameX = (short)((3 + variant) * FrameWidth);
                tile.TileFrameY = (short)(4 * FrameHeight);
            }
            else if (!hasLeftRoot && hasRightRoot)
            {
                tile.TileFrameX = (short)((3 + variant) * FrameWidth);
                tile.TileFrameY = (short)(2 * FrameHeight);
            }
            else if (hasLeftRoot && !hasRightRoot)
            {
                tile.TileFrameX = (short)((3 + variant) * FrameWidth);
                tile.TileFrameY = (short)(3 * FrameHeight);
            }
            else
            {
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

            int frameX = tile.TileFrameX / FrameWidth;
            int frameY = tile.TileFrameY / FrameHeight;

            // Don't drop wood from branches
            if (!IsBranchFrame(frameX, frameY))
            {
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                           WoodType, Main.rand.Next(1, 3));
            }

            bool isCenterStump = tile.TileFrameY >= 2 * FrameHeight &&
                               tile.TileFrameY <= 5 * FrameHeight &&
                               tile.TileFrameX >= 3 * FrameWidth;
            bool isLeftRoot = tile.TileFrameY == 5 * FrameHeight && tile.TileFrameX < 3 * FrameWidth;
            bool isRightRoot = tile.TileFrameY == 6 * FrameHeight && tile.TileFrameX < 3 * FrameWidth;

            if (isCenterStump)
            {
                KillAdjacentRoots(i, j);
            }

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

            UpdateNeighborFrames(i, j);
        }
    }

    public override bool CanKillTile(int i, int j, ref bool blockDamaged)
    {
        var tile = Framing.GetTileSafely(i, j);
        int frameX = tile.TileFrameX / FrameWidth;
        int frameY = tile.TileFrameY / FrameHeight;

        // Branches can always be killed
        if (IsBranchFrame(frameX, frameY))
            return true;

        var above = Framing.GetTileSafely(i, j - 1);
        if (above.HasTile && above.TileType == Type)
        {
            int aboveFrameX = above.TileFrameX / FrameWidth;
            int aboveFrameY = above.TileFrameY / FrameHeight;

            // Skip branch check
            if (IsBranchFrame(aboveFrameX, aboveFrameY))
                return true;

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
        var tile = Framing.GetTileSafely(i, j);
        int frameX = tile.TileFrameX / FrameWidth;
        int frameY = tile.TileFrameY / FrameHeight;

        // Branches can explode
        if (IsBranchFrame(frameX, frameY))
            return true;

        var above = Framing.GetTileSafely(i, j - 1);
        if (above.HasTile && above.TileType == Type)
        {
            int aboveFrameX = above.TileFrameX / FrameWidth;
            int aboveFrameY = above.TileFrameY / FrameHeight;

            // Skip branch check
            if (IsBranchFrame(aboveFrameX, aboveFrameY))
                return true;

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

        int frameX = tile.TileFrameX / FrameWidth;
        int frameY = tile.TileFrameY / FrameHeight;

        // Branches need adjacent trunk
        if (IsBranchFrame(frameX, frameY))
        {
            // Check left for trunk
            var leftTile = Framing.GetTileSafely(i - 1, j);
            if (leftTile.HasTile && leftTile.TileType == Type)
            {
                int leftFrameX = leftTile.TileFrameX / FrameWidth;
                int leftFrameY = leftTile.TileFrameY / FrameHeight;
                if (IsTrunkFrame(leftFrameX, leftFrameY))
                    return true;
            }

            // Check right for trunk
            var rightTile = Framing.GetTileSafely(i + 1, j);
            if (rightTile.HasTile && rightTile.TileType == Type)
            {
                int rightFrameX = rightTile.TileFrameX / FrameWidth;
                int rightFrameY = rightTile.TileFrameY / FrameHeight;
                if (IsTrunkFrame(rightFrameX, rightFrameY))
                    return true;
            }

            return false;
        }

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
        var leftTile = Framing.GetTileSafely(i - 1, j);
        if (leftTile.HasTile && leftTile.TileType == Type &&
            leftTile.TileFrameY == 5 * FrameHeight && leftTile.TileFrameX < 3 * FrameWidth)
        {
            WorldGen.KillTile(i - 1, j);
        }

        var rightTile = Framing.GetTileSafely(i + 1, j);
        if (rightTile.HasTile && rightTile.TileType == Type &&
            rightTile.TileFrameY == 6 * FrameHeight && rightTile.TileFrameX < 3 * FrameWidth)
        {
            WorldGen.KillTile(i + 1, j);
        }
    }

    protected override void CreateTree(int i, int j, int height)
    {
        Main.NewText($"[DEBUG] Creating tree at ({i}, {j}) with height {height}", Color.Yellow);

        var treeTiles = new List<Point>();
        int branchesPlaced = 0;
        int branchAttempts = 0;

        // Create center stump
        bool canPlaceLeft = WorldGen.InWorld(i - 1, j) && !Framing.GetTileSafely(i - 1, j).HasTile;
        bool canPlaceRight = WorldGen.InWorld(i + 1, j) && !Framing.GetTileSafely(i + 1, j).HasTile;

        WorldGen.PlaceTile(i, j, Type, true);
        var centerTile = Framing.GetTileSafely(i, j);
        if (centerTile.HasTile && centerTile.TileType == Type)
        {
            int variant = WorldGen.genRand.Next(3);

            if (canPlaceLeft && canPlaceRight)
            {
                centerTile.TileFrameX = (short)((3 + variant) * FrameWidth);
                centerTile.TileFrameY = (short)(4 * FrameHeight);
            }
            else if (!canPlaceLeft && canPlaceRight)
            {
                centerTile.TileFrameX = (short)((3 + variant) * FrameWidth);
                centerTile.TileFrameY = (short)(2 * FrameHeight);
            }
            else if (canPlaceLeft && !canPlaceRight)
            {
                centerTile.TileFrameX = (short)((3 + variant) * FrameWidth);
                centerTile.TileFrameY = (short)(3 * FrameHeight);
            }
            else
            {
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

        // Create trunk with branches
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

                    Main.NewText($"[DEBUG] Placed trunk at height {h}: Frame ({frame.X}, {frame.Y})", Color.Green);

                    // Determine trunk type for branch size
                    var trunkType = GetTrunkTypeForHeight(h, height);

                    // Try to place branches (skip first 2 heights)
                    if (h > 2)
                    {
                        branchAttempts++;
                        bool shouldPlaceBranch = WorldGen.genRand.NextBool(3); // 33% chance

                        Main.NewText($"[DEBUG] Height {h}: TrunkType={trunkType}, RollForBranch={shouldPlaceBranch}", Color.Cyan);

                        if (shouldPlaceBranch)
                        {
                            // Check placement for branches
                            bool canPlaceBranchLeft = WorldGen.InWorld(i - 1, currentY) &&
                                                     !Framing.GetTileSafely(i - 1, currentY).HasTile;
                            bool canPlaceBranchRight = WorldGen.InWorld(i + 1, currentY) &&
                                                      !Framing.GetTileSafely(i + 1, currentY).HasTile;

                            Main.NewText($"[DEBUG] Can place: Left={canPlaceBranchLeft}, Right={canPlaceBranchRight}", Color.Orange);

                            // Randomly decide which side(s) to place branches
                            bool placeLeft = canPlaceBranchLeft && WorldGen.genRand.NextBool();
                            bool placeRight = canPlaceBranchRight && (placeLeft ? WorldGen.genRand.NextBool(3) : WorldGen.genRand.NextBool());

                            Main.NewText($"[DEBUG] Will place: Left={placeLeft}, Right={placeRight}", Color.Magenta);

                            if (placeLeft)
                            {
                                WorldGen.PlaceTile(i - 1, currentY, Type, true);
                                var leftBranch = Framing.GetTileSafely(i - 1, currentY);
                                if (leftBranch.HasTile && leftBranch.TileType == Type)
                                {
                                    int branchVariant = WorldGen.genRand.Next(3);
                                    int branchFrameY = trunkType switch
                                    {
                                        TrunkType.Large => 7,
                                        TrunkType.Medium => 8,
                                        TrunkType.Small => 9,
                                        _ => 7
                                    };

                                    leftBranch.TileFrameX = (short)(branchVariant * FrameWidth);
                                    leftBranch.TileFrameY = (short)(branchFrameY * FrameHeight);
                                    treeTiles.Add(new Point(i - 1, currentY));
                                    branchesPlaced++;

                                    Main.NewText($"[DEBUG] LEFT BRANCH PLACED: Frame ({branchVariant}, {branchFrameY})", Color.Lime);
                                }
                            }

                            if (placeRight)
                            {
                                WorldGen.PlaceTile(i + 1, currentY, Type, true);
                                var rightBranch = Framing.GetTileSafely(i + 1, currentY);
                                if (rightBranch.HasTile && rightBranch.TileType == Type)
                                {
                                    int branchVariant = WorldGen.genRand.Next(3);
                                    int branchFrameY = trunkType switch
                                    {
                                        TrunkType.Large => 7,
                                        TrunkType.Medium => 8,
                                        TrunkType.Small => 9,
                                        _ => 7
                                    };

                                    rightBranch.TileFrameX = (short)((3 + branchVariant) * FrameWidth);
                                    rightBranch.TileFrameY = (short)(branchFrameY * FrameHeight);
                                    treeTiles.Add(new Point(i + 1, currentY));
                                    branchesPlaced++;

                                    Main.NewText($"[DEBUG] RIGHT BRANCH PLACED: Frame ({3 + branchVariant}, {branchFrameY})", Color.Lime);
                                }
                            }
                        }
                    }
                }
            }
        }

        Main.NewText($"[DEBUG] Tree creation complete. Branches placed: {branchesPlaced}/{branchAttempts} attempts", Color.Gold);

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

    public static bool Grow(int i, int j)
    {
        var instance = ModContent.GetInstance<SpruceTree>();
        return instance.GrowTree(i, j);
    }
}