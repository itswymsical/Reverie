using Reverie.Content.Tiles.Rainforest.Surface.Trees;
using System.Collections.Generic;

namespace Reverie.Content.Items.Debugging;

public class KapokTreeDebugWand : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.rare = ItemRarityID.Expert;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer != player.whoAmI)
            return true;

        Vector2 mouseWorld = Main.MouseWorld;
        int tileX = (int)(mouseWorld.X / 16f);
        int tileY = (int)(mouseWorld.Y / 16f);

        if (player.altFunctionUse == 2) // Right click
        {
            ClearTreesInArea(tileX, tileY, 8);
            return true;
        }

        // Left click - grow tree
        bool success = false;

        // Try multiple times with slight position variations if first attempt fails
        for (int attempts = 0; attempts < 5 && !success; attempts++)
        {
            int tryX = tileX + Main.rand.Next(-1, 2);
            int tryY = tileY + attempts; // Try lower positions

            success = KapokTree.GrowKapokTree(tryX, tryY);

            if (success)
            {
                // Success feedback
                Main.NewText($"Kapok tree grown at ({tryX}, {tryY}) after {attempts + 1} attempts", Color.Green);

                // Enhanced visual effect
                Vector2 effectPos = new Vector2(tryX, tryY) * 16;
                for (int i = 0; i < 50; i++)
                {
                    var dust = Dust.NewDustDirect(effectPos, 16, 16, DustID.GrassBlades,
                        Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 1f));
                    dust.scale = 1.5f;
                    dust.fadeIn = 2f;
                }
                break;
            }
        }

        if (!success)
        {
            // Enhanced failure feedback with diagnostic info
            var groundTile = Framing.GetTileSafely(tileX, tileY + 1);
            var currentTile = Framing.GetTileSafely(tileX, tileY);

            string reason = "Unknown";
            if (!WorldGen.InWorld(tileX, tileY))
                reason = "Out of world bounds";
            else if (!groundTile.HasTile)
                reason = "No ground tile";
            else if (currentTile.HasTile && Main.tileSolid[currentTile.TileType])
                reason = "Solid tile in the way";

            Main.NewText($"Failed to grow tree at ({tileX}, {tileY}) - {reason}", Color.Red);

            // Red dust effect
            for (int i = 0; i < 20; i++)
            {
                var dust = Dust.NewDustDirect(mouseWorld, 16, 16, DustID.Blood,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                dust.scale = 1f;
            }
        }

        return true;
    }

    /// <summary>
    /// Clear trees in area around target
    /// </summary>
    private void ClearTreesInArea(int centerX, int centerY, int radius)
    {
        int cleared = 0;

        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (!WorldGen.InWorld(x, y)) continue;

                var tile = Framing.GetTileSafely(x, y);
                if (tile.HasTile && tile.TileType == ModContent.TileType<KapokTree>())
                {
                    WorldGen.KillTile(x, y, false, false, false);
                    cleared++;
                }
            }
        }

        if (cleared > 0)
        {
            Main.NewText($"Cleared {cleared} tree tiles in {radius}x{radius} area", Color.Yellow);

            // Smoke effect
            Vector2 center = new Vector2(centerX, centerY) * 16;
            for (int i = 0; i < cleared; i++)
            {
                var smoke = Dust.NewDustDirect(center + Main.rand.NextVector2Unit() * radius * 8,
                    16, 16, DustID.Smoke, 0, -1f);
                smoke.scale = 1.2f;
            }

            // Network sync
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendTileSquare(-1, centerX - radius, centerY - radius,
                    radius * 2 + 1, radius * 2 + 1, TileChangeType.None);
            }
        }
        else
        {
            Main.NewText($"No trees found in {radius}x{radius} area", Color.Gray);
        }
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "Usage", "[c/00FF00:Left Click:] Grow Kapok tree at cursor"));
        tooltips.Add(new TooltipLine(Mod, "Usage2", "[c/FF6600:Right Click:] Clear trees in area"));
        tooltips.Add(new TooltipLine(Mod, "Debug", "[c/FFFF00:Debug Tool - Auto-retries failed placements]"));
    }
}