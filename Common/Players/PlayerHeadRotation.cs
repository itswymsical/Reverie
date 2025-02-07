﻿// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.
//https://github.com/Mirsario/TerrariaOverhaul/blob/dev/Common/EntityEffects/PlayerHeadRotation.cs

using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Reverie.Common.Players
{
    public class PlayerHeadRotation : ModPlayer
    {
        private static bool active;
        private float headRotation;
        private float targetHeadRotation;

        public override void Load()
        {
            // Hook into vanilla draw calls to ensure compatibility
            On_Main.DrawPlayers_AfterProjectiles += static (orig, main) =>
            {
                active = true;
                orig(main);
                active = false;
            };

            On_Main.DrawPlayers_BehindNPCs += static (orig, main) =>
            {
                active = true;
                orig(main);
                active = false;
            };
        }

        public override void PreUpdate()
        {
            const float LookStrength = 0.55f;

            if (Player.sleeping.isSleeping)
            {
                targetHeadRotation = 0f;
            }
            else
            {
                // Get mouse position in world coordinates
                Vector2 lookPosition = Main.MouseWorld;
                Vector2 offset = lookPosition - Player.Center;

                // Only rotate head if looking in the direction player is facing
                if (Math.Sign(offset.X) == Player.direction)
                {
                    targetHeadRotation = (offset * Player.direction).ToRotation() * LookStrength;
                }
                else
                {
                    targetHeadRotation = 0f;
                }
            }

            // Smoothly interpolate to target rotation
            float lerpSpeed = 0.15f; // Adjust this value to change how quickly head rotates
            headRotation = MathHelper.Lerp(headRotation, targetHeadRotation, lerpSpeed);
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (!Main.gameMenu && active)
            {
                drawInfo.drawPlayer.headRotation = headRotation;
            }
        }
    }
}