using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Reverie.Common.Players
{
    public class AnimationPlayer : ModPlayer
    {
        private float breathingSpeed = 2.5f;
        private float armAmplitude = 0.1f;
        private float bodyAmplitude = 0.05f;
        private float startTime;
        private bool wasMoving = true;
        private bool wasUsingItem = false;

        private float defaultBodyRotation;
        private Vector2 defaultBodyPosition;
        private Vector2 defaultLegPosition;
        private float defaultLegRotation;
        private Vector2 defaultHeadPosition;
        private float defaultHeadRotation;

        private bool isIdle = false;
        private static bool active = true;
        private bool needsReset = false;

        public override void Initialize()
        {
            startTime = Main.GameUpdateCount / 60f;
            SaveDefaultPositions();
        }

        private void SaveDefaultPositions()
        {
            defaultBodyRotation = Player.bodyRotation;
            defaultBodyPosition = Player.bodyPosition;
            defaultLegPosition = Player.legPosition;
            defaultLegRotation = Player.legRotation;
            defaultHeadPosition = Player.headPosition;
            defaultHeadRotation = Player.headRotation;
        }

        public override void PostUpdate()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isMoving = Player.velocity != Vector2.Zero;
            bool isUsingItem = Player.controlUseItem;

            if (isMoving || isUsingItem != wasUsingItem)
            {
                wasMoving = true;
                wasUsingItem = isUsingItem;
                startTime = Main.GameUpdateCount / 60f;
                isIdle = false;
                needsReset = true;
            }
            else if (!isMoving && !isUsingItem && wasMoving)
            {
                wasMoving = false;
            }

            if (!isMoving && !isUsingItem && Player.itemAnimation <= 0)
            {
                isIdle = true;
            }
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (!Main.gameMenu && active)
            {
                if (needsReset)
                {
                    ResetPositions(ref drawInfo);
                    needsReset = false;
                }

                if (isIdle)
                {
                    ApplyIdleAnimation(ref drawInfo);
                }
            }
        }

        private void ResetPositions(ref PlayerDrawSet drawInfo)
        {
            drawInfo.drawPlayer.bodyRotation = defaultBodyRotation;
            drawInfo.drawPlayer.bodyPosition = defaultBodyPosition;
            drawInfo.drawPlayer.legPosition = defaultLegPosition;
            drawInfo.drawPlayer.legRotation = defaultLegRotation;
            drawInfo.drawPlayer.headPosition = defaultHeadPosition;
            drawInfo.drawPlayer.headRotation = defaultHeadRotation;
        }

        private void ApplyIdleAnimation(ref PlayerDrawSet drawInfo)
        {
            float currentTime = Main.GameUpdateCount / 60f;
            float animationTime = currentTime - startTime;
            float breathingCycle = (float)Math.Sin(animationTime * breathingSpeed);
            float frontArmOffset = (float)Math.Sin((animationTime + 0.2f) * breathingSpeed);

            drawInfo.drawPlayer.bodyRotation = breathingCycle * (bodyAmplitude);

            float headPosition = breathingCycle * armAmplitude * 6.2f;
            if (Player.direction == -1)
            {
                drawInfo.drawPlayer.SetCompositeHead(true, rotation: 0.02f, new Vector2(0, -headPosition));
            }
            else
            {
                drawInfo.drawPlayer.SetCompositeHead(true, rotation: -0.02f, new Vector2(0, headPosition));
            }

            /* ROCK AND ROLL
            drawInfo.drawPlayer.SetCompositeHead(true, CompositeHeadStretchAmount.Half, new Vector2(0, 5));
            */

            float backArmRotation = breathingCycle * armAmplitude;
            float frontArmRotation = frontArmOffset * armAmplitude;
            if (Player.direction == -1)
            {
                drawInfo.drawPlayer.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, backArmRotation);
                drawInfo.drawPlayer.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -frontArmRotation);
            }
            else
            {
                drawInfo.drawPlayer.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, -backArmRotation);
                drawInfo.drawPlayer.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);
            }
        }

        /*
        private void ApplyNewAnimation(ref PlayerDrawSet drawInfo)
        {
            float currentTime = Main.GameUpdateCount / 60f;
            float animationTime = currentTime - startTime;
        }
        */
    }

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

    public static class PlayerHeadExtensions
    {
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

        public static void SetCompositeHead(this Player player, bool enabled, float rotation, Vector2 positionOffset)
        {
            player.headRotation = rotation;
            player.headPosition += positionOffset;
        }

        public static void SetCompositeHead(this Player player, bool enabled, CompositeHeadStretchAmount stretch)
        {
            float rotation = GetRotationFromStretch(stretch);
            player.headRotation = rotation * (float)player.direction;
        }

        // Most complete version with all parameters
        public static void SetCompositeHead(this Player player, bool enabled, CompositeHeadStretchAmount stretch, Vector2 positionOffset, bool stretchHorizontally = false)
        {
            float rotation = GetRotationFromStretch(stretch);
            player.headRotation = rotation * (float)player.direction;
            player.headPosition += positionOffset;

            // You could implement horizontal stretching here if needed
            // This would require modifications to the player's draw code
            if (stretchHorizontally)
            {
                // Implementation for head stretching would go here
                // This might require additional sprite manipulation
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
    }
}
