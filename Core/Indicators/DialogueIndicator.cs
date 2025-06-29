using Reverie.Core.Dialogue;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Core.Indicators;

/// <summary>
/// An indicator for dialogue, appears in the world and shows dialogue preview
/// </summary>
public class DialogueIndicator : ScreenIndicator
{
    private readonly NPCData npcData;
    private readonly string dialogueKey;
    private readonly int lineCount;
    private readonly bool zoomIn;
    private readonly int defaultDelay;
    private readonly int defaultEmote;
    private readonly int? musicId;
    private readonly (int line, int delay, int emote)[] modifications;

    private readonly Texture2D iconTexture;

    private const int PANEL_WIDTH = 250;
    private const int PADDING = 10;

    public static bool ShowDebugHitbox = false;

    public override AnimationType AnimationStyle => AnimationType.Wag;

    public DialogueIndicator(Vector2 worldPosition, NPCData npcData, string dialogueKey, int lineCount,
        bool zoomIn = false, int defaultDelay = 2, int defaultEmote = 0, int? musicId = null,
        AnimationType? animationType = null, params (int line, int delay, int emote)[] modifications)
         : base(worldPosition, 40, 40, animationType)
    {
        this.npcData = npcData;
        this.dialogueKey = dialogueKey;
        this.lineCount = lineCount;
        this.zoomIn = zoomIn;
        this.defaultDelay = defaultDelay;
        this.defaultEmote = defaultEmote;
        this.musicId = musicId;
        this.modifications = modifications;

        iconTexture = TextureAssets.Chat2.Value;
        if (iconTexture != null)
        {
            Width = iconTexture.Width;
            Height = iconTexture.Height;
        }

        OnDrawWorld = DrawIndicator;
        OnClick += HandleClick;
    }

    public static DialogueIndicator CreateForNPC(NPC npc, NPCData npcData, string dialogueKey, int lineCount,
        bool zoomIn = false, int defaultDelay = 2, int defaultEmote = 0, int? musicId = null,
        AnimationType? animationType = null, params (int line, int delay, int emote)[] modifications)
    {
        var indicator = new DialogueIndicator(npc.Top, npcData, dialogueKey, lineCount, zoomIn, defaultDelay, defaultEmote, musicId, animationType, modifications);
        indicator.TrackEntity(npc, new Vector2(0, -50));
        return indicator;
    }

    protected override void CustomUpdate()
    {
        // Custom dialogue-specific update logic can go here
        // Animation logic is now handled by the base class
    }

    private void DrawIndicator(SpriteBatch spriteBatch, Vector2 worldPos, float opacity)
    {
        if (iconTexture == null)
            return;

        var scale = GetAnimationScale();
        var glowColor = IsHovering ? Color.LightBlue : Color.White * 0.9f;
        var rotation = GetAnimationRotation();

        spriteBatch.Draw(
            iconTexture,
            worldPos,
            null,
            glowColor * opacity,
            rotation,
            new Vector2(iconTexture.Width / 2, iconTexture.Height / 2),
            scale,
            SpriteEffects.None,
            0f
        );

        if (ShowDebugHitbox)
        {
            DrawDebugHitbox(spriteBatch, worldPos, opacity);
        }

        if (IsHovering)
        {
            DrawPanel(spriteBatch, worldPos, opacity * GetHoverOpacity());
        }
    }

    private void DrawDebugHitbox(SpriteBatch spriteBatch, Vector2 worldPos, float opacity)
    {
        var scaledWidth = Width;
        var scaledHeight = Height;

        var hitboxRect = new Rectangle(
            (int)worldPos.X - scaledWidth / 2,
            (int)worldPos.Y - scaledHeight / 2,
            scaledWidth,
            scaledHeight
        );

        var pixel = TextureAssets.MagicPixel.Value;
        var borderColor = IsHovering ? Color.Cyan : Color.Blue;
        borderColor *= opacity * 0.8f;

        // Draw border
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Y, hitboxRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Bottom - 2, hitboxRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Y, 2, hitboxRect.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.Right - 2, hitboxRect.Y, 2, hitboxRect.Height), borderColor);

        var fillColor = IsHovering ? Color.Cyan : Color.Blue;
        fillColor *= opacity * 0.2f;
        spriteBatch.Draw(pixel, hitboxRect, fillColor);
    }

    private void DrawPanel(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        if (npcData == null)
            return;

        float zoom = Main.GameViewMatrix.Zoom.X;

        // Calculate panel height based on content
        int lineCount = 2; // NPC name + preview text
        if (musicId.HasValue)
            lineCount++;

        int panelHeight = 20 + (lineCount * 22) + PADDING * 2;

        // Position panel to the right of the icon
        float panelX = screenPos.X + (Width * zoom / 2) + 15;
        float panelY = screenPos.Y - (panelHeight / 2);

        // Keep panel on screen
        if (panelX + PANEL_WIDTH > Main.screenWidth)
        {
            panelX = screenPos.X - (Width * zoom / 2) - PANEL_WIDTH - 15;
        }

        if (panelY + panelHeight > Main.screenHeight)
        {
            panelY = Main.screenHeight - panelHeight - 10;
        }

        if (panelY < 10)
        {
            panelY = 10;
        }

        Rectangle panelRect = new Rectangle(
            (int)panelX,
            (int)panelY,
            PANEL_WIDTH,
            panelHeight
        );

        // Draw semi-transparent background
        var pixel = TextureAssets.MagicPixel.Value;
        spriteBatch.Draw(pixel, panelRect, npcData.BoxColor * 0.3f * opacity);

        // Draw border
        var borderColor = npcData.BoxColor * opacity;
        spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Bottom - 2, panelRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, 2, panelRect.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(panelRect.Right - 2, panelRect.Y, 2, panelRect.Height), borderColor);

        int textY = panelRect.Y + PADDING;

        // NPC name
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            npcData.NpcName,
            panelRect.X + PADDING,
            textY,
            npcData.BoxColor * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            1f
        );
        textY += 25;

        // Try to get first dialogue line as preview
        string previewText = "Talk to start dialogue";
        try
        {
            var dialogue = DialogueBuilder.BuildByKey(dialogueKey, lineCount, defaultDelay, defaultEmote, musicId, modifications);
            if (dialogue.Entries.Count > 0)
            {
                var firstEntry = dialogue.Entries[0];
                var localizedText = firstEntry.GetText();
                if (localizedText != null)
                {
                    string fullText = localizedText.Value;
                    // Truncate if too long
                    if (fullText.Length > 80)
                    {
                        previewText = fullText.Substring(0, 77) + "...";
                    }
                    else
                    {
                        previewText = fullText;
                    }
                }
            }
        }
        catch
        {
            // Keep default preview text
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            previewText,
            panelRect.X + PADDING,
            textY,
            Color.White * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            0.8f
        );
        textY += 25;

        // Music indicator if present
        if (musicId.HasValue)
        {
            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                "♪ With music",
                panelRect.X + PADDING,
                textY,
                Color.LightBlue * opacity,
                Color.Black * opacity,
                Vector2.Zero,
                0.8f
            );
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            "Click to start",
            panelRect.X + 4,
            panelRect.Y + panelHeight + 5,
            Color.Yellow * opacity,
            Color.Black * opacity,
            default,
            0.8f
        );
    }

    private void HandleClick()
    {
        try
        {
            bool success = DialogueManager.Instance.StartDialogue(npcData, dialogueKey, lineCount, zoomIn, defaultDelay, defaultEmote, musicId, false, modifications);

            if (success)
            {
                SoundEngine.PlaySound(npcData.TalkSFX);
                IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error starting dialogue: {ex.Message}");
        }
    }
}