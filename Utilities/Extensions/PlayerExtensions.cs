﻿using static Terraria.Player;
using Terraria.DataStructures;
using System.Collections.Generic;

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
}