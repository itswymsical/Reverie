using Reverie.Core.Animation;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace Reverie.Common.Players.Animation;

public partial class AnimationPlayer
{
    public override void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions)
    {
        base.ModifyDrawLayerOrdering(positions);

        if (Main.gameMenu) return;

        if (positions.ContainsKey(ModContent.GetInstance<WeaponFrontDrawLayer>()))
        {
            positions[ModContent.GetInstance<WeaponFrontDrawLayer>()] =
                new PlayerDrawLayer.AfterParent(PlayerDrawLayers.ArmOverItem);
        }
    }

    public void DrawWeaponInFront(ref PlayerDrawSet drawInfo)
    {
        var player = drawInfo.drawPlayer;
        var heldItem = player.HeldItem;

        var itemTexture = TextureAssets.Item[heldItem.type].Value;
        if (itemTexture == null) return;

        var gWidth = itemTexture.Width;
        var gHeight = itemTexture.Height;

        // Check for animations
        Rectangle? sourceRect = null;
        if (heldItem.ModItem != null && Main.itemAnimations[heldItem.type] != null)
        {
            var frameCount = Main.itemAnimations[heldItem.type].FrameCount;
            var frameCounter = Main.itemAnimations[heldItem.type].TicksPerFrame * 2;

            // Calculate animation frame directly
            var animationFrame = (int)(Main.GameUpdateCount / frameCounter) % frameCount;

            // Set source rectangle
            gHeight /= frameCount;
            sourceRect = new Rectangle(0, gHeight * animationFrame, gWidth, gHeight);
        }

        var position = player.MountedCenter - Main.screenPosition;
        position.Y += 6;

        var lighting = Lighting.GetColor(
            (int)(player.Center.X / 16f),
            (int)(player.Center.Y / 16f)
        );

        lighting = player.GetImmuneAlpha(heldItem.GetAlpha(lighting) * player.stealth, 0);

        var spriteEffects = SpriteEffects.None;
        if (player.direction < 0) spriteEffects = SpriteEffects.FlipHorizontally;
        if (player.gravDir < 0)
        {
            
            spriteEffects |= SpriteEffects.FlipVertically;
        }

        var data = new DrawData(
            itemTexture,
            position,
            sourceRect,
            lighting,
            0f,
            new Vector2(gWidth / 2f, gHeight / 2f),
            heldItem.scale,
            spriteEffects,
            0
        );

        var itemWidth = gWidth * heldItem.scale;
        var itemHeight = gHeight * heldItem.scale;
        var larger = Math.Max(itemWidth, itemHeight);
        var lesser = Math.Min(itemWidth, itemHeight);

        // Always use animation-based positioning if available
        if (currentAnimation != null)
        {
            var currentState = currentAnimation.GetStateAtTime(animationTime);

            if (currentState.TryGetValue(SegmentType.LeftArm, out var leftArm))
            {
                var rotation = leftArm.Rotation;

                if (player.direction == -1)
                    rotation = -rotation;

                rotation += 0.3f * player.direction;

                data.rotation = rotation;

                // Position weapon relative to hand position
                Vector2 handOffset = new Vector2(12 * player.direction, -8);

                // Adjust offset based on arm rotation to maintain consistent positioning
                handOffset = Vector2.Transform(
                    handOffset,
                    Matrix.CreateRotationZ(leftArm.Rotation * player.direction)
                );

                data.position += handOffset;
            }

            // If left arm isn't animated, we can use this as fallback
            else if (currentState.TryGetValue(SegmentType.RightArm, out var rightArm))
            {
                var rotation = rightArm.Rotation;
                if (player.direction == -1)
                    rotation = -rotation;

                rotation += 0.3f * player.direction;
                data.rotation = rotation;

                Vector2 handOffset = new Vector2(12 * player.direction, -8);
                handOffset = Vector2.Transform(
                    handOffset,
                    Matrix.CreateRotationZ(rightArm.Rotation * player.direction)
                );

                data.position += handOffset;
            }

            // If neither arm is animated
            else if (currentState.TryGetValue(SegmentType.Body, out var bodyState))
            {
                var rotation = bodyState.Rotation;
                if (player.direction == -1)
                    rotation = -rotation;

                rotation += 0.32f * player.direction;
                data.rotation = rotation;

                data.position.X += 10 * player.direction;
                data.position.Y -= -14 * player.gravDir;

                if (player.velocity.X != 0)
                {
                    ApplyWalkCycleFromAnimation(ref data, player, currentState);
                }
            }
            else
            {
                // Default positioning if no relevant joints are animated
                //ApplyFrontWeaponPositioning(ref data, player, larger, lesser);
            }
        }
        else
        {
            // No animation, use default positioning
            //ApplyFrontWeaponPositioning(ref data, player, larger, lesser);
        }

        drawInfo.DrawDataCache.Add(data);

        if (heldItem.glowMask != -1)
        {
            DrawGlowLayer(data, player, heldItem, ref drawInfo);
        }
    }

    public void ApplyWalkCycleFromAnimation(ref DrawData data, Player player, Dictionary<SegmentType, SegmentState> jointStates)
    {
        // Apply walk bobbing based on body joint if available
        if (jointStates.TryGetValue(SegmentType.Body, out var bodyState))
        {
            data.position.X += bodyState.Rotation * 5f * player.direction;

            data.position.Y += bodyState.Position.Y;

            // Slightly adjust rotation to match body movement
            data.rotation += bodyState.Rotation * 0.25f * player.direction;
        }

        if (jointStates.TryGetValue(SegmentType.Legs, out var legState))
        {
            // Add slight vertical adjustment based on leg position
            data.position.Y += legState.Position.Y * 0.5f;
        }
    }
    public void DrawGlowLayer(DrawData data, Player player, Item heldItem, ref PlayerDrawSet drawInfo)
    {
        if (heldItem.glowMask <= 0) return;

        var glowLighting = new Color(250, 250, 250, heldItem.alpha);
        glowLighting = player.GetImmuneAlpha(heldItem.GetAlpha(glowLighting) * player.stealth, 0);

        var glowData = new DrawData(
            TextureAssets.GlowMask[heldItem.glowMask].Value,
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

    // Keep the original methods as fallbacks
    //
    //public void ApplyFrontWeaponPositioning(ref DrawData data, Player player, float length, float width)
    //{
    //    var playerBodyFrameNum = player.bodyFrame.Y / player.bodyFrame.Height;

    //    // Positioning based on player animation frame
    //    if (playerBodyFrameNum < 5) // Standing
    //    {
    //        // Hold weapon at angle in front
    //        data.rotation = (float)(Math.PI * 0.32f * player.direction) * player.gravDir;
    //        data.position.X += 10 * player.direction; // In front
    //        data.position.Y -= -14 * player.gravDir; // Slightly up
    //    }
    //    else if (playerBodyFrameNum == 5) // Jumping
    //    {
    //        // Hold weapon more horizontally while jumping
    //        data.rotation = (float)(Math.PI * 0.15f * player.direction) * player.gravDir;
    //        data.position.X += 10 * player.direction;
    //        data.position.Y -= -4 * player.gravDir;
    //    }
    //    else // Walking
    //    {
    //        // Slight back and forth motion while walking
    //        data.rotation = (float)(Math.PI * 0.20f * player.direction) * player.gravDir;
    //        data.position.X += 11 * player.direction;
    //        data.position.Y -= -6 * player.gravDir;

    //        // Apply walk cycle adjustments
    //        ApplyWalkCyclePositioning(ref data, player);
    //    }
    //}

    //public void ApplyWalkCyclePositioning(ref DrawData data, Player player)
    //{
    //    // Original implementation remaining the same
    //    var playerBodyFrameNum = player.bodyFrame.Y / player.bodyFrame.Height;

    //    // Adjust vertical position during certain walk frames
    //    if (playerBodyFrameNum >= 7 && playerBodyFrameNum <= 9 ||
    //        playerBodyFrameNum >= 14 && playerBodyFrameNum <= 16)
    //    {
    //        data.position.Y -= 2 * player.gravDir;
    //    }

    //    // Add horizontal bobbing during walk
    //    switch (playerBodyFrameNum)
    //    {
    //        case 7:
    //        case 8:
    //        case 9:
    //        case 10:
    //            data.position.X -= player.direction * 1;
    //            data.rotation += 0.05f * player.direction * player.gravDir;
    //            break;
    //        case 14:
    //        case 15:
    //        case 16:
    //        case 17:
    //            data.position.X += player.direction * 1;
    //            data.rotation -= 0.05f * player.direction * player.gravDir;
    //            break;
    //    }
    //}


}
