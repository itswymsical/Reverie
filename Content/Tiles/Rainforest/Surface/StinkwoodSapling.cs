using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Rainforest.Surface;

public class StinkwoodSapling : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 2;

        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;

        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<CanopyGrassTile>(), ModContent.TileType<WoodgrassTile>()];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawFlipHorizontal = true;
        TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.StyleMultiplier = 3;

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Sapling"));

        TileID.Sets.TreeSapling[Type] = true;
        TileID.Sets.CommonSapling[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        DustType = DustID.RichMahogany;

        AdjTiles = [TileID.Saplings];
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override void RandomUpdate(int i, int j)
    {
        // Use palm tree growth chance (similar to vanilla palm saplings)
        if (WorldGen.genRand.NextBool(8))
        {
            TryGrowTree(i, j);
        }
    }

    // Override the Update method to handle fertilizer immediately
    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (!closer) return;

        // Check if any active fertilizer projectiles intersect with this tile
        for (int k = 0; k < Main.maxProjectiles; k++)
        {
            Projectile proj = Main.projectile[k];
            if (proj.active && proj.type == ProjectileID.Fertilizer)
            {
                Rectangle tileRect = new Rectangle(i * 16, j * 16, 32, 32); // Slightly larger hitbox
                if (proj.Hitbox.Intersects(tileRect))
                {
                    // Debug message - remove this later
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        Main.NewText($"Fertilizer detected at ({i}, {j})! Attempting growth...", Color.Yellow);
                        Main.NewText($"Can grow: {CanGrowTree(i, j)}", Color.Lime);
                    }

                    // Force immediate growth when fertilizer is present
                    TryGrowTree(i, j);
                    break;
                }
            }
        }
    }

    // Add right-click debug info
    public override bool RightClick(int i, int j)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            Main.NewText($"Sapling at ({i}, {j})", Color.White);
            Main.NewText($"Can grow: {CanGrowTree(i, j)}", Color.Yellow);
            Main.NewText($"Tile below: {Framing.GetTileSafely(i, j + 1).TileType}", Color.Cyan);
            Main.NewText($"Space above: {HasSpaceAbove(i, j)}", Color.Orange);
        }
        return true;
    }

    private bool HasSpaceAbove(int i, int j)
    {
        for (int checkY = j - 1; checkY >= j - 8; checkY--)
        {
            if (!WorldGen.InWorld(i, checkY))
                return false;

            Tile checkTile = Framing.GetTileSafely(i, checkY);
            if (checkTile.HasTile && Main.tileSolid[checkTile.TileType])
            {
                return false;
            }
        }
        return true;
    }

    private void TryGrowTree(int i, int j)
    {
        var isPlayerNear = WorldGen.PlayerLOS(i, j);

        // Check growth conditions first
        if (!CanGrowTree(i, j))
        {
            return;
        }

        // First try GrowPalmTree
        bool success = WorldGen.GrowPalmTree(i, j);

        // If that fails, try the regular GrowTree as fallback
        if (!success)
        {
            success = WorldGen.GrowTree(i, j);
        }

        if (success && isPlayerNear)
        {
            WorldGen.TreeGrowFXCheck(i, j);
        }
    }

    private bool CanGrowTree(int i, int j)
    {
        // Check if this is actually our sapling
        if (Main.tile[i, j].TileType != Type)
            return false;

        // Check if there's a valid tile below
        Tile tileBelow = Framing.GetTileSafely(i, j + 1);
        if (!tileBelow.HasTile ||
            (tileBelow.TileType != ModContent.TileType<CanopyGrassTile>() &&
             tileBelow.TileType != ModContent.TileType<WoodgrassTile>()))
        {
            return false;
        }

        // Check if there's enough space above (at least 8 tiles)
        for (int checkY = j - 1; checkY >= j - 8; checkY--)
        {
            if (!WorldGen.InWorld(i, checkY))
                return false;

            Tile checkTile = Framing.GetTileSafely(i, checkY);
            if (checkTile.HasTile && Main.tileSolid[checkTile.TileType])
            {
                return false;
            }
        }

        return true;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects)
    {
        if (i % 2 == 1)
        {
            effects = SpriteEffects.FlipHorizontally;
        }
    }
}