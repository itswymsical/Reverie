using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria;
using System;

public static class ItemDisplayExtensions
{
    public static void DrawItemBehindPlayer(this Player player, ref PlayerDrawSet drawInfo)
    {
        if (player.HeldItem == null || player.HeldItem.IsAir) return;

        Item item = player.HeldItem;
        Main.instance.LoadItem(item.type);
        Texture2D itemTexture = ModContent.Request<Texture2D>(item.ModItem?.Texture ?? $"Terraria/Images/Item_{item.type}").Value;
        Vector2 itemPosition = player.Center - Main.screenPosition;
        itemPosition.X -= itemTexture.Width / 16;
        itemPosition.Y -= itemTexture.Height / 16;
        float tilt = player.velocity.X * 0.125f;
        float currentTime = Main.GameUpdateCount / 60f;
        float animationTime = currentTime - Main.GameUpdateCount / 60f;

        float dir = MathHelper.ToRadians(182) * player.direction;
        SpriteEffects flip = player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Color lightColor = Lighting.GetColor(
            (int)((player.Center.X) / 16f),
            (int)((player.Center.Y) / 16f)
        );

        Color itemColor = item.GetAlpha(lightColor);

        DrawData itemDrawData = new(
            itemTexture,
            itemPosition,
            null,
            itemColor,
            player.bodyRotation + dir,
            new Vector2(itemTexture.Width / 2, itemTexture.Height / 2),
            1.07f,
            flip,
            0
        );
        drawInfo.DrawDataCache.Insert(0, itemDrawData);
    }

    public static void DrawItemInFrontHand(this Player player, ref PlayerDrawSet drawInfo)
    {
        if (player.HeldItem == null || player.HeldItem.IsAir || player.HeldItem.damage > 0) return;

        Item item = player.HeldItem;
        Main.instance.LoadItem(item.type);

        Texture2D itemTexture = ModContent.Request<Texture2D>(
            item.ModItem?.Texture ?? $"Terraria/Images/Item_{item.type}"
        ).Value;

        Vector2 itemPosition = player.Center - Main.screenPosition;

        itemPosition.X += (player.direction == 1 ? -10f : 10f);
        itemPosition.Y += 8f;

        float rotation = player.bodyRotation;

        SpriteEffects flip = player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Color lightColor = Lighting.GetColor(
            (int)((player.Center.X) / 16f),
            (int)((player.Center.Y) / 16f)
        );

        Color itemColor = item.GetAlpha(lightColor);

        DrawData itemDrawData = new(
            itemTexture,
            itemPosition,
            null,
            itemColor,
            rotation,
            new Vector2(itemTexture.Width / 2, itemTexture.Height / 2),
            1f,
            flip,
            0
        );

        drawInfo.DrawDataCache.Add(itemDrawData);
    }
}