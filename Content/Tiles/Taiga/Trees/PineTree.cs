using Reverie.Content.Tiles.Canopy;
using Reverie.Content.Tiles.Canopy.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace Reverie.Content.Tiles.Taiga.Trees
{
    public class PineTree : CustomTree
    {
        #region Properties
        public override int FrameWidth => 22;
        public override int FrameHeight => 22;
        public override int TreeWidth => 1;
        public override int MaxHeight => 30;
        public override int MinHeight => 12;

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

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            var tile = Framing.GetTileSafely(i, j);
            if (!tile.HasTile || tile.TileType != Type)
                return false;

            // Check if this tile needs to break due to missing anchor
            if (!ValidateAnchor(i, j))
            {
                WorldGen.KillTile(i, j);
                return false;
            }

            // Update frame for soft-cut if needed
            UpdateFrameForSoftCut(i, j);

            return false;
        }

        private bool ValidateAnchor(int i, int j)
        {
            var tile = Framing.GetTileSafely(i, j);

            // Check if this is a root stump (left or right)
            bool isLeftStump = tile.TileFrameY == 5 * FrameHeight && tile.TileFrameX < 3 * FrameWidth;
            bool isRightStump = tile.TileFrameY == 6 * FrameHeight && tile.TileFrameX < 3 * FrameWidth;

            if (isLeftStump || isRightStump)
            {
                // Root stumps must have center stump adjacent
                int centerX = isLeftStump ? i + 1 : i - 1;
                var centerTile = Framing.GetTileSafely(centerX, j);

                if (!centerTile.HasTile || centerTile.TileType != Type)
                    return false;

                // Verify it's actually a center stump
                bool isCenterStump = centerTile.TileFrameY >= 2 * FrameHeight &&
                                   centerTile.TileFrameY <= 5 * FrameHeight &&
                                   centerTile.TileFrameX >= 3 * FrameWidth;

                return isCenterStump;
            }

            // Regular trunk tiles must have valid anchor below
            var below = Framing.GetTileSafely(i, j + 1);

            // Can anchor to same tree type
            if (below.HasTile && below.TileType == Type)
                return true;

            // Or to valid ground tiles
            return below.HasTile && ValidAnchorTiles.Contains(below.TileType);
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

        private void UpdateNeighborFrames(int i, int j)
        {
            // Update tiles above and below
            for (int dy = -1; dy <= 1; dy += 2)
            {
                if (WorldGen.InWorld(i, j + dy))
                {
                    var neighbor = Framing.GetTileSafely(i, j + dy);
                    if (neighbor.HasTile && neighbor.TileType == Type)
                    {
                        WorldGen.TileFrame(i, j + dy, resetFrame: false, noBreak: false);
                    }
                }
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
            drop.Add(ItemID.None, 0.6f);
            drop.Add(ItemID.Acorn, 0.25f);
            drop.Add(ItemID.Wood, 0.12f);
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

        public static bool GrowPineTree(int i, int j)
        {
            var instance = ModContent.GetInstance<PineTree>();
            return instance.GrowTree(i, j);
        }
    }
}