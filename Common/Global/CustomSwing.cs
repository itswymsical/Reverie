using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Reverie.Core;
using Terraria.ID;
using System.Collections.Generic;
using Terraria.Audio;

namespace Reverie.Common.Global
{
    public class CustomSwing : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public bool AppliesToEntity(Item item) => item.DamageType == DamageClass.Melee && !item.noUseGraphic;
        

        private bool isSwingingUp = true;
        private float swingProgress = 0f;

        private static readonly EaseFunction Ease = EaseFunction.EaseQuadInOut;
        private const float UP_SWING_START = -MathHelper.PiOver2;
        private const float UP_SWING_END = MathHelper.PiOver2;
        private const float DOWN_SWING_START = MathHelper.PiOver2;
        private const float DOWN_SWING_END = -MathHelper.PiOver2;

        public override Vector2? HoldoutOffset(int type) => new(-4, -8);     

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            if (AppliesToEntity(item))
            {
                if (player.itemAnimation == player.itemAnimationMax)
                {
                    swingProgress = 0f;
                    SoundStyle swingSound = isSwingingUp ? SoundID.Item1 : SoundID.Item7;
                    SoundEngine.PlaySound(swingSound);
                }

                if (player.itemAnimation > 0)
                {
                    swingProgress = 1f - (player.itemAnimation / (float)player.itemAnimationMax);

                    Vector2 swingPosition = GetSwingPosition(player);
                    float rotation = GetCurrentRotation();

                    //player.itemRotation = rotation * player.direction;
                    //item.position = player.position + swingPosition;

                    float armRotation = rotation / player.direction;

                    player.itemRotation = armRotation - MathHelper.ToRadians(48);
                    item.position = player.position + swingPosition;

                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation + 3.14f);
                    
                }
            }
        }

        private Vector2 GetSwingPosition(Player player)
        {
            float rotation = GetCurrentRotation();
            float distance = 20f;

            return new Vector2(
                (float)(Ease.Ease(rotation) * distance),
                (float)(Ease.Ease(rotation) * distance)
            );
        }

        private float GetCurrentRotation()
        {
            float easedProgress = Ease.Ease(swingProgress);

            if (isSwingingUp)
                return MathHelper.Lerp(UP_SWING_START, UP_SWING_END, easedProgress);
            else
                return MathHelper.Lerp(DOWN_SWING_START, DOWN_SWING_END, easedProgress);    
        } 
    }
}