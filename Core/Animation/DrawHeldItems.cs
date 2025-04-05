using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace Reverie.Core.Animation
{
    /// <summary>
    /// Handles the drawing of held items with custom positioning and animations.
    /// </summary>
    public class DrawHeldItems : ModPlayer
    {
        // Constants for positioning adjustments
        private const float DEFAULT_ROTATION_STANDING = 0.32f;
        private const float DEFAULT_ROTATION_JUMPING = 0.15f;
        private const float DEFAULT_ROTATION_WALKING = 0.20f;

        private const float ARM_ROTATION_OFFSET = 0.7f;
        private const float WALK_ROTATION_ADJUSTMENT = 0.05f;
        private const float BODY_ROTATION_FACTOR = 0.25f;

        private const int HAND_OFFSET_X = 12;
        private const int HAND_OFFSET_Y = -8;

        public override void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions)
        {
            if (Main.gameMenu) return;

            var weaponLayer = ModContent.GetInstance<WeaponFrontDrawLayer>();
            if (positions.ContainsKey(weaponLayer))
            {
                positions[weaponLayer] = new PlayerDrawLayer.AfterParent(PlayerDrawLayers.ArmOverItem);
            }
        }
        protected float Oscillate(float amplitude, float frequency, float offset = 0f)
        {
            return amplitude * (float)Math.Cos(Main.GameUpdateCount * frequency + offset);
        }

        /// <summary>
        /// Main method to draw the weapon in front of the player
        /// </summary>
        public void DrawWeaponInFront(ref PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;
            var heldItem = player.HeldItem;

            // Early exit if no valid item
            if (heldItem.IsAir || !heldItem.active) return;

            // Get item texture
            var itemTexture = TextureAssets.Item[heldItem.type].Value;
            if (itemTexture == null) return;

            // Cache common values
            var direction = player.direction;
            var gravDir = player.gravDir;

            var sourceRect = CalculateItemSourceRectangle(heldItem, itemTexture);

            int gWidth = sourceRect.HasValue ? sourceRect.Value.Width : itemTexture.Width;
            int gHeight = sourceRect.HasValue ? sourceRect.Value.Height : itemTexture.Height;

            var position = player.Center - Main.screenPosition;
            position += new Vector2(((heldItem.width / 3f) - 8) * direction, heldItem.height / 1.76f);

            var lighting = Lighting.GetColor((int)(player.Center.X / 16f), (int)(player.Center.Y / 16f));
            lighting = player.GetImmuneAlpha(heldItem.GetAlpha(lighting) * player.stealth, 0);

            var spriteEffects = direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (gravDir < 0) spriteEffects |= SpriteEffects.FlipVertically;

            var data = new DrawData(
                itemTexture,
                position,
                null,
                lighting,
                Oscillate(0.1f, 0.03f, 0f),
                new Vector2(gWidth / 2f, gHeight / 2f),
                heldItem.scale,
                spriteEffects,
                0
            );

            PositionWeaponBasedOnAnimations(ref data, player);

            if (Math.Abs(player.velocity.X) > 0.1f)
            {
                ApplyWalkCycleAdjustments(ref data, player);
            }

            drawInfo.DrawDataCache.Add(data);

            if (heldItem.glowMask > 0)
            {
                DrawGlowLayer(data, player, heldItem, ref drawInfo);
            }
        }

        /// <summary>
        /// Calculates the source rectangle for animated items
        /// </summary>
        private Rectangle? CalculateItemSourceRectangle(Item item, Texture2D texture)
        {
            // If the item has no animation, return null
            if (item.ModItem == null || Main.itemAnimations[item.type] == null)
                return null;

            var animation = Main.itemAnimations[item.type];
            int frameCount = animation.FrameCount;
            int frameCounter = animation.TicksPerFrame * 2;
            int frameHeight = texture.Height / frameCount;

            // Calculate current animation frame
            int animationFrame = (int)(Main.GameUpdateCount / frameCounter) % frameCount;

            return new Rectangle(0, frameHeight * animationFrame, texture.Width, frameHeight);
        }

        /// <summary>
        /// Positions the weapon based on current animation states
        /// </summary>
        private void PositionWeaponBasedOnAnimations(ref DrawData data, Player player)
        {
            // Get cached values
            var direction = player.direction;
            var gravDir = player.gravDir;

            // Get arm rotation and adjust for direction
            float rotation = player.compositeFrontArm.rotation;
            if (direction == -1)
                rotation = -rotation;

            data.rotation = rotation;

            // Calculate hand position offset
            Vector2 handOffset = new Vector2(8 * direction, -14.5f);
            handOffset = Vector2.Transform(handOffset, Matrix.CreateRotationZ(rotation * direction));
            data.position += handOffset;

            // Apply default positioning
            DefaultWeaponPositioning(ref data, player);

            // Apply additional animation-based adjustments
            ApplyWalkCycleFromAnimation(ref data, player);
        }

        /// <summary>
        /// Applies walk cycle adjustments based on player animation frame
        /// </summary>
        private void ApplyWalkCycleAdjustments(ref DrawData data, Player player)
        {
            int frameNum = player.bodyFrame.Y / player.bodyFrame.Height;
            float direction = player.direction;
            float gravDir = player.gravDir;

            // Adjust vertical position during certain walk frames
            bool isUpFrame = (frameNum >= 7 && frameNum <= 9) || (frameNum >= 14 && frameNum <= 16);
            if (isUpFrame)
            {
                data.position.Y -= 2 * gravDir;
            }

            // Apply horizontal and rotational adjustments based on walk cycle
            if (frameNum >= 7 && frameNum <= 10)
            {
                data.position.X -= direction;
                data.rotation += 0.003f * direction * gravDir;
            }
            else if (frameNum >= 14 && frameNum <= 17)
            {
                data.position.X += direction;
                data.rotation -= 0.003f * direction * gravDir;
            }
        }

        /// <summary>
        /// Applies animation-based adjustments to weapon position
        /// </summary>
        public void ApplyWalkCycleFromAnimation(ref DrawData data, Player player)
        {
            // Adjust position based on body movements
            data.position.X += player.bodyRotation * 5f * player.direction;
            data.position.Y += player.bodyPosition.Y;

            // Adjust rotation to match body movement
            data.rotation += player.bodyRotation * BODY_ROTATION_FACTOR * player.direction;

            // Adjust position based on leg movement
            data.position.Y += player.legPosition.Y * 0.5f;
        }

        /// <summary>
        /// Draws the glow layer for items that have one
        /// </summary>
        public void DrawGlowLayer(DrawData data, Player player, Item heldItem, ref PlayerDrawSet drawInfo)
        {
            // Early exit if no valid glow mask
            if (heldItem.glowMask <= 0) return;

            // Get glow mask texture
            var glowTexture = TextureAssets.GlowMask[heldItem.glowMask].Value;
            if (glowTexture == null) return;

            // Calculate glow color
            var glowLighting = new Color(250, 250, 250, heldItem.alpha);
            glowLighting = player.GetImmuneAlpha(heldItem.GetAlpha(glowLighting) * player.stealth, 0);

            // Create and add glow draw data
            var glowData = new DrawData(
                glowTexture,
                data.position,
                data.sourceRect,
                glowLighting,
                data.rotation,
                data.origin,
                data.scale,
                data.effect,
                0
            );

            drawInfo.DrawDataCache.Add(glowData);
        }

        /// <summary>
        /// Default weapon positioning based on player state
        /// </summary>
        private void DefaultWeaponPositioning(ref DrawData data, Player player)
        {
            int frameNum = player.bodyFrame.Y / player.bodyFrame.Height;
            float direction = player.direction;
            float gravDir = player.gravDir;

            // Different positioning based on player animation state
            if (frameNum < 5) // Standing
            {
                // Hold weapon at angle in front
                data.rotation = (float)(Math.PI * DEFAULT_ROTATION_STANDING * direction) * gravDir;
                data.position.X += 10 * direction;
                data.position.Y -= -14 * gravDir;
            }
            else if (frameNum == 5) // Jumping
            {
                // Hold weapon more horizontally while jumping
                data.rotation = (float)(Math.PI * DEFAULT_ROTATION_JUMPING * direction) * gravDir;
                data.position.X += 10 * direction;
                data.position.Y -= -4 * gravDir;
            }
            else // Walking
            {
                // Slight back and forth motion while walking
                data.rotation = (float)(Math.PI * DEFAULT_ROTATION_WALKING * direction) * gravDir;
                data.position.X += 11 * direction;
                data.position.Y -= -6 * gravDir;

                // Apply walk cycle adjustments
                ApplyWalkCycleForDefaultPosition(ref data, player, frameNum);
            }
        }

        /// <summary>
        /// Applies walk cycle adjustments to the default weapon position
        /// </summary>
        private void ApplyWalkCycleForDefaultPosition(ref DrawData data, Player player, int frameNum)
        {
            float direction = player.direction;
            float gravDir = player.gravDir;

            // Adjust vertical position during certain walk frames
            bool isUpFrame = (frameNum >= 7 && frameNum <= 9) || (frameNum >= 14 && frameNum <= 16);
            if (isUpFrame)
            {
                data.position.Y -= 2 * gravDir;
            }

            // Apply horizontal and rotational adjustments based on walk cycle
            if (frameNum >= 7 && frameNum <= 10)
            {
                data.position.X -= direction;
                data.rotation += WALK_ROTATION_ADJUSTMENT * direction * gravDir;
            }
            else if (frameNum >= 14 && frameNum <= 17)
            {
                data.position.X += direction;
                data.rotation -= WALK_ROTATION_ADJUSTMENT * direction * gravDir;
            }
        }
    }

    public class WeaponFrontDrawLayer : PlayerDrawLayer
    {
        public AnimationPlayer animPlayer = ModContent.GetInstance<AnimationPlayer>();

        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;

            return !player.dead && player.itemAnimation <= 0
                && !player.HeldItem.IsAir && player.HeldItem.DealsDamage();
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;
            var animPlayer = player.GetModPlayer<DrawHeldItems>();
            //animPlayer.DrawWeaponInFront(ref drawInfo);
        }
    }
}