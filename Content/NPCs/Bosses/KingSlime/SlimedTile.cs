namespace Reverie.Content.NPCs.Bosses.KingSlime;

public class SlimedTile : GlobalTile
{
    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        if (SlimedTileSystem.IsSlimed(i, j))
        {
            Texture2D slimeOverlay = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}Tiles/SlimeOverlay").Value;

            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPos = new Vector2(i * 16, j * 16) + zero - Main.screenPosition;
            Tile tile = Framing.GetTileSafely(i, j);

            float intensity = SlimedTileSystem.GetSlimeIntensity(i, j);
            Color drawColor = Color.White * intensity;

            spriteBatch.Draw(slimeOverlay, drawPos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White * intensity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        }
    } 

    public override void FloorVisuals(int type, Player player)
    {
        int tileX = (int)(player.Center.X / 16f);
        int tileY = (int)((player.position.Y + player.height) / 16f);

        if (SlimedTileSystem.IsSlimed(tileX, tileY))
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
