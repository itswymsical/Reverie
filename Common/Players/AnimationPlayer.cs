using Microsoft.Xna.Framework;
using Reverie.Common.Extensions;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.Player;

namespace Reverie.Common.Players
{
    public enum AnimationState
    {
        Idle,
        Jumping,
        Walking,
        Dash,
        Drink,
        Swing
    }

    public class AnimationPlayer : ModPlayer
    {
        private AnimationState currentState = AnimationState.Idle;
        private float startTime;
        private bool needsReset = false;

        private float defaultBodyRotation;
        private Vector2 defaultBodyPosition;
        private Vector2 defaultLegPosition;
        private float defaultLegRotation;
        private Vector2 defaultHeadPosition;
        private float defaultHeadRotation;


        private void SaveDefaultPositions()
        {
            defaultBodyRotation = Player.bodyRotation;
            defaultBodyPosition = Player.bodyPosition;
            defaultLegPosition = Player.legPosition;
            defaultLegRotation = Player.legRotation;
            defaultHeadPosition = Player.headPosition;
            defaultHeadRotation = Player.headRotation;
        }

        public override void Initialize()
        {
            startTime = Main.GameUpdateCount / 60f;
            SaveDefaultPositions();
        }

        private void UpdateAnimationState()
        {

            if (Player.IsMoving())
            {
                currentState = AnimationState.Walking;
            }
            else
            {
                currentState = AnimationState.Idle;
            }
            needsReset = true;
        }

        public override void PostUpdate() => UpdateAnimationState();

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (!drawInfo.drawPlayer.IsUsingItem())
                drawInfo.drawPlayer.DrawItemBehindPlayer(ref drawInfo);
            if (drawInfo.drawPlayer.IsJumping() && !drawInfo.drawPlayer.IsUsingItem())
                drawInfo.drawPlayer.bodyFrame.Y = 0 * 56;
            if (Player.IsGrappling())
                drawInfo.drawPlayer.GrappleAnimation(ref drawInfo);         

            if (!Main.gameMenu && !Player.sleeping.isSleeping)
            {
                if (needsReset)
                {
                    ResetPositions(ref drawInfo);
                    needsReset = false;
                }

                switch (currentState)
                {
                    case AnimationState.Idle:
                        ApplyIdleAnimation(ref drawInfo);
                        break;
                    case AnimationState.Walking:
                        ApplyWalkAnimation(ref drawInfo);
                        break;
                    default:
                        break;
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
            drawInfo.drawPlayer.fullRotation = 0f;
        }

        private void ApplyIdleAnimation(ref PlayerDrawSet drawInfo)
        {
            float speed = 4.5f;
            float armAmplitude = 0.1f;

            float currentTime = Main.GameUpdateCount / 60f;
            float animationTime = currentTime - startTime;
            float cycle = (float)Math.Sin(animationTime * speed) * Player.direction;

            float headPosition = cycle * armAmplitude * 6.2f;

            drawInfo.drawPlayer.bodyRotation = cycle * 0.05f;
            drawInfo.drawPlayer.SetCompositeHead(true, new Vector2(0, 0.95f + (headPosition * Player.direction)), rotation: 0.0275f);
            if (!Player.IsUsingItem())
            {
                drawInfo.drawPlayer.ArmSway(ref drawInfo, speed, armAmplitude);
            }
        }

        private void ApplyWalkAnimation(ref PlayerDrawSet drawInfo)
        {
            float speed = 4.5f;
            float headAmplitude = 0.1f;

            float currentTime = Main.GameUpdateCount / 60f;
            float animationTime = currentTime - Main.GameUpdateCount / 60f;
            float cycle = (float)Math.Sin(animationTime * speed);
            float headPosition = cycle * headAmplitude * 6.2f;

            drawInfo.drawPlayer.SetCompositeHead(true, new Vector2(0, 0.95f + (headPosition * Player.direction)), rotation: 0.0275f);

            float rotVelX = (float)Math.Sin((animationTime + 0.1f) * Player.velocity.X);
            float offset = .87f * Player.direction;

            if (Player.IsJumping())
            {
                offset = 0f;
                drawInfo.drawPlayer.ArmSway(ref drawInfo, speed, 0.759f);
            }

            drawInfo.drawPlayer.bodyRotation = rotVelX;
            drawInfo.drawPlayer.legRotation = rotVelX * 0.75f;
            drawInfo.hidesBottomSkin = true;
            
            drawInfo.drawPlayer.legPosition = new Vector2(drawInfo.drawPlayer.legPosition.X - offset, -0.65f);      
        }
    }
}