using ReLogic.Content;
using Reverie.Common.Systems;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

namespace Reverie.Core.Dialogue;

public class DialogueBox : IInGameNotification
{
    #region Constants and Fields
    private const float ANIMATION_DURATION = 30f;
    private const float PANEL_WIDTH = 480f;
    private const int AUTO_REMOVE_DELAY = 200;
    private const string UI_ASSET_DIRECTORY = "Reverie/Assets/Textures/UI/";

    private bool isRemoved;
    private float animationProgress;

    private bool isLastDialogue;
    private Vector2 targetPosition;
    private Vector2 startPosition;

    private int charDisplayTimer;
    private int charIndex;
    private float pauseTimer;

    private readonly string dialogueKey;
    private readonly Queue<string> lineKeys = new();
    private DialogueData currentDialogue = null;
    private int currentLineIndex;

    private float currentPitchModifier = 1.0f;

    private static Texture2D ArrowTexture => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Missions/Next", AssetRequestMode.ImmediateLoad).Value;
    private static Texture2D PortraitFrameTexture => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/PortraitFrame", AssetRequestMode.ImmediateLoad).Value;
    private static Texture2D PortraitFrameInnerTexture => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/PortraitFrame_Inner", AssetRequestMode.ImmediateLoad).Value;

    public string DialogueKey { get; private set; }

    #endregion

    #region Properties
    public bool ShouldZoom { get; set; }
    public bool ShouldBeRemoved => isRemoved && animationProgress >= ANIMATION_DURATION;
    public float Opacity => isRemoved ? 1f - animationProgress / ANIMATION_DURATION : MathHelper.Clamp(animationProgress / ANIMATION_DURATION, 0f, 1f);
    #endregion

    #region Public Methods
    public static DialogueBox CreateDialogue(string dialogueKey, List<string> lineKeys, bool zoomIn)
    {
        var box = new DialogueBox(dialogueKey)
        {
            ShouldZoom = zoomIn
        };

        foreach (var lineKey in lineKeys)
        {
            box.lineKeys.Enqueue(lineKey);
        }

        box.NextLine();
        return box;
    }

    private DialogueBox(string dialogueKey)
    {
        this.dialogueKey = dialogueKey;
        this.DialogueKey = dialogueKey;
    }

    public void Update()
    {
        UpdateAnimation();
        UpdateTextDisplay();
        HandleInput();
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (Opacity <= 0f || Main.gameMenu || currentDialogue == null) return;

        SetupPositions(bottomAnchorPosition);

        DrawPanel(spriteBatch);
        DrawPortrait(spriteBatch);
        DrawText(spriteBatch);
        DrawSkip(spriteBatch);
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
        if (!isRemoved)
        {
            isRemoved = true;
            animationProgress = 0f;
        }
    }

    #endregion

    #region Core Logic
    private void NextLine()
    {
        if (lineKeys.Count > 0)
        {
            var lineKey = lineKeys.Dequeue();

            currentDialogue = DialogueBuilder.BuildLine(dialogueKey, lineKey);

            if (currentDialogue == null)
            {
                Main.NewText($"[ERROR] Failed to parse line: {lineKey}", Color.Red);
                // Try next line or end dialogue
                if (lineKeys.Count > 0)
                {
                    NextLine();
                    return;
                }
                else
                {
                    isRemoved = true;
                    return;
                }
            }


            charIndex = 0;

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
                    var sound = currentDialogue.TypeSound;
                    if (currentPitchModifier != 1.0f)
                    {
                        sound = sound with { Pitch = currentPitchModifier - 1.0f };
                    }

                    SoundEngine.PlaySound(sound, Main.LocalPlayer.position);

                    // Reset pitch modifier after playing (or keep it for continuous effect)
                    // currentPitchModifier = 1.0f; // Uncomment to reset after each character
                }
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
            case EffectType.Pitch:
                // Store the pitch modifier for the next character sound
                currentPitchModifier = effect.PitchModifier;
                break;
        }
    }

    private void HandleInput()
    {
        if (isRemoved) return;

        if (ReverieSystem.FFDialogueKeybind.JustPressed)
        {
            if (currentDialogue != null && charIndex < currentDialogue.PlainText.Length)
            {
                ForceComplete();
            }
            else if (lineKeys.Count > 0)
            {
                NextLine();
            }
            else
            {
                Close();
            }
        }
    }

    private void OnMouseOver()
    {
        if (PlayerInput.IgnoreMouseInterface) return;

        Main.LocalPlayer.mouseInterface = true;

        if (!Main.mouseLeft || !Main.mouseLeftRelease || isLastDialogue && isRemoved) return;

        Main.mouseLeftRelease = false;

        if (isRemoved) return;

        if (currentDialogue != null && charIndex < currentDialogue.PlainText.Length)
        {
            ForceComplete();
        }
        else if (lineKeys.Count > 0)
        {
            NextLine();
        }
        else
        {
            Close();
        }
    }
    #endregion

    #region Drawing
    private void SetupPositions(Vector2 bottomAnchorPosition)
    {
        if (targetPosition == Vector2.Zero)
        {
            var panelSize = CalculateSize();
            targetPosition = bottomAnchorPosition + new Vector2(0f, -panelSize.Y * 0.5f);
            startPosition = targetPosition + new Vector2(0f, panelSize.Y + 70f);
        }
    }

    private void DrawPanel(SpriteBatch spriteBatch)
    {
        var panelSize = CalculateSize();
        var panelPosition = CalculatePosition(panelSize);
        var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

        var isHovering = panelRectangle.Contains(Main.MouseScreen.ToPoint());
        var panelColor = currentDialogue.SpeakerColor * (isHovering ? 0.85f : 0.7f) * Opacity;

        DrawUtils.DrawPanel(spriteBatch, panelRectangle, panelColor);

        if (isHovering) OnMouseOver();
    }

    private void DrawPortrait(SpriteBatch spriteBatch)
    {
        var panelSize = CalculateSize();
        var panelPosition = CalculatePosition(panelSize);
        var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

        var isHovering = panelRectangle.Contains(Main.MouseScreen.ToPoint());
        var panelColor = currentDialogue.SpeakerColor * (isHovering ? 0.85f : 0.7f) * Opacity;

        Vector2 iconSize = new(92, 92);

        Vector2 iconPosition = new((panelRectangle.Left - iconSize.X) + 5, panelRectangle.Bottom - iconSize.Y - 7);


        var portraitTexture = GetPortrait(currentDialogue.SpeakerType);

        Vector2 frameSize = new(PortraitFrameTexture.Width, PortraitFrameTexture.Height);
        var framePosition = iconPosition - (frameSize - iconSize) / 2;

        if (currentDialogue.SpeakerType != "You")
        {
            spriteBatch.Draw(PortraitFrameInnerTexture, framePosition, null, panelColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (portraitTexture != null)
            {
                var sourceRect = GetFrameRect(currentDialogue.SpeakerType, currentDialogue.Emote, 92, 92);
                spriteBatch.Draw(portraitTexture.Value, new(iconPosition.X - 13, iconPosition.Y + 1), sourceRect, Color.White * Opacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(PortraitFrameTexture, framePosition, null, Color.White * Opacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            DrawSpeakerName(spriteBatch, iconPosition, iconSize);
        }
    }

    private void DrawSpeakerName(SpriteBatch spriteBatch, Vector2 iconPosition, Vector2 iconSize)
    {
        var speakerName = currentDialogue.Speaker;
        var nameTextSize = FontAssets.ItemStack.Value.MeasureString(speakerName);
        Vector2 nameTextPosition = new((iconPosition.X + iconSize.X / 2 - nameTextSize.X / 2) - 18, iconPosition.Y + iconSize.Y + 18f);

        Utils.DrawBorderString(spriteBatch, speakerName, nameTextPosition, Color.White * Opacity, 1f, anchorx: 0f, anchory: 0.5f);
    }

    private void DrawText(SpriteBatch spriteBatch)
    {
        var panelSize = CalculateSize();
        var panelPosition = CalculatePosition(panelSize);
        var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

        var displayText = currentDialogue.PlainText[..Math.Min(charIndex, currentDialogue.PlainText.Length)];
        var textPosition = panelRectangle.TopLeft() + new Vector2(15f, 32f);
        var wrappedText = WrapText(displayText, PANEL_WIDTH - 90f);

        var globalCharIndex = 0;

        for (var i = 0; i < wrappedText.Length; i++)
        {
            var linePos = textPosition + new Vector2(2f, i * FontAssets.ItemStack.Value.LineSpacing);
            var charOffset = 0;

            foreach (var c in wrappedText[i])
            {
                var charPos = linePos + new Vector2(charOffset, 0f);

                // Apply text effects (shake, sine wave, etc.)
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

                Utils.DrawBorderString(spriteBatch, c.ToString(), charPos, currentDialogue.TextColor * Opacity, .98f, anchorx: 0f, anchory: 0.3f);
                charOffset += (int)FontAssets.ItemStack.Value.MeasureString(c.ToString()).X + 1;
                globalCharIndex++;
            }
        }
    }

    /// <summary>
    /// Draws the "skip to next" line
    /// </summary>
    /// <param name="spriteBatch"></param>
    private void DrawSkip(SpriteBatch spriteBatch)
    {
        if (currentDialogue == null || charIndex < currentDialogue.PlainText.Length || lineKeys.Count == 0 && isLastDialogue)
            return;

        var panelSize = CalculateSize();
        var panelPosition = CalculatePosition(panelSize);
        var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

        var arrowScale = 0.65f;
        var arrowPosition = panelRectangle.BottomRight() - new Vector2(ArrowTexture.Width * arrowScale + 10, ArrowTexture.Height * arrowScale + 20);

        var yOffset = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 3f;
        arrowPosition.Y += yOffset;

        Utils.DrawBorderString(spriteBatch, $"{ReverieSystem.FFDialogueKeybind.GetAssignedKeys().FirstOrDefault() ?? "[None]"}", new(arrowPosition.X - 20, arrowPosition.Y + 12), Color.White * Opacity, 0.75f, anchorx: 0f, anchory: 0.5f);

        spriteBatch.Draw(ArrowTexture, arrowPosition, null, Color.White * Opacity, 0f, Vector2.Zero, arrowScale, SpriteEffects.None, 0f);
    }

    #endregion

    #region Helper Methods
    private Asset<Texture2D> GetPortrait(string speakerName)
    {
        return speakerName.ToLower() switch
        {
            "guide" => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Guide"),
            "merchant" => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Merchant"),
            "nurse" => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Nurse"),
            "demolitionist" => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Demolitionist"),
            "goblin tinkerer" => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Goblin"),
            "sophie" => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Sophie"),
            "fungore" => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/FungoreReborn"),
            "dalia" => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Dalia"),
            "eustace" => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Eustace"),
            "mechanic" => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Mechanic"),
            "argie" => ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Argie"),
            _ => null
        };
    }

    private int GetFrameCount(string speakerName)
    {
        return speakerName.ToLower() switch
        {
            "guide" => 7,
            "player" or "you" => 1,
            "merchant" => 3,
            "nurse" => 2,
            "demolitionist" => 2,
            "goblin tinkerer" => 2,
            "mechanic" => 2,
            "argie" => 1,
            _ => 1
        };
    }

    private Rectangle GetFrameRect(string speakerName, int emoteFrame, int frameWidth, int frameHeight)
    {
        var frameCount = GetFrameCount(speakerName);
        var safeFrame = Math.Max(0, Math.Min(emoteFrame, frameCount - 1));
        return new Rectangle(0, safeFrame * frameHeight, frameWidth, frameHeight);
    }

    private Vector2 CalculateSize()
    {
        if (currentDialogue == null) return new Vector2(PANEL_WIDTH + 18, 120);

        var displayText = currentDialogue.PlainText[..Math.Min(charIndex, currentDialogue.PlainText.Length)];
        var wrappedText = WrapText(displayText, PANEL_WIDTH - 90f);
        var lineCount = wrappedText.Length;
        var textHeight = FontAssets.ItemStack.Value.LineSpacing * lineCount;

        var panelHeight = textHeight + 52f;
        return new Vector2(PANEL_WIDTH, Math.Max(panelHeight, 120));
    }

    private Vector2 CalculatePosition(Vector2 panelSize)
    {
        var t = isRemoved ? animationProgress / ANIMATION_DURATION : 1f - animationProgress / ANIMATION_DURATION;
        var currentPosition = Vector2.Lerp(targetPosition, startPosition, t);
        if (currentDialogue.SpeakerType == "You")
        {
            return currentPosition - new Vector2(0f, panelSize.Y * 0.65f);

        }
        return currentPosition - new Vector2(-32f, panelSize.Y * 0.65f);
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