using System.Collections.Generic;

namespace Reverie.Content.NPCs.Bosses.KingSlime;

public class SlimedTile : GlobalTile
{
    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        if (SlimedTileSystem.IsSlimed(i, j) || SlimedTileSystem.IsNPCSlimed(i, j))
        {
            Texture2D slimeOverlay = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}Tiles/SlimeOverlay").Value;
            Texture2D slimePlatform  = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}Tiles/SlimeOverlay_Platform").Value;

            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPos = new Vector2(i * 16, j * 16) + zero - Main.screenPosition;
            Tile tile = Framing.GetTileSafely(i, j);

            float intensity = SlimedTileSystem.GetSlimeIntensity(i, j);

            if (SlimedTileSystem.IsNPCSlimed(i, j))
                intensity = SlimedTileSystem.GetNPCSlimeIntensity(i, j);
            
            Color drawColor = Color.White * (intensity + 0.15f);
            if (!TileID.Sets.Platforms[tile.TileType] || !Main.tileSolidTop[tile.TileType])
                spriteBatch.Draw(slimeOverlay, drawPos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            else
                spriteBatch.Draw(slimePlatform, drawPos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

        }
    }

    public override void FloorVisuals(int type, Player player)
    {
        int tileX = (int)(player.Center.X / 16f);
        int tileY = (int)((player.position.Y + player.height) / 16f);

        if (SlimedTileSystem.IsSlimed(tileX, tileY) && !SlimedTileSystem.IsNPCSlimed(tileX, tileY))
        {
            player.AddBuff(BuffID.Slimed, 2);
            player.AddBuff(BuffID.Slow, 2);

            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height,
                    DustID.t_Slime, 0f, 0f, 150, new Color(86, 162, 255, 100), 0.8f);
                dust.velocity *= 0.3f;
                dust.noGravity = true;
            }
        }
    }
}
public class SlimedGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override void PostAI(NPC npc)
    {
        int tileX = (int)(npc.Center.X / 16f);
        int tileY = (int)((npc.position.Y + npc.height) / 16f);

        if (SlimedTileSystem.IsNPCSlimed(tileX, tileY) && (npc.aiStyle != NPCAIStyleID.Slime 
            || npc.TypeName.Contains("slime") || npc.TypeName.Contains("Slime")))
        {
            npc.velocity *= 0.90f;

            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                    DustID.t_Slime, 0f, 0f, 150, new Color(86, 162, 255, 100), 0.8f);
                dust.velocity *= 0.3f;
                dust.noGravity = true;
            }
        }
    }
}
