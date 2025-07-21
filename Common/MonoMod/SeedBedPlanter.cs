/// <summary>
/// Adapted from Spirit Reforged's PlanterBoxMerge: https://github.com/GabeHasWon/SpiritReforged/blob/master/Common/TileCommon/PresetTiles/PlanterBoxTile.cs#L5
/// </summary>
using System.Collections.Generic;
using Terraria.Enums;

namespace Reverie.Common.MonoMod;

public class SeedBedPlanter : GlobalTile
{
    /// <summary>
    /// Includes modded tile types that use seedbed behavior.
    /// Types are expected to be added during SetStaticDefaults.
    /// </summary>
    public static readonly HashSet<int> SeedBedTypes = [];

    public override void Load()
    {
        On_WorldGen.CanCutTile += StopCut;
        On_WorldGen.PlaceAlch += ForcePlaceAlch;
        On_WorldGen.GrowAlch += CustomGrowAlch;
        On_WorldGen.CheckAlch += CustomCheckAlch;
    }

    /// <summary>
    /// Prevent planted herbs from being cut above custom seedbeds.
    /// </summary>
    private static bool StopCut(On_WorldGen.orig_CanCutTile orig, int x, int y, TileCuttingContext context)
    {
        if (Main.tile[x, y + 1] != null && SeedBedTypes.Contains(Main.tile[x, y + 1].TileType))
            return false; // Skips orig

        return orig(x, y, context);
    }

    /// <summary>
    /// Allow herbs to be placed on custom seedbeds.
    /// </summary>
    private static bool ForcePlaceAlch(On_WorldGen.orig_PlaceAlch orig, int x, int y, int style)
    {
        if (Main.tile[x, y + 1] != null && SeedBedTypes.Contains(Main.tile[x, y + 1].TileType))
        {
            var tile = Main.tile[x, y];
            tile.HasTile = true;
            tile.TileType = TileID.ImmatureHerbs;
            tile.TileFrameX = (short)(18 * style);
            tile.TileFrameY = 0;
            return true;
        }

        return orig(x, y, style);
    }

    /// <summary>
    /// Custom herb growth with 5x speed on seedbeds.
    /// </summary>
    private static void CustomGrowAlch(On_WorldGen.orig_GrowAlch orig, int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);
        var tileBelow = Framing.GetTileSafely(i, j + 1);

        if (IsHerbTile(tile.TileType) && tileBelow.HasTile && SeedBedTypes.Contains(tileBelow.TileType))
        {
            var herb = Main.tile[i, j];
            if (!herb.HasTile) return;

            // 5x growth attempts for seedbeds
            for (int attempts = 0; attempts < 5; attempts++)
            {
                if (WorldGen.genRand.NextBool(10))
                {
                    var currentFrame = herb.TileFrameX; // Preserve herb type
                    var currentTileType = herb.TileType;

                    if (currentTileType == TileID.ImmatureHerbs)
                    {
                        herb.TileType = TileID.MatureHerbs;
                        herb.TileFrameX = currentFrame;
                        herb.TileFrameY = 0;
                    }
                    else if (currentTileType == TileID.MatureHerbs)
                    {
                        herb.TileType = TileID.BloomingHerbs;
                        herb.TileFrameX = currentFrame;
                        herb.TileFrameY = 0;
                    }

                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        NetMessage.SendTileSquare(-1, i, j, 1, 1, TileChangeType.None);
                    }
                    break;
                }
            }
            return;
        }

        orig(i, j);
    }

    /// <summary>
    /// Herbs on seedbeds are always valid during validation.
    /// </summary>
    private static void CustomCheckAlch(On_WorldGen.orig_CheckAlch orig, int x, int y)
    {
        var herb = Framing.GetTileSafely(x, y);
        var tileBelow = Framing.GetTileSafely(x, y + 1);

        // Skip validation for herbs on seedbeds
        if (IsHerbTile(herb.TileType) && tileBelow.HasTile && SeedBedTypes.Contains(tileBelow.TileType))
        {
            return;
        }

        orig(x, y);
    }

    public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
    {
        // Prevent vanilla plants from breaking on seedbeds
        if ((type is TileID.Plants or TileID.Plants2 || Main.tileAlch[type]) &&
            SeedBedTypes.Contains(Framing.GetTileSafely(i, j + 1).TileType))
        {
            noBreak = true;
        }
        else if (SeedBedTypes.Contains(type))
        {
            // Handle custom seedbed framing
            UpdateSeedBedFraming(i, j);
            return false; // Prevent vanilla framing
        }

        return true;
    }

    /// <summary>
    /// Updates seedbed framing with connecting behavior.
    /// </summary>
    private static void UpdateSeedBedFraming(int centerI, int centerJ)
    {
        // Skip if no seedbed at this position
        if (!IsSeedBed(centerI, centerJ))
            return;

        // Find the full extent of connected seedbeds horizontally
        int leftMost = centerI;
        int rightMost = centerI;

        // Find leftmost seedbed
        while (IsSeedBed(leftMost - 1, centerJ))
            leftMost--;

        // Find rightmost seedbed
        while (IsSeedBed(rightMost + 1, centerJ))
            rightMost++;

        // Update all tiles in the sequence
        for (int x = leftMost; x <= rightMost; x++)
        {
            if (IsSeedBed(x, centerJ))
            {
                UpdateSeedBedTileFrame(x, centerJ, leftMost, rightMost);
            }
        }
    }

    private static void UpdateSeedBedTileFrame(int i, int j, int leftMost, int rightMost)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || !SeedBedTypes.Contains(tile.TileType))
            return;

        int sequenceLength = rightMost - leftMost + 1;
        int positionInSequence = i - leftMost;

        int frameX;

        if (sequenceLength == 1)
        {
            // Orphan tile
            frameX = 7;
        }
        else if (positionInSequence == 0)
        {
            // Left end
            frameX = 0;
        }
        else if (positionInSequence == sequenceLength - 1)
        {
            // Right end
            frameX = 6;
        }
        else
        {
            // Calculate center positions
            int centerPos = sequenceLength / 2;

            if (positionInSequence == centerPos && sequenceLength % 2 == 1)
            {
                // True center - only for odd-length sequences at exact middle
                frameX = 3;
            }
            else if (positionInSequence < centerPos)
            {
                // Left side centers - cycle through frames 1-2
                int leftCenterIndex = (positionInSequence - 1) % 2;
                frameX = 1 + leftCenterIndex;
            }
            else
            {
                // Right side centers - cycle through frames 4-5
                int rightCenterIndex = (positionInSequence - centerPos - (sequenceLength % 2)) % 2;
                frameX = 4 + rightCenterIndex;
            }
        }

        short newFrameX = (short)(frameX * 18);
        if (tile.TileFrameX != newFrameX)
        {
            tile.TileFrameX = newFrameX;
            tile.TileFrameY = 0;

            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendTileSquare(-1, i, j, 1, 1, TileChangeType.None);
            }
        }
    }

    private static bool IsSeedBed(int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);
        return tile.HasTile && SeedBedTypes.Contains(tile.TileType);
    }

    private static bool IsHerbTile(int tileType)
    {
        return tileType == TileID.BloomingHerbs ||
               tileType == TileID.MatureHerbs ||
               tileType == TileID.ImmatureHerbs;
    }

    public override void Unload()
    {
        SeedBedTypes.Clear();
    }
}