using Reverie.Core.Dialogue;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Core.Indicators;

public class DialogueIndicator : ScreenIndicator
{
    private readonly string dialogueKey;
    private readonly int lineCount;
    private readonly bool zoomIn;
    private readonly bool letterbox;
    private readonly string speakerName;

    private readonly Texture2D iconTexture;

    private const int PANEL_WIDTH = 280;
    private const int PADDING = 12;
    public override AnimationType AnimationStyle => AnimationType.Wag;

    public DialogueIndicator(Vector2 worldPosition, string dialogueKey, int lineCount,
        string speakerName = "Unknown", bool zoomIn = false, bool letterbox = true,
        AnimationType? animationType = null)
         : base(worldPosition, 40, 40, animationType)
    {
        this.dialogueKey = dialogueKey;
        this.lineCount = lineCount;
        this.zoomIn = zoomIn;
        this.letterbox = letterbox;
        this.speakerName = speakerName;

        iconTexture = TextureAssets.Chat2.Value;
        if (iconTexture != null)
        {
            Width = iconTexture.Width;
            Height = iconTexture.Height;
        }

        OnDrawWorld = DrawIndicator;
        OnClick += HandleClick;
    }

    public static DialogueIndicator CreateForNPC(NPC npc, string dialogueKey, int lineCount,
        bool zoomIn = false, bool letterbox = true, AnimationType? animationType = null)
    {
        string speakerName = (!string.IsNullOrEmpty(npc.GivenName) && npc.GivenName != npc.TypeName)
            ? npc.GivenName
            : npc.TypeName;

        var indicator = new DialogueIndicator(npc.Top, dialogueKey, lineCount, speakerName, zoomIn, letterbox, animationType);
        indicator.TrackEntity(npc, new Vector2(0, -50));
        return indicator;
    }

    protected override void PostUpdate()
    {
        if (DialogueManager.Instance.IsAnyActive())
        {
            IsActive = false;
        }
    }

    private void DrawIndicator(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        if (iconTexture == null)
            return;

        var scale = GetAnimationScale();
        var glowColor = IsHovering ? Color.LightBlue : Color.White * 0.9f;
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

    private void DrawPanel(SpriteBatch spriteBatch, float opacity)
    {
        // Get proper screen position for UI panel positioning
        var screenPos = GetScreenPosition();

        int lineCount = 3;
        bool hasMusic = TryGetMusicId(out _);
        if (hasMusic) lineCount++;

        int panelHeight = 20 + (lineCount * 22) + PADDING * 2;

        // Position panel to the right of the icon
        float panelX = screenPos.X + (Width / 2) + 15;
        float panelY = screenPos.Y - (panelHeight / 2);

        // Adjust if panel would go off screen
        if (panelX + PANEL_WIDTH > Main.screenWidth)
        {
            panelX = screenPos.X - (Width / 2) - PANEL_WIDTH - 15;
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

        var speakerColor = GetDisplayColor();
        int textY = panelRect.Y + PADDING;

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            speakerName,
            panelRect.X + PADDING,
            textY,
            speakerColor * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            1f
        );
        textY += 25;

        string previewText = GetPreview();

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

        if (hasMusic)
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
            textY += 20;
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            "Click to start",
            panelRect.X + PADDING,
            textY,
            Color.Yellow * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            0.8f
        );
    }

    private string GetPreview()
    {
        try
        {
            var firstLineData = DialogueBuilder.BuildLine(dialogueKey, "Line1");
            if (firstLineData != null && !string.IsNullOrEmpty(firstLineData.PlainText))
            {
                string fullText = firstLineData.PlainText;
                fullText = fullText.Replace("\n", " ").Replace("\r", "");
                if (fullText.Length > 60)
                {
                    return fullText.Substring(0, 57) + "...";
                }
                return fullText;
            }
        }
        catch
        {
            // Fall through to default
        }

        return "Talk to start";
    }

    private bool TryGetMusicId(out int musicId)
    {
        musicId = -1;
        try
        {
            var musicKey = $"DialogueLibrary.{dialogueKey}.Music";
            var musicLoc = Instance.GetLocalization(musicKey);
            if (!string.IsNullOrEmpty(musicLoc.Value) && int.TryParse(musicLoc.Value, out musicId))
            {
                return true;
            }
        }
        catch { }
        return false;
    }

    private Color GetDisplayColor()
    {
        return speakerName.ToLower() switch
        {
            "guide" => new Color(64, 109, 164),
            "player" => new Color(100, 150, 200),
            "you" => new Color(100, 150, 200),
            _ => new Color(180, 180, 180)
        };
    }

    private void HandleClick()
    {
        try
        {
            bool success = DialogueManager.Instance.StartDialogue(dialogueKey, lineCount, zoomIn, letterbox);

            if (success)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                IsActive = false;
            }
            else
            {
                Main.NewText("[ERROR] Failed to start dialogue", Color.Red);
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error starting dialogue: {ex.Message}");
            Main.NewText($"[ERROR] Dialogue error: {ex.Message}", Color.Red);
        }
    }
}