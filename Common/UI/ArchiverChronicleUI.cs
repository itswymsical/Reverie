using ReLogic.Content;
using Terraria.Audio;
using Terraria.UI;
using Terraria.GameContent;
using System.Text;
using System.Collections.Generic;
using Terraria.Localization;

namespace Reverie.Common.UI;

public class ArchiverChronicleUI : IInGameNotification
{
    #region Constants and Fields
    private const float ANIMATION_DURATION = 30f;
    private const float PAGE_HEIGHT = 520f;

    // Texture for the chronicle page
    public Texture2D page = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/ArchiverChroniclePage", AssetRequestMode.ImmediateLoad).Value;

    private bool isRemoved;
    private float animationProgress;

    private Vector2 targetPosition;
    private Vector2 startPosition;
    private int charDisplayTimer;
    private int charIndex;

    // Store the current text to display

    private string localizationKey = string.Empty;
    private readonly LocalizedText currentText;
    private int autoRemoveTimer;
    private const int AUTO_REMOVE_DELAY = 600;

    private int charDisplayDelay = 2;
    #endregion

    #region Properties
    public bool ShouldBeRemoved => isRemoved && animationProgress >= ANIMATION_DURATION;
    public float Opacity => isRemoved ? 1f - animationProgress / ANIMATION_DURATION : MathHelper.Clamp(animationProgress / ANIMATION_DURATION, 0f, 1f);
    public Color TextColor { get; init; } = Color.Wheat; // Default text color for the chronicle pages
    #endregion

    #region Constructors
    public ArchiverChronicleUI(string key)
    {
        localizationKey = key;
        currentText = Instance.GetLocalization(localizationKey);
        charIndex = 0;
    }
    #endregion

    public void Update()
    {
        UpdateAnimation();
        UpdateTextDisplay();
        UpdateAutoRemove();
    }

    private void UpdateAnimation()
    {
        if (!isRemoved || animationProgress < ANIMATION_DURATION)
        {
            animationProgress = Math.Min(animationProgress + 1f, ANIMATION_DURATION);
        }
    }

    private void UpdateTextDisplay()
    {
        if (charIndex < currentText.Value.Length)
        {
            charDisplayTimer++;
            if (charDisplayTimer >= charDisplayDelay)
            {
                charDisplayTimer = 0;
                charIndex++;
                SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}ChroniclePage_{Main.rand.Next(1, 3)}") { Volume = 0.11f, MaxInstances = 2 }, Main.LocalPlayer.position);
            }
        }
        else
        {
            if (autoRemoveTimer == 0)
            {
                autoRemoveTimer = AUTO_REMOVE_DELAY;
            }
        }
    }

    private void UpdateAutoRemove()
    {
        if (Main.gameMenu)
        {
            isRemoved = true;
            animationProgress = 0f;
        }

        if (autoRemoveTimer > 0)
        {
            autoRemoveTimer--;
            if (autoRemoveTimer == 0)
            {
                isRemoved = true;
                animationProgress = 0f;
            }
        }
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (Opacity <= 0f || Main.gameMenu) return;

        SetupPositions(bottomAnchorPosition);

        DrawPage(spriteBatch);
        DrawText(spriteBatch);
    }

    private void SetupPositions(Vector2 bottomAnchorPosition)
    {
        if (targetPosition == Vector2.Zero)
        {
            targetPosition = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            startPosition = targetPosition + new Vector2(0f, PAGE_HEIGHT);
        }
    }

    private void DrawPage(SpriteBatch spriteBatch)
    {
        var t = isRemoved ? animationProgress / ANIMATION_DURATION : 1f - animationProgress / ANIMATION_DURATION;
        var currentPosition = Vector2.Lerp(targetPosition, startPosition, t);

        Vector2 pageSize = new Vector2(page.Width, page.Height);
        Vector2 pagePosition = currentPosition - pageSize / 2;

        spriteBatch.Draw(page, pagePosition, null, Color.White * Opacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        var pageRect = new Rectangle((int)pagePosition.X, (int)pagePosition.Y, (int)pageSize.X, (int)pageSize.Y);
        if (pageRect.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && Main.mouseLeftRelease)
        {
            Main.mouseLeftRelease = false;

            if (charIndex < currentText.Value.Length)
            {
                charIndex = currentText.Value.Length;
            }
            else
            {
                isRemoved = true;
                animationProgress = 0f;
            }
        }
    }

    private void DrawText(SpriteBatch spriteBatch)
    {
        var t = isRemoved ? animationProgress / ANIMATION_DURATION : 1f - animationProgress / ANIMATION_DURATION;
        var currentPosition = Vector2.Lerp(targetPosition, startPosition, t);

        Vector2 pageSize = new Vector2(page.Width * 1.5f, page.Height);
        Vector2 pagePosition = currentPosition - pageSize / 2;

        float textMarginX = 140f;
        float textMarginY = 50f;
        Vector2 textAreaStart = pagePosition + new Vector2(textMarginX, textMarginY);
        float textAreaWidth = pageSize.X / 1.25f;

        string displayText = currentText.Value.Substring(0, Math.Min(charIndex, currentText.Value.Length));

        string[] wrappedText = WrapText(displayText, textAreaWidth);

        for (int i = 0; i < wrappedText.Length; i++)
        {
            Vector2 linePosition = textAreaStart + new Vector2(0f, i * 17.5f);
            Utils.DrawBorderString(spriteBatch, wrappedText[i], linePosition, TextColor * Opacity, 0.65f);
        }
    }

    private static string[] WrapText(string text, float maxWidth)
    {
        List<string> lines = new List<string>();
        StringBuilder currentLine = new StringBuilder();
        var spaceWidth = FontAssets.MouseText.Value.MeasureString(" ").X;

        foreach (var word in text.Split(' '))
        {
            var wordWidth = FontAssets.MouseText.Value.MeasureString(word).X;
            if (currentLine.Length > 0 && FontAssets.MouseText.Value.MeasureString(currentLine.ToString()).X + wordWidth + spaceWidth > maxWidth)
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
            }
            if (currentLine.Length > 0)
                currentLine.Append(" ");
            currentLine.Append(word);
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString());

        return lines.ToArray();
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom) => positionAnchorBottom.Y -= Main.screenHeight / 2;

    public static void ShowChronicle(string localizationKey)
    {
        var notification = new ArchiverChronicleUI(localizationKey);
        InGameNotificationsTracker.AddNotification(notification);
    }
}