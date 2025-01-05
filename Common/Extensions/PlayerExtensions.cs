using Microsoft.Xna.Framework;
using System;
using Terraria;
using static Terraria.Player;
using Terraria.DataStructures;
using Reverie.Common.Players;

namespace Reverie.Common.Extensions
{
    public static class PlayerExtensions
    {
        public enum CompositeHeadStretchAmount
        {
            None,
            Quarter,
            Half,
            ThreeQuarters,
            Full
        }

        public struct CompositeHeadData
        {
            public bool Enabled;
            public float Rotation;
            public Vector2 Position;
            public bool StretchHorizontally;

            public CompositeHeadData(bool enabled, float rotation, Vector2 position, bool stretchHorizontally = false)
            {
                Enabled = enabled;
                Rotation = rotation;
                Position = position;
                StretchHorizontally = stretchHorizontally;
            }
        }
        private static float GetRotationFromStretch(CompositeHeadStretchAmount stretch)
        {
            return stretch switch
            {
                CompositeHeadStretchAmount.None => 0f,
                CompositeHeadStretchAmount.Quarter => 0.4f,
                CompositeHeadStretchAmount.Half => 0.8f,
                CompositeHeadStretchAmount.ThreeQuarters => 1.2f,
                CompositeHeadStretchAmount.Full => 1.57f,
                _ => 0f
            };
        }

        public static void SetCompositeHead(this Player player, bool enabled, float rotation)
        {
            player.headRotation = rotation;
        }

        public static void SetCompositeHead(this Player player, bool enabled, Vector2 positionOffset, float rotation)
        {
            player.headRotation = rotation;
            player.headPosition += positionOffset;
        }

        public static void SetCompositeHead(this Player player, bool enabled, CompositeHeadStretchAmount stretch)
        {
            float rotation = GetRotationFromStretch(stretch);
            player.headRotation = rotation * (float)player.direction;
        }

        public static void SetCompositeHead(this Player player, bool enabled, CompositeHeadStretchAmount stretch, Vector2 positionOffset, bool stretchHorizontally = false)
        {
            float rotation = GetRotationFromStretch(stretch);
            player.headRotation = rotation * (float)player.direction;
            player.headPosition += positionOffset;

            if (stretchHorizontally)
            {
                // Implementation for head stretching would go here
            }
        }

        public static void SetCompositeHead(this Player player, CompositeHeadData headData)
        {
            player.headRotation = headData.Rotation * (float)player.direction;
            player.headPosition += headData.Position;

            if (headData.StretchHorizontally)
            {
                // Implementation for head stretching would go here
            }
        }
        public static bool IsMoving(this Player player) => player.velocity.Length() > 0.1f;
        public static bool IsGrappling(this Player player) => player.controlHook && !player.releaseHook;
        public static bool IsJumping(this Player player)
        {
            if (!player.controlJump) return false;

            bool canJump = (player.sliding || player.velocity.Y == 0f || player.AnyExtraJumpUsable()) &&
                           (player.releaseJump || (player.autoJump && (player.velocity.Y == 0f || player.sliding)));

            if (!canJump && player.jump <= 0) return false;

            return true;
        }
        public static bool IsUsingItem(this Player player)
        {
            bool consideredUsing = (player.HeldItem.holdStyle != 0 || player.ItemAnimationActive);

            if (!consideredUsing && player.ItemAnimationEndingOrEnded) return false;

            return true;
        }
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
        public static void SetDynamicArms(this Player player, ref PlayerDrawSet drawInfo, float rotation, float stretchAmount)
        {
            int stretchInt = (int)(stretchAmount * 4);
            stretchInt = Math.Clamp(stretchInt, 0, 4);

            var stretchEnum = (CompositeArmStretchAmount)stretchInt;

            if (player.direction == -1)
            {
                drawInfo.drawPlayer.SetCompositeArmFront(true, stretchEnum, -(rotation - 0.1f));
                drawInfo.drawPlayer.SetCompositeArmBack(true, stretchEnum, -(rotation + 0.1f));
            }
            else
            {
                drawInfo.drawPlayer.SetCompositeArmFront(true, stretchEnum, rotation - 0.1f);
                drawInfo.drawPlayer.SetCompositeArmBack(true, stretchEnum, rotation + 0.1f);
            }
        }
    }
}
