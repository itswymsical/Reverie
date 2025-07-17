using Reverie.Content.Tiles.Canopy;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Farming;

public class SeedBedTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);

        TileObjectData.newTile.CoordinateWidth = 18;
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.DrawYOffset = -(16 - 18);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 1, 0);

        AdjTiles = [TileID.PlanterBox, TileID.ClayPot];

        TileObjectData.newTile.AnchorInvalidTiles = [TileID.MagicalIceBlock];
        TileID.Sets.IgnoredByNpcStepUp[Type] = true;

        AddMapEntry(new Color(247, 124, 124), Language.GetText("Seedbed"));
        RegisterItemDrop(ModContent.ItemType<SeedBedItem>());
        DustType = DustID.Dirt;
        HitSound = SoundID.Dig;

        TileObjectData.addTile(Type);
    }

    public override bool CanPlace(int i, int j)
    {
        var tileBelow = Framing.GetTileSafely(i, j + 1);
        return tileBelow.HasTile && (Main.tileSolid[tileBelow.TileType] || tileBelow.TileType == Type) &&
               !tileBelow.IsHalfBlock && tileBelow.Slope == 0;
    }

    public override void PostSetDefaults()
    {
        Main.tileNoSunLight[Type] = false;
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        UpdateFraming(i, j);
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!fail)
        {
            UpdateFraming(i - 1, j); // Left side
            UpdateFraming(i + 1, j); // Right side
        }
    }

    void UpdateFraming(int centerI, int centerJ)
    {
        // Skip if no seedbed at this position
        if (!IsSeedbed(centerI, centerJ))
            return;

        // Find the full extent of connected seedbeds horizontally
        int leftMost = centerI;
        int rightMost = centerI;

        // Find leftmost seedbed
        while (IsSeedbed(leftMost - 1, centerJ))
            leftMost--;

        // Find rightmost seedbed
        while (IsSeedbed(rightMost + 1, centerJ))
            rightMost++;

        // Update all tiles in the sequence
        for (int x = leftMost; x <= rightMost; x++)
        {
            if (IsSeedbed(x, centerJ))
            {
                UpdateTileFrame(x, centerJ, leftMost, rightMost);
            }
        }
    }

    void UpdateTileFrame(int i, int j, int leftMost, int rightMost)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
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

    bool IsSeedbed(int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);
        return tile.HasTile && tile.TileType == Type;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        var tileBelow = Framing.GetTileSafely(i, j + 1);
        if (!tileBelow.HasTile || (!Main.tileSolid[tileBelow.TileType] && tileBelow.TileType != Type) ||
            tileBelow.IsHalfBlock || tileBelow.Slope != 0)
        {
            WorldGen.KillTile(i, j);
            return false;
        }

        // Handle custom framing
        UpdateFraming(i, j);

        // Return false to prevent vanilla framing
        return false;
    }

    public override void RandomUpdate(int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
            return;

        if (WorldGen.genRand.NextBool(20))
        {
            var dustPos = new Vector2(i * 16 + 8, j * 16 + 8);
            var dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.Clay,
                Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, 0f));
            dust.fadeIn = 0.8f;
            dust.scale = 0.6f;
        }
    }
}