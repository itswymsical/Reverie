using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace Reverie.Content.Tiles.AssailantStronghold;

public class CommandTent : ModTile
{
    public override string Texture => PLACEHOLDER;
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileHammer[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 10;
        TileObjectData.newTile.Height = 6;

        TileObjectData.newTile.Origin = new Point16(5, 5);
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16];

        TileObjectData.newTile.UsesCustomCanPlace = true;
        AddMapEntry(new Color(196, 117, 190), CreateMapEntryName());
        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;

        TileObjectData.addTile(Type);
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        Tile tile = Framing.GetTileSafely(i, j);
        var lightColor = Lighting.GetColor(i, j);
        if (tile.TileFrameX != 0 || tile.TileFrameY != 0)
            return false;

        var tentOuterTexture = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}Tiles/AssailantStronghold/CommandTent_Outer").Value;
        var tentInnerTexture = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}Tiles/AssailantStronghold/CommandTent_Inner").Value;
        int tileWidth = 10 * 16;
        int tileHeight = 6 * 16;

        Vector2 textureOrigin = new Vector2(tentOuterTexture.Width / 2f, tentOuterTexture.Height / 2f);
        Vector2 worldCenter = new Vector2(i * 16 + tileWidth / 2f, j * 16 + tileHeight / 2f);
        Vector2 drawPos = worldCenter - Main.screenPosition + zero;
        bool flipped = (i % 2 == 1);
        SpriteEffects tentEffect = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Player player = Main.LocalPlayer;
        Rectangle tentBounds = new Rectangle(i * 16, j * 16, tileWidth, tileHeight);
        float outerAlpha = 1f;
        bool playerInside = tentBounds.Contains(player.Center.ToPoint());

        if (playerInside)
        {
            Vector2 tentCenter = new Vector2(tentBounds.Center.X, tentBounds.Center.Y);
            float distFromCenter = Vector2.Distance(player.Center, tentCenter);
            float maxFadeRange = Math.Min(tileWidth, tileHeight);

            float fadeProgress = distFromCenter / maxFadeRange;
            outerAlpha = MathHelper.Lerp(0.15f, 1f, fadeProgress);
        }

        var leftPole = ModContent.Request<Texture2D>(
            $"{TEXTURE_DIRECTORY}Tiles/AssailantStronghold/CommandTent_RopeL").Value;
        var rightPole = ModContent.Request<Texture2D>(
            $"{TEXTURE_DIRECTORY}Tiles/AssailantStronghold/CommandTent_RopeR").Value;

        Vector2 poleOrigin = new Vector2(leftPole.Width / 2f, leftPole.Height / 2f);
        Vector2 leftOffset = new Vector2(-tentOuterTexture.Width / 2f - 8, tentOuterTexture.Height / 2f - leftPole.Height / 2f + 2);
        Vector2 rightOffset = new Vector2(tentOuterTexture.Width / 2f - 8, tentOuterTexture.Height / 2f - rightPole.Height / 2f + 2);

        if (flipped)
        {
            rightOffset = new Vector2(-tentOuterTexture.Width / 2f + 4, tentOuterTexture.Height / 2f - leftPole.Height / 2f + 2);
            leftOffset = new Vector2(tentOuterTexture.Width / 2f + 8, tentOuterTexture.Height / 2f - rightPole.Height / 2f + 2);
        }

        spriteBatch.Draw(rightPole, drawPos + rightOffset, null, lightColor, 0f, poleOrigin, 1f, tentEffect, 0f);
        spriteBatch.Draw(tentInnerTexture, new Vector2(drawPos.X, drawPos.Y + 2), null, lightColor, 0f, textureOrigin, 1f, tentEffect, 0f);
        spriteBatch.Draw(tentOuterTexture, new Vector2(drawPos.X, drawPos.Y + 2), null, lightColor * outerAlpha, 0f, textureOrigin, 1f, tentEffect, 0f);
        spriteBatch.Draw(leftPole, drawPos + leftOffset, null, lightColor, 0f, poleOrigin, 1f, tentEffect, 0f);

        if (playerInside)
        {
            Rectangle screenBounds = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            Vector2 tentCenter = new Vector2(tentBounds.Center.X, tentBounds.Center.Y);
            float distFromCenter = Vector2.Distance(player.Center, tentCenter);
            float maxFadeRange = Math.Min(tileWidth, tileHeight) / 2f;
            float fadeProgress = distFromCenter / maxFadeRange;

            float overlayStrength = MathHelper.Lerp(0.6f, 0f, fadeProgress);
            Color overlayColor = Color.Black * overlayStrength;
        }
        
        return false;
    }
}

public class GruntTent : ModTile
{
    public override string Texture => PLACEHOLDER;
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileHammer[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 10;
        TileObjectData.newTile.Height = 5;

        TileObjectData.newTile.Origin = new Point16(5, 4);
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];

        TileObjectData.newTile.UsesCustomCanPlace = true;
        AddMapEntry(new Color(196, 117, 190), CreateMapEntryName());
        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;

        TileObjectData.addTile(Type);
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        Tile tile = Framing.GetTileSafely(i, j);
        var lightColor = Lighting.GetColor(i, j);
        if (tile.TileFrameX != 0 || tile.TileFrameY != 0)
            return false;

        var tentOuterTexture = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}Tiles/AssailantStronghold/GruntTent_Outer").Value;
        var tentInnerTexture = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}Tiles/AssailantStronghold/GruntTent_Inner").Value;
        int tileWidth = 10 * 16;
        int tileHeight = 6 * 16;

        Vector2 textureOrigin = new Vector2(tentOuterTexture.Width / 2f, tentOuterTexture.Height / 2f);
        Vector2 worldCenter = new Vector2(i * 16 + tileWidth / 2f, j * 16 + tileHeight / 2f);
        Vector2 drawPos = worldCenter - Main.screenPosition + zero;
        bool flipped = (i % 2 == 1);
        SpriteEffects tentEffect = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Player player = Main.LocalPlayer;
        Rectangle tentBounds = new Rectangle(i * 16, j * 16, tileWidth, tileHeight);
        float outerAlpha = 1f;
        bool playerInside = tentBounds.Contains(player.Center.ToPoint());

        if (playerInside)
        {
            Vector2 tentCenter = new Vector2(tentBounds.Center.X, tentBounds.Center.Y);
            float distFromCenter = Vector2.Distance(player.Center, tentCenter);
            float maxFadeRange = Math.Min(tileWidth, tileHeight);

            float fadeProgress = distFromCenter / maxFadeRange;
            outerAlpha = MathHelper.Lerp(0.15f, 1f, fadeProgress);
        }

        var leftPole = ModContent.Request<Texture2D>(
            $"{TEXTURE_DIRECTORY}Tiles/AssailantStronghold/GruntTent_RopeL").Value;
        var rightPole = ModContent.Request<Texture2D>(
            $"{TEXTURE_DIRECTORY}Tiles/AssailantStronghold/GruntTent_RopeR").Value;

        Vector2 poleOrigin = new Vector2(leftPole.Width / 2f, leftPole.Height / 2f);
        Vector2 leftOffset = new Vector2(-tentOuterTexture.Width / 2f + 4, tentOuterTexture.Height / 2f - leftPole.Height / 2f + 2);
        Vector2 rightOffset = new Vector2(tentOuterTexture.Width / 2f - 16, tentOuterTexture.Height / 2f - rightPole.Height / 2f + 2);

        if (flipped)
        {
            rightOffset = new Vector2(-tentOuterTexture.Width / 2f + 16, tentOuterTexture.Height / 2f - leftPole.Height / 2f + 2);
            leftOffset = new Vector2(tentOuterTexture.Width / 2f - 8, tentOuterTexture.Height / 2f - rightPole.Height / 2f + 2);
        }

        spriteBatch.Draw(rightPole, drawPos + rightOffset, null, lightColor, 0f, poleOrigin, 1f, tentEffect, 0f);
        spriteBatch.Draw(tentInnerTexture, new Vector2(drawPos.X, drawPos.Y + 2), null, lightColor, 0f, textureOrigin, 1f, tentEffect, 0f);
        spriteBatch.Draw(tentOuterTexture, new Vector2(drawPos.X, drawPos.Y + 2), null, lightColor * outerAlpha, 0f, textureOrigin, 1f, tentEffect, 0f);
        spriteBatch.Draw(leftPole, drawPos + leftOffset, null, lightColor, 0f, poleOrigin, 1f, tentEffect, 0f);

        if (playerInside)
        {
            Rectangle screenBounds = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            Vector2 tentCenter = new Vector2(tentBounds.Center.X, tentBounds.Center.Y);
            float distFromCenter = Vector2.Distance(player.Center, tentCenter);
            float maxFadeRange = Math.Min(tileWidth, tileHeight) / 2f;
            float fadeProgress = distFromCenter / maxFadeRange;

            float overlayStrength = MathHelper.Lerp(0.6f, 0f, fadeProgress);
            Color overlayColor = Color.Black * overlayStrength;
        }

        return false;
    }
}