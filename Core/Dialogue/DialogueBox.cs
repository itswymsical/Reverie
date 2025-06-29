using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reverie.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

namespace Reverie.Core.Dialogue;

public class DialogueBox : IInGameNotification
{
    #region Constants and Fields
    private const float ANIMATION_DURATION = 30f;
    private const float PANEL_WIDTH = 420f;
    private const int AUTO_REMOVE_DELAY = 200;

    private bool isRemoved;
    private float animationProgress;
    private int autoRemoveTimer;
    private bool isLastDialogue;
    private Vector2 targetPosition;
    private Vector2 startPosition;

    // Text display state
    private int charDisplayTimer;
    private int charIndex;
    private float pauseTimer;

    // Dialogue state - simplified to use DialogueData
    private readonly Queue<DialogueData> dialogueQueue = new();
    private DialogueData currentDialogue;
    private int currentLineIndex;
    #endregion

    #region Properties
    public bool ShouldZoom { get; set; }
    public bool ShouldBeRemoved => isRemoved && animationProgress >= ANIMATION_DURATION;
    public float Opacity => isRemoved ? 1f - animationProgress / ANIMATION_DURATION : MathHelper.Clamp(animationProgress / ANIMATION_DURATION, 0f, 1f);

    private static Texture2D ArrowTexture
    {
        get
        {
            try
            {
                return ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/ArrowForward").Value;
            }
            catch (Exception ex)
            {
                Main.NewText($"[ERROR] Failed to load ArrowTexture: {ex.Message}", Color.Red);
                return TextureAssets.MagicPixel.Value;
            }
        }
    }

    private static Texture2D PortraitFrameTexture
    {
        get
        {
            try
            {
                return ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/PortraitFrame").Value;
            }
            catch (Exception ex)
            {
                Main.NewText($"[ERROR] Failed to load PortraitFrameTexture: {ex.Message}", Color.Red);
                return TextureAssets.MagicPixel.Value;
            }
        }
    }
    #endregion

    #region Public Methods
    public static DialogueBox Create(List<DialogueData> dialogueLines, bool zoomIn)
    {
        var box = new DialogueBox
        {
            ShouldZoom = zoomIn
        };

        // Queue all dialogue lines
        foreach (var line in dialogueLines)
        {
            box.dialogueQueue.Enqueue(line);
        }

        // Start first line
        box.NextLine();

        return box;
    }

    public void Update()
    {
        UpdateAnimation();
        UpdateTextDisplay();
        UpdateAutoRemove();
        HandleInput();
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (Opacity <= 0f || Main.gameMenu || currentDialogue == null) return;

        SetupPositions(bottomAnchorPosition);

        DrawPanel(spriteBatch);
        DrawText(spriteBatch);
        DrawArrow(spriteBatch);
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom) => positionAnchorBottom.Y -= 50f * Opacity;

    public void ForceComplete()
    {
        if (currentDialogue != null)
        {
            charIndex = currentDialogue.PlainText.Length;
            pauseTimer = 0f;
        }
    }

    public void Close()
    {
        isRemoved = true;
        animationProgress = 0f;
    }
    #endregion

    #region Private Methods
    private void NextLine()
    {
        if (dialogueQueue.Count > 0)
        {
            currentDialogue = dialogueQueue.Dequeue();
            charIndex = 0;
            autoRemoveTimer = 0;
            pauseTimer = 0f;
            currentLineIndex++;
        }
        else
        {
            isRemoved = true;
            isLastDialogue = true;
            animationProgress = 0f;
        }
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
        if (Main.gameMenu || currentDialogue == null)
        {
            Close();
            return;
        }

        if (pauseTimer > 0)
        {
            pauseTimer--;
            return;
        }

        if (charIndex < currentDialogue.PlainText.Length)
        {
            // Check for effect at current position
            if (currentDialogue.Effects.TryGetValue(charIndex, out var effect))
            {
                ApplyEffect(effect);
            }

            charDisplayTimer++;
            var currentDelay = (int)(currentDialogue.BaseDelay / currentDialogue.Speed);

            if (charDisplayTimer >= currentDelay)
            {
                charDisplayTimer = 0;
                charIndex++;

                if (charIndex <= currentDialogue.PlainText.Length)
                {
                    SoundEngine.PlaySound(currentDialogue.TypeSound, Main.LocalPlayer.position);
                }
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

    private void ApplyEffect(DialogueEffect effect)
    {
        switch (effect.Type)
        {
            case EffectType.Pause:
                pauseTimer = effect.Duration;
                break;
            case EffectType.Sound:
                if (effect.Sound != null)
                    SoundEngine.PlaySound(effect.Sound.Value, Main.LocalPlayer.position);
                break;
        }
    }

    private void UpdateAutoRemove()
    {
        if (autoRemoveTimer > 0)
        {
            autoRemoveTimer--;
            if (autoRemoveTimer == 0)
            {
                NextLine();
            }
        }
    }

    private void HandleInput()
    {
        if (PlayerInput.Triggers.Current.Jump || PlayerInput.Triggers.Current.Inventory)
        {
            if (currentDialogue != null && charIndex < currentDialogue.PlainText.Length)
            {
                ForceComplete();
            }
            else if (dialogueQueue.Count > 0)
            {
                NextLine();
            }
            else
            {
                Close();
            }
        }
    }

    private void SetupPositions(Vector2 bottomAnchorPosition)
    {
        if (targetPosition == Vector2.Zero)
        {
            var panelSize = CalculatePanelSize();
            targetPosition = bottomAnchorPosition + new Vector2(0f, -panelSize.Y * 0.5f);
            startPosition = targetPosition + new Vector2(0f, panelSize.Y + 70f);
        }
    }

    private void DrawPanel(SpriteBatch spriteBatch)
    {
        var panelSize = CalculatePanelSize();
        var panelPosition = CalculatePanelPosition(panelSize);
        var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

        var isHovering = panelRectangle.Contains(Main.MouseScreen.ToPoint());
        var panelColor = currentDialogue.BackgroundColor * (isHovering ? 0.75f : 0.5f) * Opacity;

        DrawUtils.DrawPanel(spriteBatch, panelRectangle, panelColor);

        if (isHovering) OnMouseOver();

        DrawSpeakerName(spriteBatch, panelRectangle);
    }

    private void DrawSpeakerName(SpriteBatch spriteBatch, Rectangle panelRectangle)
    {
        var speakerName = currentDialogue.Speaker;
        var namePosition = new Vector2(panelRectangle.X + 10f, panelRectangle.Y - 25f);

        Utils.DrawBorderString(spriteBatch, speakerName, namePosition, currentDialogue.SpeakerColor * Opacity, 1f, anchorx: 0f, anchory: 0.5f);
    }

    private void DrawText(SpriteBatch spriteBatch)
    {
        var panelSize = CalculatePanelSize();
        var panelPosition = CalculatePanelPosition(panelSize);
        var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

        var displayText = currentDialogue.PlainText[..Math.Min(charIndex, currentDialogue.PlainText.Length)];
        var textPosition = panelRectangle.TopLeft() + new Vector2(10f, 20f);
        var wrappedText = WrapText(displayText, PANEL_WIDTH - 20f);

        int globalCharIndex = 0;

        for (var i = 0; i < wrappedText.Length; i++)
        {
            var linePos = textPosition + new Vector2(0f, i * FontAssets.ItemStack.Value.LineSpacing);
            var charOffset = 0;

            foreach (char c in wrappedText[i])
            {
                var charPos = linePos + new Vector2(charOffset, 0f);

                // Apply effects if present
                if (currentDialogue.Effects.ContainsKey(globalCharIndex))
                {
                    var effect = currentDialogue.Effects[globalCharIndex];

                    if (effect.Type == EffectType.Shake)
                    {
                        var time = Main.GameUpdateCount * 0.8f;
                        var shakeOffset = new Vector2(
                            (float)(Math.Sin(time + globalCharIndex * 1.2f) * effect.Intensity +
                                    Math.Sin(time * 1.7f + globalCharIndex * 0.8f) * effect.Intensity * 0.6f),
                            (float)(Math.Cos(time * 1.3f + globalCharIndex * 0.9f) * effect.Intensity * 0.8f +
                                    Math.Sin(time * 2.1f + globalCharIndex * 0.7f) * effect.Intensity * 0.4f)
                        );
                        charPos += shakeOffset;
                    }
                    else if (effect.Type == EffectType.Sine)
                    {
                        var time = Main.GlobalTimeWrappedHourly * 3f;
                        var amplitude = effect.Intensity * 1.5f;
                        var sineOffset = new Vector2(0f, (float)Math.Sin(time + globalCharIndex * 0.5f) * amplitude);
                        charPos += sineOffset;
                    }
                }

                Utils.DrawBorderString(spriteBatch, c.ToString(), charPos, currentDialogue.TextColor * Opacity, .9f, anchorx: 0f, anchory: 0.5f);
                charOffset += (int)FontAssets.ItemStack.Value.MeasureString(c.ToString()).X;
                globalCharIndex++;
            }
        }
    }

    private void DrawArrow(SpriteBatch spriteBatch)
    {
        if (currentDialogue == null || charIndex < currentDialogue.PlainText.Length || (dialogueQueue.Count == 0 && isLastDialogue))
            return;

        var panelSize = CalculatePanelSize();
        var panelPosition = CalculatePanelPosition(panelSize);
        var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);


        var arrowPosition = panelRectangle.BottomRight() - new Vector2(30, 20);
        var yOffset = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 3f;
        arrowPosition.Y += yOffset;

        spriteBatch.Draw(ArrowTexture, arrowPosition, null, Color.White * Opacity, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
    }

    private void OnMouseOver()
    {
        if (PlayerInput.IgnoreMouseInterface) return;

        Main.LocalPlayer.mouseInterface = true;

        if (!Main.mouseLeft || !Main.mouseLeftRelease || (isLastDialogue && isRemoved)) return;

        Main.mouseLeftRelease = false;

        if (currentDialogue != null && charIndex < currentDialogue.PlainText.Length)
        {
            ForceComplete();
        }
        else if (dialogueQueue.Count > 0)
        {
            NextLine();
        }
        else
        {
            Close();
        }
    }

    private Vector2 CalculatePanelSize()
    {
        if (currentDialogue == null) return new Vector2(PANEL_WIDTH + 18, 100);

        var displayText = currentDialogue.PlainText[..Math.Min(charIndex, currentDialogue.PlainText.Length)];
        var wrappedText = WrapText(displayText, PANEL_WIDTH - 20f);
        var lineCount = wrappedText.Length;
        var textHeight = FontAssets.ItemStack.Value.LineSpacing * lineCount;

        var panelHeight = textHeight + 40f;
        return new Vector2(PANEL_WIDTH + 18, Math.Max(panelHeight, 80f));
    }

    private Vector2 CalculatePanelPosition(Vector2 panelSize)
    {
        var t = isRemoved ? animationProgress / ANIMATION_DURATION : 1f - animationProgress / ANIMATION_DURATION;
        var currentPosition = Vector2.Lerp(targetPosition, startPosition, t);
        return currentPosition - new Vector2(0f, panelSize.Y * 0.4f);
    }

    private static string[] WrapText(string text, float maxWidth)
    {
        List<string> lines = [];
        StringBuilder currentLine = new();
        var spaceWidth = FontAssets.ItemStack.Value.MeasureString(" ").X;

        foreach (var word in text.Split(' '))
        {
            var wordWidth = FontAssets.ItemStack.Value.MeasureString(word).X;
            if (currentLine.Length > 0 && FontAssets.ItemStack.Value.MeasureString(currentLine.ToString()).X + wordWidth + spaceWidth > maxWidth)
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

        return [.. lines];
    }
    #endregion
}