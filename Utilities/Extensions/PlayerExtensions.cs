﻿using static Terraria.Player;
using Terraria.DataStructures;

namespace Reverie.Utilities.Extensions;

/// <summary>
///     Provides <see cref="Player"/> extension methods.
/// </summary>
public static class PlayerExtensions
{
    /// <summary>
    ///     Attemps to dig a <see cref="Tile"/> area at the specified coordinates.
    /// </summary>
    /// <param name="i">The horizontal coordinates of the origin.</param>
    /// <param name="j">The vertical coordinates of the origin.</param>
    /// <param name="width">The width of the area, in tiles.</param>
    /// <param name="height">The height of the area, in tiles.</param>
    /// <param name="power">The digging power to use.</param>
    public static void DigArea(this Player player, int i, int j, int power)
    {
        int tileX = i / 16;
        int tileY = j / 16;

        for (int num = -1; num < 2; num++)
        {
            int newX = tileX + num;
            int newY = tileY + num;

            if (newX >= 0 && newX < Main.maxTilesX)
                player.DigTile(newX, tileY, power);

            if (newY >= 0 && newY < Main.maxTilesY)
                player.DigTile(tileX, newY, power);
        }
    }

    /// <summary>
    ///     Attempts to dig a <see cref="Tile"/> at the specified coordinates.
    /// </summary>
    /// <param name="i">The horizontal coordinates of the tile.</param>
    /// <param name="j">The vertical coordinates of the tile.</param>
    /// <param name="power">The digging power to use.</param>
    public static void DigTile(this Player player, int i, int j, int power)
    {
        var type = Main.tile[i, j].TileType;

        if (Main.tileAxe[type] || Main.tileHammer[type])
        {
            return;
        }
        
        player.PickTile(i, j, power);
        
        if (TileID.Sets.CanBeDugByShovel[type])
        {
            return;
        }
        
        player.PickTile(i, j, power - (power / 2));
    }

    /// 
    /// <summary>
    ///     Attempts to position/rotate the <see cref="Player"/> head.
    /// </summary>
    public static void SetCompositeHead(this Player player, bool enabled, float rotation)
    {
        player.headRotation = rotation;
    }

    /// 
    /// <summary>
    ///     Attempts to position/rotate the <see cref="Player"/> head, with a position offset.
    /// </summary>
    public static void SetCompositeHead(this Player player, bool enabled, Vector2 positionOffset, float rotation)
    {
        player.headRotation = rotation;
        player.headPosition += positionOffset;
    }

    /// 
    /// <summary>
    ///     Checks if the <see cref="Player"/> is actively moving. Used for animations.
    /// </summary>
    public static bool IsMoving(this Player player) => player.velocity.Length() > 0.1f;

    /// 
    /// <summary>
    ///     Checks if the <see cref="Player"/> is actively using a grappling hook. Used for animations.
    /// </summary>
    public static bool IsGrappling(this Player player) => player.controlHook && !player.releaseHook;

    /// 
    /// <summary>
    ///     Checks if the <see cref="Player"/> is actively jumping. Used for animations.
    /// </summary>
    public static bool IsJumping(this Player player)
    {
        if (!player.controlJump) return false;

        bool canJump = (player.sliding || player.velocity.Y == 0f || player.AnyExtraJumpUsable()) &&
                       (player.releaseJump || (player.autoJump && (player.velocity.Y == 0f || player.sliding)));

        if (!canJump && player.jump <= 0) return false;

        return true;
    }

    /// 
    /// <summary>
    ///     Checks if the <see cref="Player"/> is actively using an <see cref="Item"/>. Used for animations.
    /// </summary>
    public static bool IsUsingItem(this Player player)
    {
        bool consideredUsing = (player.HeldItem.holdStyle != 0 || player.ItemAnimationActive);

        if (!consideredUsing && player.ItemAnimationEndingOrEnded) return false;

        return true;
    }

    /// 
    /// <summary>
    ///     Attempts to create an arm swaying animation with <see cref="Player.SetCompositeArmBack(bool, CompositeArmStretchAmount, float)"/> 
    ///     and <see cref="Player.SetCompositeArmFront(bool, CompositeArmStretchAmount, float)"/>. Used for animations.
    /// </summary>
    public static void ArmSway(this Player player, ref PlayerDrawSet drawInfo, float speed, float amplitude)
    {
        float currentTime = Main.GameUpdateCount / 60f;
        float animationTime = currentTime - Main.GameUpdateCount / 60f;
        float cycle = (float)Math.Sin(animationTime * speed);
        float frontArmOffset = (float)Math.Sin((animationTime + 0.1f) * speed);

        float backArmRotation = cycle * amplitude;
        float frontArmRotation = frontArmOffset * amplitude;
        if (player.direction == -1)
        {
            drawInfo.drawPlayer.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, backArmRotation);
            drawInfo.drawPlayer.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, -frontArmRotation);
        }
        else
        {
            drawInfo.drawPlayer.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, -backArmRotation);
            drawInfo.drawPlayer.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, frontArmRotation);
        }
    }
    /// 
    /// <summary>
    ///     Attempts to create an grappling animation with <see cref="Player.SetCompositeArmBack(bool, CompositeArmStretchAmount, float)"/> 
    ///     and <see cref="Player.SetCompositeArmFront(bool, CompositeArmStretchAmount, float)"/>. Used for animations.
    /// </summary>
    public static void GrappleAnimation(this Player player, ref PlayerDrawSet drawInfo)
    {
        float armRotation = -MathHelper.PiOver2 + 0.2f;

        if (player.direction == -1)
        {
            drawInfo.drawPlayer.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, -(armRotation + 0.2f));
            drawInfo.drawPlayer.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, -(armRotation - 0.2f));
        }
        else
        {
            drawInfo.drawPlayer.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, armRotation - 0.2f);
            drawInfo.drawPlayer.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, armRotation + 0.2f);
        }
    }

    /// 
    /// <summary>
    ///     Attempts to draw the <see cref="Player.HeldItem"/> behind the  <see cref="Player"/>, whilst holstered. Used for animations.
    /// </summary>
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

    /// 
    /// <summary>
    ///     Attempts to draw the <see cref="Player.HeldItem"/> in the <see cref="Player"/> offhand. Used for animations.
    /// </summary>
    public static void DrawItemInFrontHand(this Player player, ref PlayerDrawSet drawInfo)
    {
        if (player.HeldItem == null || player.HeldItem.IsAir || player.HeldItem.damage > 0) return;

        Item item = player.HeldItem;
        Main.instance.LoadItem(item.type);

        Texture2D itemTexture = ModContent.Request<Texture2D>(
            item.ModItem?.Texture ?? $"Terraria/Images/Item_{item.type}"
        ).Value;

        Vector2 itemPosition = player.HandPosition.Value - Main.screenPosition;

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