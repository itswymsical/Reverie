using Reverie.Core.Indicators;
using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using Reverie.Utilities;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;

namespace Reverie.Core.Indicators;

public class MissionIndicator : ScreenIndicator
{
    private readonly Mission mission;
    private readonly Texture2D iconTexture;

    private const int PANEL_WIDTH = 220;
    private const int PADDING = 10;

    public static bool ShowDebugHitbox = false;

    public override AnimationType AnimationStyle => AnimationType.Wag;

    public MissionIndicator(Vector2 worldPosition, Mission mission, AnimationType? animationType = null)
         : base(worldPosition, 48, 48, animationType)
    {
        this.mission = mission;

        iconTexture = ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Missions/Indicator", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

        if (iconTexture != null)
        {
            Width = iconTexture.Width;
            Height = iconTexture.Height;
        }

        OnDrawWorld = DrawIndicator;
        OnClick += HandleClick;
    }

    public static MissionIndicator CreateForNPC(NPC npc, Mission mission, AnimationType? animationType = null)
    {
        var indicator = new MissionIndicator(npc.Top, mission, animationType);
        indicator.TrackEntity(npc, new Vector2(0, -40));
        return indicator;
    }

    protected override void PostUpdate()
    {
        // Check if mission should still show indicator
        if (mission != null && ShouldHideIndicator())
        {
            IsVisible = false;
        }
    }

    /// <summary>
    /// Determines if the indicator should be hidden based on mission state.
    /// For mainline missions, hides if ANY player has started it.
    /// For sideline missions, hides only if the local player has started it.
    /// </summary>
    private bool ShouldHideIndicator()
    {
        if (mission.IsMainline)
        {
            var worldMission = WorldMissionSystem.Instance.GetMainlineMission(mission.ID);
            return worldMission?.Progress != MissionProgress.Inactive || worldMission?.Status != MissionStatus.Unlocked;
        }
        else
        {
            var localMissionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            var localMission = localMissionPlayer.GetMission(mission.ID);
            return localMission?.Progress != MissionProgress.Inactive || localMission?.Status != MissionStatus.Unlocked;
        }
    }

    private void DrawIndicator(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        if (ShowDebugHitbox)
        {
            DrawDebugHitbox(spriteBatch, screenPos, opacity);
        }

        var scale = GetAnimationScale();
        var glowColor = IsHovering ? Color.White : Color.White * 0.8f;
        var rotation = GetAnimationRotation();
        spriteBatch.Draw(
            iconTexture,
            screenPos,
            null,
            glowColor * opacity,
            rotation,
            new Vector2(iconTexture.Width / 2, iconTexture.Height / 2),
            scale,
            SpriteEffects.None,
            0f
        );

        if (IsHovering)
        {
            DrawPanel(spriteBatch, opacity * GetHoverOpacity());
        }
    }

    private void DrawDebugHitbox(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        var zoom = Main.GameViewMatrix.Zoom.X;
        var scaledWidth = (int)(Width * zoom);
        var scaledHeight = (int)(Height * zoom);

        var hitboxRect = new Rectangle(
            (int)screenPos.X - scaledWidth / 2,
            (int)screenPos.Y - scaledHeight / 2,
            scaledWidth,
            scaledHeight
        );

        var pixel = TextureAssets.MagicPixel.Value;
        var borderColor = IsHovering ? Color.Lime : Color.Red;
        borderColor *= opacity * 0.8f;

        // Draw border lines
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Y, hitboxRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Bottom - 2, hitboxRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Y, 2, hitboxRect.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.Right - 2, hitboxRect.Y, 2, hitboxRect.Height), borderColor);

        var fillColor = IsHovering ? Color.Lime : Color.Red;
        fillColor *= opacity * 0.2f;
        spriteBatch.Draw(pixel, hitboxRect, fillColor);
    }

    private void DrawPanel(SpriteBatch spriteBatch, float opacity)
    {
        if (mission == null)
            return;

        // Get proper screen position for UI panel positioning
        var screenPos = GetScreenPosition();

        // Calculate reward layout
        var rewardLayout = CalculateRewardLayout(mission);

        var lineCount = 3; // Mission type, name, employer, description
        if (mission.Objective.Count > 0)
        {
            lineCount += mission.Objective[0].Objectives.Count;
        }

        // Add space for rewards if they exist
        var rewardSectionHeight = 0;
        if (rewardLayout.HasRewards)
        {
            rewardSectionHeight = 20 + // "Rewards:" text
                                 (rewardLayout.SlotSize + 5) * rewardLayout.Rows + // Reward rows
                                 10; // Bottom padding
        }

        var panelHeight = 20 + lineCount * 20 + PADDING * 2 + rewardSectionHeight + 40; // Extra space for click text

        // Position panel to the right of the icon
        var panelX = screenPos.X + Width / 2 + 10;
        var panelY = screenPos.Y - panelHeight / 2;

        // Adjust if panel would go off screen
        if (panelX + PANEL_WIDTH > Main.screenWidth)
        {
            panelX = screenPos.X - Width / 2 - PANEL_WIDTH - 10;
        }

        if (panelY + panelHeight > Main.screenHeight)
        {
            panelY = Main.screenHeight - panelHeight - 10;
        }

        if (panelY < 10)
        {
            panelY = 10;
        }

        var panelRect = new Rectangle(
            (int)panelX,
            (int)panelY,
            PANEL_WIDTH,
            panelHeight
        );

        var textY = panelRect.Y + PADDING;

        // Mission type indicator
        var missionTypeColor = mission.IsMainline ? new Color(255, 215, 0) : new Color(173, 216, 230);
        var missionTypeText = mission.IsMainline ? "[Mainline]" : "[Sideline]";

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            missionTypeText,
            panelRect.X + PADDING,
            textY,
            missionTypeColor * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            0.7f
        );
        textY += 20;

        // Mission name
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            mission.Name,
            panelRect.X + PADDING,
            textY,
            new Color(192, 151, 83, (int)(255 * opacity)),
            Color.Black * opacity,
            Vector2.Zero,
            1f
        );
        textY += 25;

        // Employer name
        var employerName = "Unknown";
        if (mission.ProviderNPC > 0)
        {
            employerName = Lang.GetNPCNameValue(mission.ProviderNPC);
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            $"From: {employerName}",
            panelRect.X + PADDING,
            textY,
            Color.White * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            0.9f
        );
        textY += 24;

        // Mission description
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            mission.Description,
            panelRect.X + PADDING,
            textY,
            Color.White * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            0.8f
        );
        textY += 28;

        // Draw rewards with dynamic layout
        if (rewardLayout.HasRewards)
        {
            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                "Rewards:",
                panelRect.X + PADDING,
                textY,
                Color.White * opacity,
                Color.Black * opacity,
                Vector2.Zero,
                0.8f
            );
            textY += 20;

            DrawDynamicRewards(spriteBatch, mission, rewardLayout, panelRect.X + PADDING, textY, opacity);
        }

        var clickText = mission.IsMainline ? "Click to accept" : "Click to accept";
        var clickColor = mission.IsMainline ? new Color(255, 215, 0) : Color.Yellow;

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            clickText,
            panelRect.X + 4,
            panelRect.Y + panelHeight - 30,
            clickColor * opacity,
            Color.Black * opacity,
            default,
            0.8f
        );
    }

    private struct RewardLayout
    {
        public bool HasRewards;
        public int TotalRewards;
        public int Columns;
        public int Rows;
        public int SlotSize;
        public float ItemScale;
        public bool HasExperience;
    }

    private RewardLayout CalculateRewardLayout(Mission mission)
    {
        var layout = new RewardLayout();

        // Count total rewards (items + experience if present)
        layout.TotalRewards = mission.Rewards.Count(r => r.type > ItemID.None);
        layout.HasExperience = mission.Experience > 0;

        if (layout.HasExperience)
            layout.TotalRewards++; // Count XP as a reward slot

        layout.HasRewards = layout.TotalRewards > 0;

        if (!layout.HasRewards)
            return layout;

        // Determine layout based on reward count
        if (layout.TotalRewards <= 3)
        {
            // Few rewards: Large slots, single row
            layout.Columns = layout.TotalRewards;
            layout.Rows = 1;
            layout.SlotSize = 32;
            layout.ItemScale = 1.0f;
        }
        else if (layout.TotalRewards <= 6)
        {
            // Medium rewards: Medium slots, up to 2 rows
            layout.Columns = Math.Min(3, layout.TotalRewards);
            layout.Rows = (int)Math.Ceiling(layout.TotalRewards / 3.0);
            layout.SlotSize = 28;
            layout.ItemScale = 0.85f;
        }
        else if (layout.TotalRewards <= 12)
        {
            // Many rewards: Small slots, multiple rows
            layout.Columns = Math.Min(4, layout.TotalRewards);
            layout.Rows = (int)Math.Ceiling(layout.TotalRewards / 4.0);
            layout.SlotSize = 24;
            layout.ItemScale = 0.7f;
        }
        else
        {
            // Too many rewards: Very small slots, grid layout
            layout.Columns = 5;
            layout.Rows = (int)Math.Ceiling(layout.TotalRewards / 5.0);
            layout.SlotSize = 20;
            layout.ItemScale = 0.6f;
        }

        return layout;
    }

    private void DrawDynamicRewards(SpriteBatch spriteBatch, Mission mission, RewardLayout layout, int startX, int startY, float opacity)
    {
        int currentSlot = 0;
        int slotSpacing = layout.SlotSize + 2;

        // Draw item rewards
        foreach (var reward in mission.Rewards)
        {
            if (reward.type <= ItemID.None)
                continue;

            int row = currentSlot / layout.Columns;
            int col = currentSlot % layout.Columns;

            var slotX = startX + col * slotSpacing;
            var slotY = startY + row * slotSpacing;

            // Draw inventory slot background
            var slotRect = new Rectangle(slotX, slotY, layout.SlotSize, layout.SlotSize);
            spriteBatch.Draw(
                TextureAssets.InventoryBack.Value,
                slotRect,
                null,
                Color.White * opacity * 0.8f,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0f
            );

            // Center the item in the slot
            var itemTexture = TextureAssets.Item[reward.type].Value;
            var itemSize = new Vector2(itemTexture.Width, itemTexture.Height) * layout.ItemScale;
            var itemPos = new Vector2(
                slotX + (layout.SlotSize - itemSize.X) / 2,
                slotY + (layout.SlotSize - itemSize.Y) / 2
            );

            spriteBatch.Draw(
                itemTexture,
                itemPos,
                null,
                Color.White * opacity,
                0f,
                Vector2.Zero,
                layout.ItemScale,
                SpriteEffects.None,
                0f
            );

            // Draw stack count if > 1
            if (reward.stack > 1)
            {
                var stackScale = Math.Max(0.5f, layout.ItemScale * 0.8f);
                Utils.DrawBorderStringFourWay(
                    spriteBatch,
                    FontAssets.ItemStack.Value,
                    reward.stack.ToString(),
                    slotX + layout.SlotSize - 12,
                    slotY + layout.SlotSize - 12,
                    Color.White * opacity,
                    Color.Black * opacity,
                    Vector2.Zero,
                    stackScale
                );
            }

            currentSlot++;
        }

        // Draw experience reward if present
        if (layout.HasExperience)
        {
            int row = currentSlot / layout.Columns;
            int col = currentSlot % layout.Columns;

            var slotX = startX + col * slotSpacing;
            var slotY = startY + row * slotSpacing;

            // Draw XP slot background with different color
            var slotRect = new Rectangle(slotX, slotY, layout.SlotSize, layout.SlotSize);
            spriteBatch.Draw(
                TextureAssets.InventoryBack.Value,
                slotRect,
                null,
                new Color(73, 213, 255) * opacity * 0.6f, // Light blue tint for XP
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0f
            );

            // Draw XP text in the slot
            var xpText = mission.Experience.ToString();
            var xpTextSize = FontAssets.MouseText.Value.MeasureString(xpText) * layout.ItemScale;
            var xpPos = new Vector2(
                slotX + (layout.SlotSize - xpTextSize.X) / 2,
                slotY + (layout.SlotSize - xpTextSize.Y) / 2
            );

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                xpText,
                xpPos.X,
                xpPos.Y + 2,
                new Color(73, 213, 255) * opacity,
                Color.Black * opacity,
                Vector2.Zero,
                layout.ItemScale
            );

            // Draw "XP" label below the number
            var xpLabelScale = layout.ItemScale * 0.75f;
            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                "XP",
                xpPos.X + 16,
                xpPos.Y + xpTextSize.Y - 6,
                new Color(73, 213, 255) * opacity * 0.8f,
                Color.Black * opacity,
                Vector2.Zero,
                xpLabelScale
            );
        }
    }

    private void HandleClick()
    {
        try
        {
            if (mission.IsMainline)
            {
                WorldMissionSystem.Instance.StartMission(mission.ID);
            }
            else
            {
                var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
                missionPlayer.StartMission(mission.ID);
            }

            IsVisible = false;

            var missionType = mission.IsMainline ? "mainline" : "sideline";
            ModContent.GetInstance<Reverie>().Logger.Info($"Player {Main.LocalPlayer.name} started {missionType} mission: {mission.Name}");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error starting mission {mission.Name}: {ex.Message}");
        }
    }
}