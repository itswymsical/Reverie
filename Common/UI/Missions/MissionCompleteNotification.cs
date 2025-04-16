using Reverie.Core.Missions;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI;

namespace Reverie.Common.UI.Missions;

public class MissionCompleteNotification(Mission mission) : IInGameNotification
{
    private const float DISPLAY_TIME = 8f * 60f;
    private const float ANIMATION_TIME = 90f;
    private const float FADEOUT_TIME = 60f;

    public bool ShouldBeRemoved => timeLeft <= 0;
    private float timeLeft = DISPLAY_TIME;
    private readonly Mission completedMission = mission;

    private float AnimationProgress => Math.Clamp((DISPLAY_TIME - timeLeft) / ANIMATION_TIME, 0f, 1f);

    private float FadeoutProgress
    {
        get
        {
            if (timeLeft > FADEOUT_TIME) return 1f;
            return timeLeft / FADEOUT_TIME;
        }
    }

    private float TextScale
    {
        get
        {
            // Scale up during animation phase
            var introScale = MathHelper.SmoothStep(0.1f, 1f, AnimationProgress);
            // Scale down during fadeout
            return introScale * MathHelper.Lerp(0.5f, 1f, FadeoutProgress);
        }
    }

    private float TextOpacity
    {
        get
        {
            // Match opacity to the scaling animation (fade in + fade out)
            return AnimationProgress * FadeoutProgress;
        }
    }

    private float SunburstScale
    {
        get
        {
            var introScale = MathHelper.SmoothStep(0.1f, 0.6f, AnimationProgress);
            return introScale * MathHelper.Lerp(0.3f, 1f, FadeoutProgress);
        }
    }

    private float SunburstRotation => timeLeft * 0.01f;

    private Color TextColor
    {
        get
        {
            var baseColor = Color.Gold;
            var glowColor = Color.White;
            var currentColor = Color.Lerp(glowColor, baseColor, AnimationProgress);
            return currentColor * TextOpacity;
        }
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (timeLeft <= 0) return;

        var fadeoutOffset = MathHelper.Lerp(30f, 0f, FadeoutProgress);
        Vector2 screenCenter = new(Main.screenWidth / 2f, Main.screenHeight / 3f - fadeoutOffset);

        // Draw sunburst
        var sunburstTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Sunburst").Value;
        var rotation = SunburstRotation;
        Vector2 origin = new(sunburstTexture.Width / 2f);
        spriteBatch.Draw(
            sunburstTexture,
            screenCenter,
            null,
            TextColor * 0.2f,
            rotation,
            origin,
            SunburstScale,
            SpriteEffects.None,
            0f
        );

        // Draw title and measure for positioning other elements
        var title = $"{completedMission.Name}";
        var titleScale = TextScale * 0.75f;
        var titleSize = FontAssets.DeathText.Value.MeasureString(title) * titleScale;
        var titlePos = screenCenter - titleSize / 2f; // Center alignment

        // Draw accent texture
        var accentTexture = ModContent.Request<Texture2D>($"Reverie/Assets/Textures/UI/Missions/Accent").Value;
        Vector2 accentOrigin = new(accentTexture.Width / 2f, 0); // Origin at top-center
        Vector2 accentPosition = new(screenCenter.X, titlePos.Y - 20); // Under the title
        float accentScale = (titleSize.X / accentTexture.Width) * 1f; // Scale to match title width

        spriteBatch.Draw(
            accentTexture,
            accentPosition,
            null,
            TextColor * 0.8f,
            0f, // No rotation
            accentOrigin,
            1f, // Different X and Y scaling
            SpriteEffects.None,
            0f
        );

        // Draw title text
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.DeathText.Value,
            title,
            titlePos.X,
            titlePos.Y - 10,
            TextColor,
            Color.Black * TextOpacity,
            Vector2.Zero,
            titleScale
        );

        // Draw reward items
        DrawRewardItems(spriteBatch, screenCenter, titlePos.Y + titleSize.Y + 20);
    }

    private void DrawRewardItems(SpriteBatch spriteBatch, Vector2 screenCenter, float yPosition)
    {
        // Get rewards from mission
        var rewards = completedMission.Rewards;
        if (rewards == null || rewards.Count == 0) return;

        // Calculate total width needed for all items
        const float ITEM_SPACING = 10f;
        const float ITEM_SIZE = 16f;
        float totalWidth = (rewards.Count * ITEM_SIZE) + ((rewards.Count - 1) * ITEM_SPACING);

        // Start position (centered)
        Vector2 currentPos = new(screenCenter.X - totalWidth / 2f, yPosition);

        // Draw each item
        foreach (var reward in rewards)
        {
            // Get item texture
            Main.instance.LoadItem(reward.type);
            var itemTexture = TextureAssets.Item[reward.type].Value;
            Rectangle? sourceRect = null;

            // If using item animation frames, get the correct frame
            if (Main.itemAnimations[reward.type] != null)
            {
                Main.itemAnimations[reward.type].Update();
                sourceRect = Main.itemAnimations[reward.type].GetFrame(itemTexture);
            }

            // Calculate scale to fit in ITEM_SIZE
            float scale = ITEM_SIZE / Math.Max(itemTexture.Width, itemTexture.Height) * 1.25f;

            // Calculate origin (center of texture)
            Vector2 origin = sourceRect.HasValue
                ? new Vector2(sourceRect.Value.Width / 2f, sourceRect.Value.Height / 2f)
                : new Vector2(itemTexture.Width / 2f, itemTexture.Height / 2f);

            // Draw item with centered origin
            spriteBatch.Draw(
                itemTexture,
                currentPos + new Vector2(ITEM_SIZE / 2f),
                sourceRect,
                Color.White * TextOpacity,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );

            // Draw stack count if more than 1
            if (reward.stack > 1)
            {
                var stackText = reward.stack.ToString();
                var stackScale = 0.8f;
                var stackSize = FontAssets.ItemStack.Value.MeasureString(stackText) * stackScale;
                var stackPos = currentPos + new Vector2(ITEM_SIZE - stackSize.X / 2f, ITEM_SIZE - stackSize.Y / 2f);

                Utils.DrawBorderStringFourWay(
                    spriteBatch,
                    FontAssets.ItemStack.Value,
                    stackText,
                    stackPos.X,
                    stackPos.Y,
                    Color.White * TextOpacity,
                    Color.Black * TextOpacity,
                    Vector2.Zero,
                    stackScale
                );
            }

            // Move to next position
            currentPos.X += ITEM_SIZE + ITEM_SPACING;
        }
    }

    public void Update()
    {
        if (timeLeft == DISPLAY_TIME)
            SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}MissionComplete"));

        timeLeft--;
        timeLeft = Math.Max(0, timeLeft);
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom) { }
}