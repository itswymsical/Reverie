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
using Terraria.Enums;
using Terraria.ObjectData;
using Terraria.Localization;

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

    public static bool IsTrunkTile(Tile tile)
    {
        if (!tile.HasTile || tile.TileType != ModContent.TileType<SpruceTree>())
            return false;

        int frameY = tile.TileFrameY / 22;
        return frameY is 1 or 2;
    }

    public static SpruceBranch.BranchSize GetBranchSizeForTrunk(Tile trunkTile)
    {
        int frameX = trunkTile.TileFrameX / 22;
        int frameY = trunkTile.TileFrameY / 22;

        if (frameY == 2 && frameX < 3) return SpruceBranch.BranchSize.Large;
        if (frameY == 1 && frameX < 3) return SpruceBranch.BranchSize.Medium;
        if (frameY == 1 && frameX >= 3) return SpruceBranch.BranchSize.Small;

        return SpruceBranch.BranchSize.Small;
    }

    private void PlaceBranch(int i, int j, SpruceBranch.BranchSize size)
    {
        var tile = Framing.GetTileSafely(i, j);
        tile.HasTile = true;
        tile.TileType = (ushort)ModContent.TileType<SpruceBranch>();
        tile.IsHalfBlock = false;
        tile.Slope = SlopeType.Solid;

        // check trunk neighbor position
        var leftTile = Framing.GetTileSafely(i - 1, j);
        var rightTile = Framing.GetTileSafely(i + 1, j);

        bool facingRight;
        if (leftTile.HasTile && leftTile.TileType == Type && IsTrunkTile(leftTile))
        {
            facingRight = true;
        }
        else if (rightTile.HasTile && rightTile.TileType == Type && IsTrunkTile(rightTile))
        {
            facingRight = false;
        }
        else
        {
            facingRight = false;
        }

        var frame = SpruceBranch.GetBranchFrame(size, facingRight, WorldGen.genRand.Next(3));

        int sizeOffset = size switch
        {
            SpruceBranch.BranchSize.Large => 0,
            SpruceBranch.BranchSize.Medium => 3,
            SpruceBranch.BranchSize.Small => 6,
            _ => 6
        };

        tile.TileFrameX = (short)(frame.X * 16);
        tile.TileFrameY = (short)((frame.Y + sizeOffset) * 16);
    }

    private bool CanPlaceBranch(int i, int j)
    {
        if (!WorldGen.InWorld(i, j)) return false;

        var tile = Framing.GetTileSafely(i, j);
        if (tile.HasTile) return false;

        var aboveTile = Framing.GetTileSafely(i, j - 1);
        var belowTile = Framing.GetTileSafely(i, j + 1);

        bool hasBranchAbove = aboveTile.HasTile && aboveTile.TileType == ModContent.TileType<SpruceBranch>();
        bool hasBranchBelow = belowTile.HasTile && belowTile.TileType == ModContent.TileType<SpruceBranch>();

        return !hasBranchAbove && !hasBranchBelow;
    }

    private void PlaceBranches(int i, int j, int height)
    {
        for (int h = 3; h < height - 1; h += 3)
        {
            int currentY = j - h;
            if (!WorldGen.InWorld(i, currentY)) continue;

            var trunkTile = Framing.GetTileSafely(i, currentY);
            if (!IsTrunkTile(trunkTile)) continue;

            var branchSize = GetBranchSizeForTrunk(trunkTile);

            if (CanPlaceBranch(i - 1, currentY))
            {
                PlaceBranch(i - 1, currentY, branchSize);
            }

            if (CanPlaceBranch(i + 1, currentY))
            {
                PlaceBranch(i + 1, currentY, branchSize);
            }
        }
    }

    protected override void CreateTree(int i, int j, int height)
    {
        var treeTiles = new List<Point>();

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

        // Create trunk
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

        // Place branches after trunk is complete
        PlaceBranches(i, j, height);

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

    // Rest of the existing methods remain the same
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
        if (fail && WorldGen.genRand.NextBool(18))
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

            // Kill adjacent branches when trunk is destroyed
            KillAdjacentBranches(i, j);
        }
    }

    private void KillAdjacentBranches(int i, int j)
    {
        // Check left and right for branches
        var leftTile = Framing.GetTileSafely(i - 1, j);
        if (leftTile.HasTile && leftTile.TileType == ModContent.TileType<SpruceBranch>())
        {
            WorldGen.KillTile(i - 1, j);
        }

        var rightTile = Framing.GetTileSafely(i + 1, j);
        if (rightTile.HasTile && rightTile.TileType == ModContent.TileType<SpruceBranch>())
        {
            WorldGen.KillTile(i + 1, j);
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

public class SpruceBranch : ModTile
{
    public enum BranchSize
    {
        Large,
        Medium,
        Small
    }

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileAxe[Type] = true;
        Main.tileBlockLight[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.IsATreeTrunk[Type] = false;
        TileID.Sets.IsShakeable[Type] = false;
        TileID.Sets.GetsDestroyedForMeteors[Type] = true;

        AddMapEntry(new Color(88, 150, 112), Language.GetText("MapObject.Tree"));
        DustType = DustID.BorealWood_Small;
        HitSound = SoundID.Dig;
    }

    public static Point GetBranchFrame(BranchSize size, bool facingRight, int variant = 0)
    {
        int frameX = facingRight ? 1 : 0;
        int frameY = variant % 3;
        return new Point(frameX, frameY);
    }

    public static (int width, int height, Rectangle sourceRect) GetBranchData(BranchSize size, bool facingRight, int variant = 0)
    {
        var frame = GetBranchFrame(size, facingRight, variant);

        return size switch
        {
            BranchSize.Large => (78, 78, new Rectangle(frame.X * 78, frame.Y * 78, 78, 78)),
            BranchSize.Medium => (62, 62, new Rectangle(160 + frame.X * 62, frame.Y * 62, 62, 62)),
            BranchSize.Small => (38, 38, new Rectangle(288 + frame.X * 38, frame.Y * 38, 38, 38)),
            _ => (38, 38, new Rectangle(288, 0, 38, 38))
        };
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
            return false;

        var samplerState = Main.graphics.GraphicsDevice.SamplerStates[0];
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
            DepthStencilState.None, Main.Rasterizer);

        var size = GetBranchSizeFromFrame(tile);
        bool facingRight = (tile.TileFrameX / 16) % 2 == 1;
        int variant = (tile.TileFrameY / 16) % 3;

        var (width, height, sourceRect) = GetBranchData(size, facingRight, variant);

        float sway = SpruceTree.GetSway(i, j) * 0.3f;
        if (!facingRight) sway = -sway;

        var texture = TextureAssets.Tile[Type].Value;
        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        Vector2 drawPos = new Vector2(i * 16, j * 16) + zero - Main.screenPosition;

        int rightOffset = size switch
        {
            BranchSize.Large => 9,
            BranchSize.Medium => 6,
            BranchSize.Small => 10,
            _ => 4
        };

        int leftOffset = size switch
        {
            BranchSize.Large => width - 23,
            BranchSize.Medium => width - 22,
            BranchSize.Small => width - 24,
            _ => width - 16
        };

        if (facingRight)
        {
            drawPos.X -= rightOffset;
        }
        else
        {
            drawPos.X -= leftOffset;
        }

        drawPos.Y -= (height - 16) / 2;

        Color lighting = Lighting.GetColor(i, j);

        spriteBatch.Draw(texture, drawPos, sourceRect, lighting, sway, Vector2.Zero, 1f,
                        SpriteEffects.None, 1f);

        return false;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null);
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        if (!ValidateTrunkConnection(i, j))
        {
            WorldGen.KillTile(i, j);
            return false;
        }
        return false;
    }

    private bool ValidateTrunkConnection(int i, int j)
    {
        var leftTile = Framing.GetTileSafely(i - 1, j);
        var rightTile = Framing.GetTileSafely(i + 1, j);

        bool hasLeftTrunk = leftTile.HasTile && leftTile.TileType == ModContent.TileType<SpruceTree>() &&
                           SpruceTree.IsTrunkTile(leftTile);
        bool hasRightTrunk = rightTile.HasTile && rightTile.TileType == ModContent.TileType<SpruceTree>() &&
                            SpruceTree.IsTrunkTile(rightTile);

        return hasLeftTrunk || hasRightTrunk;
    }

    private BranchSize GetBranchSizeFromFrame(Tile tile)
    {
        int frameY = tile.TileFrameY / 16;

        return frameY switch
        {
            0 or 1 or 2 => BranchSize.Large,
            3 or 4 or 5 => BranchSize.Medium,
            6 or 7 or 8 => BranchSize.Small,
            _ => BranchSize.Small
        };
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                       ModContent.ItemType<SpruceWoodItem>(), 1);
        }
    }
}