using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace Reverie.Core.Animation
{
    /// <summary>
    /// Handles the drawing of held items with custom positioning and animations.
    /// </summary>
    //public class DrawHeldItems : ModPlayer
    //{
    //    private const float DEFAULT_ROTATION_STANDING = 0.32f;
    //    private const float DEFAULT_ROTATION_JUMPING = 0.15f;
    //    private const float DEFAULT_ROTATION_WALKING = 0.20f;

    //    private const float ARM_ROTATION_OFFSET = 0.7f;
    //    private const float WALK_ROTATION_ADJUSTMENT = 0.05f;
    //    private const float BODY_ROTATION_FACTOR = 0.25f;

    //    private const int HAND_OFFSET_X = 12;
    //    private const int HAND_OFFSET_Y = -8;

    //    public override void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions)
    //    {
    //        if (Main.gameMenu) return;

    //        var weaponLayer = ModContent.GetInstance<WeaponFrontDrawLayer>();
    //        if (positions.ContainsKey(weaponLayer))
    //        {
    //            positions[weaponLayer] = new PlayerDrawLayer.AfterParent(PlayerDrawLayers.ArmOverItem);
    //        }
    //    }

    //    protected float Oscillate(float amplitude, float frequency, float offset = 0f)
    //    {
    //        return amplitude * (float)Math.Cos(Main.GameUpdateCount * frequency + offset);
    //    }

    //    /// <summary>
    //    /// Draws the held item texture in the players left hand.
    //    /// </summary>
    //    public void DrawItemInHand(ref PlayerDrawSet drawInfo)
    //    {
    //        var player = drawInfo.drawPlayer;
    //        var heldItem = player.HeldItem;

    //        if (heldItem.IsAir || !heldItem.active) return;

    //        var itemTexture = TextureAssets.Item[heldItem.type];

    //        if (itemTexture == null) return;

    //        var direction = player.direction;
    //        var gravDir = player.gravDir;

    //        var sourceRect = Calculate_ItemSrcRect(heldItem, itemTexture.Value);

    //        int gWidth = sourceRect.HasValue ? sourceRect.Value.Width : itemTexture.Width();
    //        int gHeight = sourceRect.HasValue ? sourceRect.Value.Height : itemTexture.Height();

    //        var position = player.Center - Main.screenPosition;
    //        position += new Vector2(((heldItem.width / 3f) - 8) * direction, heldItem.height / 1.76f);

    //        var lighting = Lighting.GetColor((int)(player.Center.X / 16f), (int)(player.Center.Y / 16f));
    //        lighting = player.GetImmuneAlpha(heldItem.GetAlpha(lighting) * player.stealth, 0);

    //        var spriteEffects = direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
    //        if (gravDir < 0) spriteEffects |= SpriteEffects.FlipVertically;

    //        var data = new DrawData(
    //            itemTexture.Value,
    //            position,
    //            null,
    //            lighting,
    //            Oscillate(0.1f, 0.03f, 0f),
    //            new Vector2(gWidth / 2f, gHeight / 2f),
    //            heldItem.scale,
    //            spriteEffects,
    //            0
    //        );

    //        GetItemLocation(ref data, player);

    //        if (Math.Abs(player.velocity.X) > 0.1f)
    //        {
    //            PostWalkCycle(ref data, player);
    //        }

    //        drawInfo.DrawDataCache.Add(data);

    //        if (heldItem.glowMask > 0)
    //        {
    //            DrawGlowLayer(data, player, heldItem, ref drawInfo);
    //        }
    //    }

    //    /// <summary>
    //    /// Calculates the source rectangle for animated items
    //    /// </summary>
    //    private Rectangle? Calculate_ItemSrcRect(Item item, Texture2D texture)
    //    {
    //        if (item.ModItem == null || Main.itemAnimations[item.type] == null)
    //            return null;

    //        var animation = Main.itemAnimations[item.type];
    //        int frameCount = animation.FrameCount;
    //        int frameCounter = animation.TicksPerFrame * 2;
    //        int frameHeight = texture.Height / frameCount;

    //        int animationFrame = (int)(Main.GameUpdateCount / frameCounter) % frameCount;

    //        return new Rectangle(0, frameHeight * animationFrame, texture.Width, frameHeight);
    //    }

    //    private void GetItemLocation(ref DrawData data, Player player)
    //    {
    //        var direction = player.direction;
    //        var gravDir = player.gravDir;

    //        float rotation = player.compositeFrontArm.rotation;
    //        if (direction == -1)
    //            rotation = -rotation;

    //        data.rotation = rotation;

    //        Vector2 handOffset = new Vector2(8 * direction, -22f);
    //        handOffset = Vector2.Transform(handOffset, Matrix.CreateRotationY(rotation * direction));
    //        data.position += handOffset;

    //        ItemLocation(ref data, player);

    //        WalkCycleLocation(ref data, player);
    //    }

    //    /// <summary>
    //    /// Applies walk cycle adjustments based on player texture frame
    //    /// </summary>
    //    private void PostWalkCycle(ref DrawData data, Player player)
    //    {
    //        int frameNum = player.bodyFrame.Y / player.bodyFrame.Height;
    //        float direction = player.direction;
    //        float gravDir = player.gravDir;
    //    }

    //    /// <summary>
    //    /// Applies a walk cycle to the item position and rotation
    //    /// </summary>
    //    /// <param name="data"></param>
    //    /// <param name="player"></param>
    //    /// 
    //    public void WalkCycleLocation(ref DrawData data, Player player)
    //    {
    //        float rotation = player.compositeFrontArm.rotation / 5f;
    //        data.position.X += rotation * 5f * player.direction;
    //        data.position.Y += rotation;

    //        data.rotation += rotation * player.direction;
    //    }

    //    /// <summary>
    //    /// Draws the glow layer for items that have one
    //    /// </summary>
    //    public void DrawGlowLayer(DrawData data, Player player, Item heldItem, ref PlayerDrawSet drawInfo)
    //    {
    //        if (heldItem.glowMask <= 0) return;

    //        var glowTexture = TextureAssets.GlowMask[heldItem.glowMask].Value;
    //        if (glowTexture == null) return;

    //        var glowLighting = new Color(250, 250, 250, heldItem.alpha);
    //        glowLighting = player.GetImmuneAlpha(heldItem.GetAlpha(glowLighting) * player.stealth, 0);

    //        var glowData = new DrawData(
    //            glowTexture,
    //            data.position,
    //            data.sourceRect,
    //            glowLighting,
    //            data.rotation,
    //            data.origin,
    //            data.scale,
    //            data.effect,
    //            0
    //        );

    //        drawInfo.DrawDataCache.Add(glowData);
    //    }

    //    /// <summary>
    //    /// Default weapon positioning based on player state
    //    /// </summary>
    //    private void ItemLocation(ref DrawData data, Player player)
    //    {
    //        int frameNum = player.bodyFrame.Y / player.bodyFrame.Height;
    //        float direction = player.direction;
    //        float gravDir = player.gravDir;

    //        if (frameNum == 5)
    //        {
    //            data.rotation = (float)(Math.PI * DEFAULT_ROTATION_JUMPING * direction) * gravDir;
    //            data.position.X += 10 * direction;
    //            data.position.Y -= -4 * gravDir;
    //        }
    //        data.rotation = (float)(Math.PI * 0.28f * direction) * gravDir;
    //        data.position.X += 11 * direction;
    //        data.position.Y -= -9 * gravDir;
    //    }

    //    /// <summary>
    //    /// Applies walk cycle adjustments to the default weapon position
    //    /// </summary>
    //    private void WalkCycleDefaultPosition(ref DrawData data, Player player, int frameNum)
    //    {
    //        float direction = player.direction;
    //        float gravDir = player.gravDir;

    //        // Adjust vertical position during certain walk frames
    //        bool isUpFrame = (frameNum >= 7 && frameNum <= 9) || (frameNum >= 14 && frameNum <= 16);
    //        if (isUpFrame)
    //        {
    //            data.position.Y -= 2 * gravDir;
    //        }

    //        // Apply horizontal and rotational adjustments based on walk cycle
    //        if (frameNum >= 7 && frameNum <= 10)
    //        {
    //            data.position.X -= direction;
    //            data.rotation += WALK_ROTATION_ADJUSTMENT * direction * gravDir;
    //        }
    //        else if (frameNum >= 14 && frameNum <= 17)
    //        {
    //            data.position.X += direction;
    //            data.rotation -= WALK_ROTATION_ADJUSTMENT * direction * gravDir;
    //        }
    //    }
    //}

    //public class WeaponFrontDrawLayer : PlayerDrawLayer
    //{
    //    public AnimationPlayer animPlayer = ModContent.GetInstance<AnimationPlayer>();

    //    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

    //    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    //    {
    //        var player = drawInfo.drawPlayer;

    //        return !player.dead && player.itemAnimation <= 0
    //            && !player.HeldItem.IsAir && player.HeldItem.DealsDamage();
    //    }

    //    protected override void Draw(ref PlayerDrawSet drawInfo)
    //    {
    //        var player = drawInfo.drawPlayer;
    //        var animPlayer = player.GetModPlayer<DrawHeldItems>();
    //        animPlayer.DrawItemInHand(ref drawInfo);
    //    }
    //}
}