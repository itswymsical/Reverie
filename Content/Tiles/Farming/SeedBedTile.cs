using Reverie.Common.MonoMod;
using Terraria.DataStructures;
using Terraria.Enums;
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

        // Register this tile type for seedbed behavior
        SeedBedPlanter.SeedBedTypes.Add(Type);
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
        // Trigger framing update for this tile and neighbors
        UpdateFramingArea(i, j);
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!fail)
        {
            // Update framing for neighboring tiles after this one is removed
            UpdateFramingArea(i - 1, j);
            UpdateFramingArea(i + 1, j);
        }
    }

    private void UpdateFramingArea(int i, int j)
    {
        // Force a frame update by calling TileFrame on the tile
        if (WorldGen.InWorld(i, j))
        {
            var tile = Main.tile[i, j];
            if (tile.HasTile == true && SeedBedPlanter.SeedBedTypes.Contains(tile.TileType))
            {
                bool resetFrame = false;
                bool noBreak = false;
                WorldGen.TileFrame(i, j, resetFrame, noBreak);
            }
        }
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